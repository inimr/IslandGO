using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManagerMultiplayer : NetworkBehaviour
{
    [Serializable]
    public class PlayerData //TEMPORAL
    {
        public int ID;
        public Jugador player;
        public PlayerUIData UIData;
        public List<Casilla> listaPropiedades = new(); 
    }

    public static GameManagerMultiplayer Instance;
    private DiaryManager diaryManager;
    public NetworkVariable<int> turnoPlayerActual = new ();
    private int IDturnoInicial = 0; // MAS ADELANTE SE TENDRA QUE MODIFICAR
    private int contadorTurno;

    public Jugador ownerPlayer {  get; private set; }
    private int movimientosFichasPendientes; //ESTO SERAN LAS FICHAS DE LOS NO-JUGADORES

    private Dictionary<ulong, Jugador> listaJugadores = new();
    public List<PlayerData> listaPlayers = new(); // ESTO SERA TEMPORAL DE TESTEO PARA TENER COSA VISUAL, USAREMOS EL DICTIONARY
    public event Action<Casilla> OnCasillaComprable;

    public event Action OnCasillaComprada;
    public event Action OnCasillaRechazada;

    public event Action<int, int> OnTurnoVueltaDada; //ESTE EVENTO ES PARA LA UI Y LOS INGRESOS PASIVOS
    public event Action<Jugador.FaseJuego, int> OnCambiarFaseJugador;
    public event Action<int, int> OnActualizarDinero;
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
    }

    private void Start()
    {
        UIManager.Instance.OnBotonSiClickado += UIManager_OnBotonSiClickado;
        UIManager.Instance.OnBotonNoClickado += UIManager_OnBotonNoClickado;
        TableManager.Instance.OnALaCarcel += TableManager_OnALaCarcel;
        OnCasillaComprada += GameManagerMultiplayer_OnCasillaComprada;
        OnCasillaRechazada += GameManagerMultiplayer_OnCasillaRechazada;

        diaryManager = FindFirstObjectByType<DiaryManager>();
    }

    // Si luego queremos cambiar los parametros de como se ordenan las listas tenemos que modificar esta para todas las pestañas
    // que no sean la mano jugable; para esa, el otro Sort esta en el Script Jugador.
    static int SortCasillasByGroupID(Casilla c1, Casilla c2)
    {
        int carta1 = c1.GetGroupID();
        int carta2 = c2.GetGroupID();

        int result = carta1.CompareTo(carta2);
        if(result == 0)
        {
            result = c1.ID.CompareTo(c2.ID);
        }
        return result;
    }

    private void TableManager_OnALaCarcel()
    {
        ComienzoCambioTurno();
    }

    private void GameManagerMultiplayer_OnCasillaRechazada()
    {
        CheckDobles();
    }

    private void GameManagerMultiplayer_OnCasillaComprada()
    {
        Casilla casilla = TableManager.Instance.GetCasillaArray()[GetActualPlayer().GetPosicionTablero()];
        listaPlayers[turnoPlayerActual.Value].listaPropiedades.Add(casilla);
        listaPlayers[turnoPlayerActual.Value].listaPropiedades.Sort(SortCasillasByGroupID);
        casilla.ModificarDatosAlCambiarPropietario(turnoPlayerActual.Value, true);
        TableManager.Instance.ComprobarGrupoCompleto(casilla);        

        PerderDinero(casilla.GetPrecioCasilla(), GetActualPlayer());

        CheckDobles();
    }

    public void PerderDinero(int casillaValor, Jugador player)
    {
        int dineroRestante = player.dinero.Value - casillaValor;
        player.dinero.Value = dineroRestante;
        ActualizarUIDineroRpc(dineroRestante, player.GetPlayerID());
    }

    public void GanarDinero(int casillaValor, Jugador player)
    {
        int dineroRestante = player.dinero.Value + casillaValor;
        player.dinero.Value = dineroRestante;
        ActualizarUIDineroRpc(dineroRestante, player.GetPlayerID());
    }
    public void CheckDobles()
    {
        ulong IDPlayer = (ulong)turnoPlayerActual.Value;

        if (GetActualPlayer().GetEsDobles())
        {
            GetActualPlayer().ModificarEsDobles(false);
            CambiarFaseRpc(Jugador.FaseJuego.LanzaDados,RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
        }
        else
        {
            GetActualPlayer().SetNumDobles(0);
            GetActualPlayer().ModificarEsDobles(false);
            CambiarFaseRpc(Jugador.FaseJuego.Planificacion, RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ActualizarUIDineroRpc(int dineroActual, int playerID)
    {
        OnActualizarDinero?.Invoke(dineroActual, playerID);

    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void CambiarFaseRpc(Jugador.FaseJuego fase, RpcParams clientRpcParams)
    {
        OnCambiarFaseJugador?.Invoke(fase, contadorTurno);

    }
    #region Metodos y eventos de comprar y rechazar una casilla

    private void UIManager_OnBotonNoClickado()
    {
        ConfirmacionRechazoCompraRpc();
    }

    private void UIManager_OnBotonSiClickado()
    {
        ConfirmacionCompraCasillaRpc();
    }

    [Rpc(SendTo.Server)]
    private void ConfirmacionCompraCasillaRpc()
    {
        OnCasillaComprada?.Invoke();
    }

    [Rpc(SendTo.Server)]
    private void ConfirmacionRechazoCompraRpc()
    {
        OnCasillaRechazada?.Invoke();
    }

    #endregion

    
    public void ComienzoCambioTurno()
    {
        CambioTurnoRpc();
    }

    [Rpc(SendTo.Server)]

    private void CambioTurnoRpc()
    {
        turnoPlayerActual.Value++;
        turnoPlayerActual.Value %= listaPlayers.Count;

        if (GetActualPlayer().GetImpostor())
        {
            GetActualPlayer().SetClass(Classes.Impostor);
        }
        //ESTO IMPLICARA QUE EL TURNO HA VUELTO AL JUGADOR QUE HAYA EMPEZADO LA PARTIDA
        if(turnoPlayerActual.Value == IDturnoInicial)
        {
            contadorTurno++; //SI QUEREMOS QUE LAS PARTIDAS DUREN X TURNOS LO CONTROLAMOS AQUI

            FichaPandillero[] numFichas = FindObjectsByType<FichaPandillero>(FindObjectsSortMode.None);
            movimientosFichasPendientes = numFichas.Length;
            if (movimientosFichasPendientes < 1)
            {
                TareaEntreTurnosTerminada();
                return;
            }

            //<<<<<02/07>>>>>>
            // BUSCAR EN INTERNET COMO ORDENAR LAS FICHAS MEDIANTE UN INT SUYO, turnosVida

            List<FichaPandillero> listaFichas = new();
            for(int i= 0; i< numFichas.Length; i++)
            {
                listaFichas.Add(numFichas[i]);
            }


            for(int i = movimientosFichasPendientes - 1; i > 0; i--)
            {
                
                listaFichas[i].ReducirTurnosVida();
                if (listaFichas[i].GetTurnosVida() < 1)
                {
                    //AQUI HABRA QUE HACER UNA ANIMACION DE DESAPARICION Y ASI,
                    listaFichas.Remove(listaFichas[i]);
                    listaFichas[i].gameObject.SetActive(false);
                    listaFichas[i].GetComponent<NetworkObject>().Despawn();

                    continue;
                }

                listaFichas[i].Movimiento();

            }
            return;
            
        }
        ulong IDJugador = (ulong) turnoPlayerActual.Value;
        CambiarFaseRpc(Jugador.FaseJuego.LanzaDados, RpcTarget.Single(IDJugador, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void CheckFinalizacionVueltaTurnosRpc(int numDinero, int numKarma, RpcParams clientRpcParams)
    {       
        OnTurnoVueltaDada?.Invoke(numDinero, numKarma);  
        
        //NO SUMAMOS EN NINGUN LADO EL DINERO NI EL KARMA CONSEGUIDO <<<<<<<<<13/07
    }        
    public void TareaEntreTurnosTerminada()
    {
        movimientosFichasPendientes--;

        if(movimientosFichasPendientes < 1)
        {            
            CalcularIngresosPasivos();           
        }
    }
    private void CalcularIngresosPasivos()
    {
        foreach(PlayerData data in listaPlayers)
        {
            int cantidadDinero = 0;
            int cantidadKarma = 0;
            for (int i = 0; i <  data.listaPropiedades.Count; i++)
            {
                if (data.listaPropiedades[i].esHipotecado) continue;
                if (data.listaPropiedades[i].esPasivaKarma)
                {                    
                    cantidadKarma += data.listaPropiedades[i].ingresoPasivo;                    
                }
                else
                {                    
                    cantidadDinero += data.listaPropiedades[i].ingresoPasivo;
                }

            }
            int cantidadKarmaFinal = data.player.GetClass() == Classes.Feligres ? (int)(cantidadKarma * ClassManager.AUMENTO_FELIGRES) : cantidadKarma;
            int cantidadDineroFinal = data.player.GetClass() == Classes.Comerciante ? (int)(cantidadDinero * ClassManager.AUMENTO_COMERCIANTE) : cantidadDinero;
            ulong IDJugadorPanel = (ulong)data.ID;
            CheckFinalizacionVueltaTurnosRpc(cantidadDineroFinal, cantidadKarmaFinal, RpcTarget.Single(IDJugadorPanel, RpcTargetUse.Temp));
        }

        ulong IDJugador = (ulong)turnoPlayerActual.Value;
        CambiarFaseRpc(Jugador.FaseJuego.LanzaDados, RpcTarget.Single(IDJugador, RpcTargetUse.Temp));

    }

    //ESTO HABRA QUE PEDIRLO EN EL BOTON DEL MAPA
    public void ActualizarInfoPosicionTablero(int posTablero)
    {
        ulong IDPlayer = (ulong)turnoPlayerActual.Value;

        ActualizarPosicionTableroJugadorRpc(posTablero, RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ActualizarPosicionTableroJugadorRpc(int posTablero, RpcParams clientRpcParams)
    {
        ownerPlayer.SetPosicionTablero(posTablero);
    }


    // -------------------------------- METODOS DE COMPRA ------------------------------------- //

    public void ActivarEventoCompraCasilla()
    {
        ulong IDPlayer = (ulong)turnoPlayerActual.Value;
        MandarInfoCompraJugadorRpc(GetActualPlayer().GetPosicionTablero(), RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void MandarInfoCompraJugadorRpc(int posTablero, RpcParams clientRpcParams)
    {
        Casilla casilla = TableManager.Instance.GetCasillaArray()[posTablero];
        OnCasillaComprable?.Invoke(casilla);

    }

    #region REGISTRO_PLAYERS

    public void RegistrarJugadores(Jugador player)
    {
        PlayerData data = new()
        {
            ID = player.GetPlayerID(),
            player = player
        };
        listaPlayers.Add(data);

        //ESTO ES CON DICCIONARIO Y NO CON LISTA, LO USAREMOS CUANDO SEPAMOS QUE FUNCIONA BIEN CON LISTA QUE ES VISIBLE
        //listaJugadores.Add(player.GetPlayerID(), player);
    }

    //--------------------------------------------- DIARIO -------------------------------------

  
    [Rpc(SendTo.Server)]
    public void ActualizarDiarioRpc(ulong player)
    {
        
        int cantidadCasillas0 = listaPlayers[0].listaPropiedades.Count;
        int cantidadCasillas1 = listaPlayers[1].listaPropiedades.Count;
        int cantidadCasillas2 = listaPlayers[2].listaPropiedades.Count;
        int cantidadCasillas3 = listaPlayers[3].listaPropiedades.Count;

        int[] arrayCasillasPlayer0 = new int[cantidadCasillas0];
        int[] arrayCasillasPlayer1 = new int[cantidadCasillas1];
        int[] arrayCasillasPlayer2 = new int[cantidadCasillas2];
        int[] arrayCasillasPlayer3 = new int[cantidadCasillas3];
        
        for(int i = 0; i < cantidadCasillas0; i++)
        {
            arrayCasillasPlayer0[i] = listaPlayers[0].listaPropiedades[i].GetPosTablero(); //Cogemos el ID pero seguramente sea mejor coger otra cosa
        }

        for(int i = 0;i < cantidadCasillas1; i++)
        {
            arrayCasillasPlayer1[i] = listaPlayers[1].listaPropiedades[i].GetPosTablero();
        }
        
        for(int i = 0; i<cantidadCasillas2; i++)
        {
            arrayCasillasPlayer2[i] = listaPlayers[2].listaPropiedades[i].GetPosTablero();
        }
        for(int i = 0; i < cantidadCasillas3; i++)
        {
            arrayCasillasPlayer3[i] = listaPlayers[3].listaPropiedades[i].GetPosTablero();
        }

        MandarInfoAJugador(player, arrayCasillasPlayer0, arrayCasillasPlayer1, arrayCasillasPlayer2, arrayCasillasPlayer3);

        //Actualizar el GameManagerMultiplayer del jugador que ha pedido la actualizacion
        //Cambiar el valor de la booleana de DiaryManager de manera local

    }

    private void MandarInfoAJugador(ulong player, int[] lista0, int[] lista1, int[] lista2, int[] lista3)
    {
        MandarInfoListasAJugadorRpc(lista0, lista1, lista2, lista3, RpcTarget.Single(player, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void MandarInfoListasAJugadorRpc(int[] lista0, int[] lista1, int[] lista2, int[] lista3, RpcParams clientRpcParams)
    {
        //Cambiar el valor del update de DiaryManager
        //Resto del code que pueda hacer falta

        if (!IsServer)
        {
            int[][] almacen = new int[][] {lista0, lista1, lista2, lista3};           

            for (int i = 0; i < almacen.Length; i++)
            {
                listaPlayers[i].listaPropiedades.Clear();

                foreach(int y in almacen[i])
                {                    
                    Casilla casilla = TableManager.Instance.GetCasillaArray()[y];
                    listaPlayers[i].listaPropiedades.Add(casilla);
                }              
                listaPlayers[i].listaPropiedades.Sort(SortCasillasByGroupID);
            }
        }

        diaryManager.ActualizarValoresDiario();
        
    }

    //------------------------------------------- FIN DIARIO -------------------------
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            turnoPlayerActual.Value = 0; //<<<< ESTE VALOR POR AHORA SERA 0 HASTA QUE PENSEMOS COMO HACERLO DE OTRA MANERA, ES EL TURNO DEL PLAYER
        }
    }

    #endregion

    public Jugador GetActualPlayer()
    {
        return listaPlayers[turnoPlayerActual.Value].player;
    }

    public Jugador GetPlayer(int IDPlayer)
    {
        return listaPlayers[IDPlayer].player;
    }

    public void SetOwnerPlayer(Jugador player)
    {
        ownerPlayer = player;
    }
}

