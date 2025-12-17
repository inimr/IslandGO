using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    public static LobbyUI Instance { get; private set; }

    [SerializeField] GameObject panelAutentificacion;
    [SerializeField] GameObject panelLobbyList;
    [SerializeField] GameObject panelCreacionLobby;
    [SerializeField] GameObject panelLobby;
    [SerializeField] TMP_InputField createLobbyNameIF;
    [SerializeField] Button botonEleccionNivelProteccion;
    [SerializeField] Button botonCreacionLobby;
    [SerializeField] Button botonComenzarPartida;

    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject canvasLobby;
    private bool isPrivate = true;

    private Sprite playerIcon;
    private string playerName;
    private int iconInt;
    [SerializeField] List<Sprite> listSprite;


    [Header("Variables necesarias para crear lobbies y los players dentro de cada uno")]
    [SerializeField] Transform containerLobbyList;
    [SerializeField] Transform containerLobbyPlayers;

    [SerializeField] GameObject prefabSinglePlayer;
    [SerializeField] GameObject prefabSingleLobby;

    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] TextMeshProUGUI lobbyPlayersText;

    


    private void Awake()
    {
        Instance = this;

        panelAutentificacion.SetActive(true);
    }
    private void Start()
    {
        LobbyManager.instance.OnLobbyJoined += LobbyManager_OnLobbyJoined;
        LobbyManager.instance.OnAuthenticated += LobbyManager_OnAuthenticated;
        LobbyManager.instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.instance.OnLobbyLeft += LobbyManager_OnLobbyLeft;
        LobbyManager.instance.OnLobbyJoinedUpdate += LobbyManager_OnLobbyJoinedUpdate;
        LobbyManager.instance.OnLobbyKicked += LobbyManager_OnLobbyLeft;
    }

    private void LobbyManager_OnLobbyJoinedUpdate(Lobby lobby)
    {
        UpdateLobby(lobby);
    }

    private void LobbyManager_OnLobbyLeft()
    {
        ClearLobbyPlayers();
        Debug.Log("Lobby left");
        panelLobby.SetActive(false);
        panelLobbyList.SetActive(true); 
    }

    private void LobbyManager_OnLobbyListChanged(List<Lobby> list)
    {
        UpdateLobbyList(list);
    }

    private void LobbyManager_OnAuthenticated(string arg1, int arg2)
    {
        iconInt = arg2;
        playerName = arg1;
        playerIcon = listSprite[iconInt];
    }

    private void LobbyManager_OnLobbyJoined(Lobby lobby)
    {
        UpdateLobby(lobby);
        panelLobbyList.SetActive(false);
        panelLobby.SetActive(true);

    }

    public void ActivarPanelCreacionLobby()
    {
        panelCreacionLobby.SetActive(true);
    }

    public void CambiarValorPrivate()
    {
        isPrivate = !isPrivate;

        botonEleccionNivelProteccion.GetComponentInChildren<TextMeshProUGUI>().text = isPrivate ? "Privado" : "Público";
    }

    public void CrearLobby()
    {
        if (createLobbyNameIF.text == null) return;

        LobbyManager.instance.CreateLobby(createLobbyNameIF.text, isPrivate);

        panelCreacionLobby.SetActive(false);
    }

    public void CancelarCreacionLobby()
    {
        createLobbyNameIF.text = null;

        isPrivate = true;

        botonEleccionNivelProteccion.GetComponentInChildren<TextMeshProUGUI>().text = "Privado";

        panelCreacionLobby.SetActive(false);

        panelLobbyList.SetActive(true);
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobbyPlayers();

        foreach(Player player in lobby.Players)
        {
            GameObject jugador = Instantiate(prefabSinglePlayer, containerLobbyPlayers);

            LobbyPlayer lobbyPlayer = jugador.GetComponent<LobbyPlayer>();

            lobbyPlayer.UpdatePlayerInfo(player);

            lobbyPlayer.SetKickButtonVisible(LobbyManager.instance.IsLobbyHost() && player.Id != AuthenticationService.Instance.PlayerId);

        }

        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        botonComenzarPartida.gameObject.SetActive(lobby.Players.Count == lobby.MaxPlayers && LobbyManager.instance.IsLobbyHost());
       // panelLobby.SetActive(true);
        
    }

    public void LeaveLobbyButton()
    {
        LobbyManager.instance.LeaveLobby();
        
    }

    public void RefreshLobbyList()
    {
        LobbyManager.instance.LobbiesList();
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        ClearLobbyList();

        foreach (Lobby lobby in lobbyList)
        {
            GameObject lobbySinglePrefab = Instantiate(prefabSingleLobby, containerLobbyList);
            SingleLobby lobbySingle = lobbySinglePrefab.GetComponent<SingleLobby>();
            lobbySingle.UpdateLobby(lobby);
        }
    }

    private void ClearLobbyList()
    {
        for(int i = containerLobbyList.childCount -1; i >= 0; i--)
        {
            Destroy(containerLobbyList.GetChild(i).gameObject);
        }
    }

    private void ClearLobbyPlayers()
    {
        for(int i = containerLobbyPlayers.childCount -1; i >= 0; i--)
        {
            Destroy(containerLobbyPlayers.GetChild(i).gameObject);
        }
    }

    public void MetodoBotonComenzarParitda()
    {
        ComenzarPartidaLobby();
    }

    private async void ComenzarPartidaLobby()
    {
        await LobbyManager.instance.ComenzarPartida();
    }

    public Sprite GetSprite(int id)
    {
        return listSprite[id];
    }
}
