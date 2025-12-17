using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager instance { get; private set; }
    public const int MAX_PLAYERS = 3;
    public const int MIN_TIME = 5;
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_ICON = "PlayerIcon";
    public const string KEY_START_GAME = "Filler";
    public const string KEY_RELAY_CONNECTED = "Filler";

    public const string GAME_SCENE_NAME = "EnsambleMapa";
    [SerializeField] List<Sprite> iconList;

    private int iconChosen = -1;
    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyTimer;

    public event Action<Lobby> OnLobbyJoined;
    public event Action<Lobby> OnLobbyJoinedUpdate;
    public event Action<string, int> OnAuthenticated;
    public event Action<List<Lobby>> OnLobbyListChanged;
    public event Action OnLobbyLeft;
    public event Action OnLobbyKicked;
    private string playName;

    private bool inGame;
    private bool loadEventTriggered;

    private HashSet<ulong> clientsLoaded = new();

    private Lobby lobby;
    private void Awake()
    {

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
   /* private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        GameManagerMultiplayer multiplayer = GameObject.FindFirstObjectByType<GameManagerMultiplayer>();

        Debug.Log("El evento ha sido triggereado" + multiplayer.gameObject.name + " es el objeto multiplayer");

        if (multiplayer != null)
        {
            Debug.Log("Hemos entrado en el if del evento");
            multiplayer.CreacionPlayers();
        }
    }*/

    
    private void Update()
    {
        if (inGame) return;
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
        //HandleRefreshLobbyList(); Desactivarlo solo para el testeo cuando este yo solo
    }
    public async void Authenticate(string playerName, int iconInt)
    {
        playName = playerName;
        iconChosen = iconInt;
        await UnityServices.InitializeAsync();
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            OnAuthenticated?.Invoke(playerName, iconChosen);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }




    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        Player player = GetPlayer();
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = player,
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };
            Lobby myLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, createLobbyOptions);
            lobby = myLobby;

            OnLobbyJoined?.Invoke(lobby);
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();
        try
        {
            if (lobby.HasPassword)
            {
                //PANTALLA PARA QUE META LA CONTRASEÑA
                return;
            }

           this.lobby =  await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
           {
               Player = player
           });
            OnLobbyJoined.Invoke(lobby);
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);

        }
    }

    private async void HandleLobbyHeartbeat()
    {
        try
        {
            if (IsLobbyHost())
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0)
                {
                    float maxTimer = 15f;
                    heartbeatTimer = maxTimer;

                    await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            Debug.Log("En el heartbeat");
        }

    }

    private async void HandleLobbyPolling()
    {
        try
        {
            if (lobby != null)
            {
                lobbyPollTimer -= Time.deltaTime;
                if (lobbyPollTimer < 0)
                {
                    float lobbyPollTimerMax = 2f;
                    lobbyPollTimer = lobbyPollTimerMax;
                    lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

                    OnLobbyJoinedUpdate?.Invoke(lobby);
                    if (!IsPlayerInLobby())
                    {
                        //Player was kicked

                        lobby = null;
                        OnLobbyKicked?.Invoke();
                    }

                    if (lobby.Data[KEY_START_GAME].Value != "0")
                    {
                        if (!IsLobbyHost() && !inGame)
                        {
                            await JoinRelay(lobby.Data[KEY_START_GAME].Value);
                        }
                    }

                   /* if (IsLobbyHost())
                    {
                        int connectedPlayers = 0;
                        foreach(Player player in lobby.Players)
                        {
                            if (player.Data[KEY_RELAY_CONNECTED].Value == "1")
                            {
                                connectedPlayers++;
                            }
                        }

                        if(connectedPlayers == lobby.MaxPlayers - 1)
                        {
                            //ChangeScene();
                            StartGame();
                        }
                    }*/
                }
            }
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log("No deberia pasar nada, simplemente intenta acceder a un lobby que ya no existe " + e);
            }

        }
     
    }

    private void HandleRefreshLobbyList()
    {
        if(UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            refreshLobbyTimer -= Time.deltaTime;
            if(refreshLobbyTimer < 0)
            {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyTimer = refreshLobbyListTimerMax;
                LobbiesList();
            }
        }
    }

    public bool IsLobbyHost()
    {
        return lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    private bool IsPlayerInLobby()
    {
        if(lobby != null && lobby.Players != null)
        {
            foreach(Player player in lobby.Players)
            {
                if(player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true; //Player IS in lobby
                }
            }
        }

        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            {   KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playName)},
            {   KEY_PLAYER_ICON, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, iconChosen.ToString()) },
            {   KEY_RELAY_CONNECTED, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
        });
    }

    public void RefreshLobbyListOnEnter()
    {
        LobbiesList();
    }

    //Esto habra que hacerlo luego en el update con un countdown
    public async void LobbiesList()
    {
        try
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions();

            queryOptions.Count = 25;
            queryOptions.Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            };
            queryOptions.Order = new List<QueryOrder>
            {
                new(asc: false, field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            OnLobbyListChanged?.Invoke(queryResponse.Results);

        }

        catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        if(lobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);

                lobby = null;

                OnLobbyLeft?.Invoke();
            } catch (LobbyServiceException e) 
            {
                    Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerID)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerID);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async Task ComenzarPartida()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Comenzamos partida por fin");

                string relayCode = await CreateRelay();

                Lobby myLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });
                lobby = myLobby;
            }

            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }


        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); //Este 3 es el numero de jugadores maximos sin contar el host

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

           /* NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);
           */
            NetworkManager.Singleton.StartHost();
            inGame = true;
            //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

            //NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent; << //Desactivado para la escena unica 01/12
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if(sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {

            clientsLoaded.Add(sceneEvent.ClientId);

            if (clientsLoaded.SetEquals(NetworkManager.Singleton.ConnectedClientsIds) && NetworkManager.Singleton.ConnectedClientsIds.Count == MAX_PLAYERS)
            {
                GameManagerMultiplayer multiplayer = FindFirstObjectByType<GameManagerMultiplayer>();

                Debug.Log("El evento ha sido triggereado" + multiplayer.gameObject.name + " es el objeto multiplayer");

                if (multiplayer != null && !loadEventTriggered)
                {
                    loadEventTriggered = true;
                    Debug.Log("Hemos entrado en el if del evento");
                    multiplayer.CreacionPlayers();
                }
            }
           
        }
     
    }

    private async Task JoinRelay(string joinCode)
    {
        try
        {

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
            
            /*NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);*/
            NetworkManager.Singleton.StartClient();
            inGame = true;
            //UpdatePlayerConecctionStatus();


        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerConecctionStatus()
    {
        if(lobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new();

                options.Data = new Dictionary<string, PlayerDataObject>
                {
                    { KEY_RELAY_CONNECTED, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "1") }
                };

                string playerID = AuthenticationService.Instance.PlayerId;

                Lobby thisLobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, playerID, options);

                lobby = thisLobby;

                inGame = true;


                OnLobbyJoinedUpdate?.Invoke(lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private void ChangeScene()
    {
        Debug.Log("Hemos llegado hasta aqui");
        inGame = true;
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(MIN_TIME);
    }
}
