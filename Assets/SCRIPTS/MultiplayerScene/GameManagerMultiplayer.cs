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
        public List<Casilla> listaPropiedades = new(); //<< ESTO NO SE SI LO USAREMOS PARA ALGO LA VERDAD
    }

    public static GameManagerMultiplayer Instance;
    private DiaryManager diaryManager;

    public NetworkVariable<bool> hasGameStarted = new();
    public NetworkVariable<int> turnoPlayerActual = new ();
    private int IDturnoInicial = 0; // MAS ADELANTE SE TENDRA QUE MODIFICAR
    private int contadorTurno;

    [SerializeField] GameObject playerPrefab;

    public Jugador ownerPlayer{  get; private set; }
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
        if (result == 0)
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
        
        while (GetActualPlayer().enDerrota.Value)
        {
            turnoPlayerActual.Value++;
            turnoPlayerActual.Value %= listaPlayers.Count;
        }
      

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
            data.player.SetIngresosPasivos(cantidadDineroFinal, cantidadKarmaFinal);
          
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
    //--------------------------------------------- DIARIO -------------------------------------


    [Rpc(SendTo.Server)]
    public void ActualizarDiarioRpc(ulong player)
    {
        List<List<int>> listaPropiedades = new();
        
        for(int i = 0; i < listaPlayers.Count; i++)
        {
            listaPropiedades.Add(new List<int>());
            if (listaPlayers[i].player.enDerrota.Value) continue;

            int cantidadCasillas = listaPlayers[i].listaPropiedades.Count;
            for(int y = 0; y < cantidadCasillas; y++)
            {
                listaPropiedades[i].Add(listaPlayers[i].listaPropiedades[y].GetPosTablero());
            }
            
        }


        int[] arrayPropsP0 = listaPropiedades.Count > 0 ? listaPropiedades[0]?.ToArray() : null;
        int[] arrayPropsP1 = listaPropiedades.Count > 1 ? listaPropiedades[1]?.ToArray() : null;
        int[] arrayPropsP2 = listaPropiedades.Count > 2 ? listaPropiedades[2]?.ToArray() : null;
        int[] arrayPropsP3 = listaPropiedades.Count > 3 ? listaPropiedades[3]?.ToArray() : null;

        

        MandarInfoAJugador(player, arrayPropsP0, arrayPropsP1, arrayPropsP2, arrayPropsP3);

    }

    private void MandarInfoAJugador(ulong player, int[] lista0, int[] lista1, int[] lista2, int[] lista3)
    {
        MandarInfoListasAJugadorRpc(lista0, lista1, lista2, lista3, RpcTarget.Single(player, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void MandarInfoListasAJugadorRpc(int[] lista0, int[] lista1, int[] lista2, int[] lista3, RpcParams clientRpcParams)
    {

        // CORREGIR ERROR, SI SOMOS MENOS DE 4 PLAYERS, DA ERROR DE INDEX

        if (!IsServer)
        {
            int[][] almacen = new int[][] { lista0, lista1, lista2, lista3 };

            for (int i = 0; i < listaPlayers.Count; i++)
            {
                listaPlayers[i].listaPropiedades.Clear();
                if (almacen[i].Length == 0) continue;
                foreach (int y in almacen[i])
                {
                    Casilla casilla = TableManager.Instance.GetCasillaArray()[y];
                    listaPlayers[i].listaPropiedades.Add(casilla);
                }
                listaPlayers[i].listaPropiedades.Sort(SortCasillasByGroupID);
            }
        }

        diaryManager.ActualizarValoresDiario();

    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void MandarOfertaJugadorRpc(int[] listaOfrecer, int[] listaPedir, int dineroOfrecer, int dineroPedir, int jugadorOfreciente, RpcParams clientRpcParams)
    {
        diaryManager.RellenarDatosOfertaNegociadora(listaOfrecer, listaPedir, dineroOfrecer, dineroPedir, jugadorOfreciente);
    }

    [Rpc(SendTo.Server)]
    public void ModificarValoresNegociacionRpc(int playerID0, int playerID1, bool estado)
    {
        listaPlayers[playerID0].player.enNegociacion.Value = estado;
        listaPlayers[playerID1].player.enNegociacion.Value = estado;


    }

    [Rpc(SendTo.Server)]
    public void ActualizarNegociacionRpc(int player0, int player1, int[]listaOfrecidaPlayer0, int[] listaPedidaPlayer1, int dineroOfrecido, int dineroPedido)
    {
        foreach(int i in listaOfrecidaPlayer0)
        {
            Casilla cas = TableManager.Instance.GetCasillaArray()[i];

            cas.ModificarDatosAlCambiarPropietario(player1, true);
            cas.SetNivelAlquiler(0);
            TableManager.Instance.ComprobarGrupoCompleto(cas);
        }

        //FALTA TAROT
        foreach(int i in listaPedidaPlayer1)
        {
            Casilla cas = TableManager.Instance.GetCasillaArray()[i];

            cas.ModificarDatosAlCambiarPropietario(player0, true);
            cas.SetNivelAlquiler(0);
            TableManager.Instance.ComprobarGrupoCompleto(cas);
        }

        //FALTA TAROT

        int dineroGanadoPlayer0 = -dineroOfrecido + dineroPedido;
        int dineroGanadoPlayer1 = dineroOfrecido - dineroPedido;
        GanarDinero(dineroGanadoPlayer0, listaPlayers[player0].player);
        GanarDinero(dineroGanadoPlayer1, listaPlayers[player1].player);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void MandarConfirmacionNegociacionRpc(bool aceptado, int oponenteID, RpcParams clientRpcParams)
    {
        diaryManager.ConfirmacionNegociacion(aceptado, oponenteID);
    }

    //------------------------------------------- FIN DIARIO -------------------------

    #region REGISTRO_PLAYERS

    public void RegistrarJugadores(Jugador player)
    {
        PlayerData data = new()
        {
            ID = player.GetPlayerID(),
            player = player
        };
        listaPlayers.Add(data);

        listaPlayers.Sort((a,b) => a.ID.CompareTo(b.ID)); //Para ordenar la lista de manera de los ID
        //ESTO ES CON DICCIONARIO Y NO CON LISTA, LO USAREMOS CUANDO SEPAMOS QUE FUNCIONA BIEN CON LISTA QUE ES VISIBLE
        //listaJugadores.Add(player.GetPlayerID(), player);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Hemos conseguido entrar satisfactoriamente aqui solo desde el host");
            turnoPlayerActual.Value = 0; //<<<< ESTE VALOR POR AHORA SERA 0 HASTA QUE PENSEMOS COMO HACERLO DE OTRA MANERA, ES EL TURNO DEL PLAYER

          
        }
    }

    public void CreacionPlayers()
    {
        if (!IsServer) return;
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {

            GameObject newPlayer = Instantiate(playerPrefab, TableManager.Instance.GetCasillaArray()[0].transform.position, Quaternion.identity);

            NetworkObject obj = newPlayer.GetComponent<NetworkObject>();

            obj.SpawnAsPlayerObject(client.ClientId);

            //obj.SpawnWithOwnership(client.ClientId);
            Debug.Log("Hemos entrado al foreach y este es el clientId de cada uno " + client.ClientId);

        }
    }
    #endregion
    private bool GetLobbyManager()
    {
       LobbyManager obj = GameObject.FindFirstObjectByType<LobbyManager>();

        if (obj == null) return false;
        else
        {
            return true;
        }
    }
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

