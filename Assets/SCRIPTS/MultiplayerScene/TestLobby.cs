using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{


    private Lobby hostLobby;
    private float heartbeatTimer;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateLobby();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ListLobbies();
        }
    }

    private async void  HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float maxTimer = 15f;
                heartbeatTimer = maxTimer;

               await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            Lobby myLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
            hostLobby = myLobby;
            Debug.Log("Created Lobby! " + myLobby.Name + " " + myLobby.MaxPlayers);
        }

        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queyOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    //new QueryFilter(QueryFilter.FieldOptions.HasPassword, "0", QueryFilter.OpOptions.CONTAINS) //<< Esto no se si funcionara
                },
               
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(); // Tiene un parametro para filtrar 

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
     

    }

}
