using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour
{

    public CinemachineCamera camaraCinemachine;

    [SerializeField] CinemachineCamera[] camerasPlayers;

    [SerializeField] CinemachineCamera cinemachineTrampero;

    [SerializeField] CinemachineCamera cinemachineZoom;


    private const int MAX_PRIORITY = 20;
   public static CameraManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        //camaraCinemachine.Follow = this.transform; ==> Con esto pondremos a quien seguir
        //camaraCinemachine.Priority = 5;   ==> Cambiaremos la prioridad, prioridad mas alta = Camara va hacia ahi
    }

    public CinemachineCamera EscogerCamara(int pos)
    {
        return camerasPlayers[pos];
    }

   
    public void AsignarCamaraPlayer(Jugador player, Transform lookAt, Transform positionCamara)
    {
        CinemachineCamera camaraJugador = camerasPlayers[player.GetPlayerID()];
        camaraJugador.transform.GetComponent<EliminarObjetosDelantePlayer>().SetOwnerPlayer(player);
        camaraJugador.Follow = lookAt;
        camaraJugador.transform.position = positionCamara.position;
    }

    public void AsignarPrioridad(Jugador player)
    {
        CinemachineCamera camaraJugador = camerasPlayers[player.GetPlayerID()];

        camaraJugador.Priority = MAX_PRIORITY;
    }
}
