using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class SingleLobby : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyName;
    [SerializeField] TextMeshProUGUI lobbyProtection;
    [SerializeField] TextMeshProUGUI lobbyPlayers;

    private Lobby lobby;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            LobbyManager.instance.JoinLobby(lobby);
        });
    }

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyName.text = lobby.Name;
        lobbyProtection.text = lobby.IsPrivate ? "Private" : "Public";
        lobbyPlayers.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }

}
