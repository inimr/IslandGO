using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField] Image playerIcon;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] Button kickButton;

    private Player player;



    public void SetKickButtonVisible(bool state)
    {
        kickButton.gameObject.SetActive(state);
    }

    public void UpdatePlayerInfo(Player jugador)
    {
        player = jugador;
        playerNameText.text = jugador.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        string value = jugador.Data[LobbyManager.KEY_PLAYER_ICON].Value;
        int.TryParse(value, out int iconChosen);
        playerIcon.sprite = LobbyUI.Instance.GetSprite(iconChosen);

    }

    public void KickPlayer()
    {
        if(player != null)
        {
            LobbyManager.instance.KickPlayer(player.Id);
        }
    }
}
