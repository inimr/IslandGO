using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CasillasEventManager : NetworkBehaviour
{
    // Este script tendra toda la logica de los eventos que suceden en las casillas Aleatorias y todo lo necesario
    // para que esto funcione bien
    [SerializeField] GameObject panelEleccion;
    [SerializeField] TextMeshProUGUI panelEleccionTexto;
    [SerializeField] Button botonSi;
    [SerializeField] Button botonNo;
    [SerializeField] GameObject panelNoEleccion;
    [SerializeField] TextMeshProUGUI panelNoEleccionTexto;

    [SerializeField] EventosCasillas[] arrayEventos; // Array donde guardaremos todos los eventos posibles

    public static CasillasEventManager Instance;
    private void Awake()
    {
       
        Instance = this;
    }



    public void EscogerEventoCasilla(int casillaID)
    {
        Casilla casilla = TableManager.Instance.GetCasillaArray()[casillaID];

        int numeroEvento = Random.Range(0, casilla.listaEventos.Count);

        EventosCasillas evento = casilla.listaEventos[numeroEvento];

        LogicaEventos(evento);
    }


    // AQUI IRAN TODA LA LOGICA DE TODOS LOS EVENTOS DE LAS CASILLAS ALEATORIAS, EL ID SE LO PASARA LA PROPIA CASILLA
    public void LogicaEventos(EventosCasillas evento)
    {
        int ID = evento.ID;
        Jugador jugador = GameManagerMultiplayer.Instance.GetActualPlayer();
        ulong IDPlayer = (ulong)GameManagerMultiplayer.Instance.turnoPlayerActual.Value;

        switch (ID)
        {
            case 0: //GANAS 200
                ActivarPanelNoEleccionRpc(evento.ID, RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
                GameManagerMultiplayer.Instance.GanarDinero(evento.cantidadDinero, jugador);
                break;
            case 1: //PIERDES 200
                if(jugador.dinero.Value < evento.cantidadDinero)
                {
                    // MODO HIPOTECACION
                }
                else
                {
                    ActivarPanelNoEleccionRpc(evento.ID, RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
                    GameManagerMultiplayer.Instance.PerderDinero(evento.cantidadDinero, jugador);
                    TableManager.Instance.SumarFondosBote(evento.cantidadDinero);
                }
               
                break;
            case 2:
                foreach (GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
                {

                    if (data.player != jugador)
                    {
                        if (data.player.enDerrota) continue;
                        if (data.player.dinero.Value < 50)
                        {
                            //MODO HIPOTECACION
                        }
                        else
                        {
                            GameManagerMultiplayer.Instance.PerderDinero(50, data.player);
                            GameManagerMultiplayer.Instance.GanarDinero(50, jugador);

                        }
                    }
                }
                GameManagerMultiplayer.Instance.GanarDinero(50, jugador);

                break;
            case 3:
                break;
                
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ActivarPanelNoEleccionRpc(int eventoID, RpcParams clientRpcParams)
    {
        panelNoEleccionTexto.text = arrayEventos[eventoID].cuerpoTexto;
        panelNoEleccion.SetActive(true);

    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ActivarPanelEleccionRpc(int eventoID, RpcParams clientRpcParams)
    {
        panelEleccionTexto.text = arrayEventos[eventoID].cuerpoTexto;
        panelEleccion.SetActive(true);

    }
    /// <summary>
    /// Evento para el boton del PanelNoSeleccionable para comprobar si ha sacado dobles y cambiar la fase del Jugador actual.
    /// </summary>
    public void FinalizarEventoNoSeleccionable()
    {
        panelNoEleccion.SetActive(false);
        MandarCheckDoblesServerRpc();

    }
    [Rpc(SendTo.Server)]
    private void MandarCheckDoblesServerRpc()
    {
        GameManagerMultiplayer.Instance.CheckDobles();

    }

}
