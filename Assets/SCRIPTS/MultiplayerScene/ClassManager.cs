using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum Classes
{
    None = 0,
    Trampero = 1,
    Feligres = 2,
    Pandillero = 3,
    Trotamundos = 4,
    Comerciante = 5,
    Escapista = 6,
    Inversor = 7,
    Impostor = 8
}
public class ClassManager : NetworkBehaviour
{

    [SerializeField] List<ClassesSO> baseDatosClases = new();

    private List<int> clasesEscogidas;

    private int claseAnteriorTurno = -1;

    [SerializeField] FichaPandillero prefabPandillero;
    [SerializeField] Transform padrePandilleros;
    private List<FichaPandillero> listaPandilleros = new();


    public event Action<int, int> OnClasesAEscoger;

    public event Action<int> OnClaseEscogida;

    public event Action<int[]> OnClasesParaImpostor; //<<<<< ESTA LISTA ES LA QUE LA UI TIENE QUE USAR PARA LA CLASE INPOSTOR

    public event Action<int> OnModoTramperoActivado;

    public static ClassManager Instance;

    [Header("Variables Modo Trampero")]
    [SerializeField] GameObject canvasFlechasTrampero;
    [SerializeField] Button botonAvanzarCasillaTrampero;
    [SerializeField] Button botonRetrocederCasillaTrampero;
    [SerializeField] Button botonActivarModoTrampero; // Este es el boton que aparecera a la izquierda (no se si ponerlo en UI)
    private int posTrampa = -1; //Al iniciar modo trampero se pondra en la casilla donde se encuentra el jugado


    public const float PAGO_EXTRA_INVERSOR = 1.15f;
    public const float INGRESO_EXTRA_INVERSOR = 1.3f;
    public const float DESCUENTO_ESCAPISTA = 0.8f;
    public const float AUMENTO_FELIGRES = 1.25F;
    public const float AUMENTO_COMERCIANTE = 1.15f;
    public const float AUMENTO_TROTAMUNDOS = 1.5f;
    public const int PORCENTAJE_ATAJABLE = 15;
    public const int PORCENTAJE_ATAJABLE_TROTAMUNDOS = 30;
    public const int TURNOS_ENCARCEL_ESCAPISTA = 1;

    private const int TURNOS_REACTIVAR_TRAMPERO = 3;

    private int turnosRestantesReactivarTrampero = 0;

    public const int NUMERO_IMPOSTOR = 7;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    
    //ESTE METODO TIENE QUE HACERLO EL SERVIDOR
    public void ElegirClases()
    {
        int primeraOpcion = UnityEngine.Random.Range(0, baseDatosClases.Count);

        while (clasesEscogidas.Contains(primeraOpcion))
        {
            primeraOpcion = UnityEngine.Random.Range(0, baseDatosClases.Count);
        }
        clasesEscogidas.Add(primeraOpcion);
        int segundaOpcion = UnityEngine.Random.Range(0, baseDatosClases.Count);

        while(clasesEscogidas.Contains(segundaOpcion) || segundaOpcion == primeraOpcion)
        {
            segundaOpcion = UnityEngine.Random.Range(0, baseDatosClases.Count);
        }
        clasesEscogidas.Add(segundaOpcion);

        ulong player = (ulong)GameManagerMultiplayer.Instance.turnoPlayerActual.Value;
        MandarInfoClasesServerRpc(primeraOpcion, segundaOpcion, RpcTarget.Single(player, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void MandarInfoClasesServerRpc(int clase, int clase2, RpcParams clientRpcParams)
    {
        OnClasesAEscoger?.Invoke(clase, clase2);

    }
    
    [Rpc(SendTo.Server)]
    public void MandarInfoClickadoServerRpc(int numeroEscogido, bool esImpostor)
    {

        if (!clasesEscogidas.Contains(numeroEscogido))
        {
            clasesEscogidas.Add(numeroEscogido);
        }
        Jugador actualPlayer = GameManagerMultiplayer.Instance.GetActualPlayer();
        Classes claseEscogida = baseDatosClases[numeroEscogido].clase;
        actualPlayer.SetClass(claseEscogida);
        if(numeroEscogido == NUMERO_IMPOSTOR)
        {
            actualPlayer.SetImpostor(true);
        }
        if (esImpostor) //Esta booleana es para cuando modifiquemos la clase de un Jugador que tenga la clase Impostor
        {
            claseAnteriorTurno = numeroEscogido;
        }
        ActualizarIconosClaseUI(numeroEscogido);
    }

    /// <summary>
    /// Método que llamara el Jugador de manera local para que esta clase mande al servidor que comience a preparar la lista
    /// con las clases disponibles
    /// </summary>
    public void PrepararImpostor()
    {
        PrepararImpostorRpc();
    }

  
    [Rpc(SendTo.Server)]
    private void PrepararImpostorRpc()
    {
        List<int> listaImpostor = new();

        for(int i = 0; i < clasesEscogidas.Count; i++)
        {
            if (clasesEscogidas[i] == claseAnteriorTurno) continue;
            if (clasesEscogidas[i] == NUMERO_IMPOSTOR) continue;
            listaImpostor.Add(i);
        }
        PrepararListaDevolverImpostor(listaImpostor.ToArray());
    }

    private void PrepararListaDevolverImpostor(int[] lista)
    {
        ulong player = (ulong)GameManagerMultiplayer.Instance.turnoPlayerActual.Value;
        ListaDevolverImpostorRpc(lista, RpcTarget.Single(player, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ListaDevolverImpostorRpc(int[] lista, RpcParams clientRpcParams)
    {
        OnClasesParaImpostor?.Invoke(lista);

    } 

    // ESTE METODO HABRA QUE LANZARLO CADA VEZ QUE IMPOSTOR CAMBIA DE CLASE TAMBIEN
    public void ActualizarIconosClaseUI(int numeroEscogido)
    {
        MandarInfoIconoUIRpc(numeroEscogido);
    }
    [Rpc(SendTo.Everyone)]
    private void MandarInfoIconoUIRpc(int numeroEscogido)
    {
        OnClaseEscogida.Invoke(numeroEscogido);
    }

    public void BotonClasePandillero()
    {
        Jugador player = GameManagerMultiplayer.Instance.ownerPlayer;
        if (player.GetClass() != Classes.Pandillero) return;
        if (player.GetFaseActual() != Jugador.FaseJuego.Planificacion) return;
        InfoCreacionPandilleroRpc();
    }

    [Rpc(SendTo.Server)]
    private void InfoCreacionPandilleroRpc()
    {
        for(int i = 0; i < listaPandilleros.Count; i++)
        {
            FichaPandillero pandillero = listaPandilleros[i];
            if (pandillero.gameObject.activeSelf) continue;

            
            //13/07 >>>>>>>> MIRAR APUNTES CODE MONKEY PARA VER SI EL ORDEN DE LAS COSAS ESTA BIEN AL INSTANCIAR UN OBJETO
            // 18/07 >>> AL ESTAR YA INSTANCIADO NO SE COMO FUNCIONARA EL ASUNTO, SI EN EL TESTEO NOS DA PROBLEMAS, QUITAREMOS
            // LA INSTANCIA EN OnNetworkSpawn Y SIMPLEMENTE IREMOS INSTANCIANDO AQUI
            int posTableroPandillero = UnityEngine.Random.Range(0, TableManager.Instance.GetCasillaArray().Length);
            pandillero.transform.position = TableManager.Instance.GetCasillaArray()[posTableroPandillero].transform.position;
            TableManager.Instance.GetCasillaArray()[posTableroPandillero].ModificarContienePandillero(true);
            pandillero.posTablero = posTableroPandillero;
            pandillero.gameObject.SetActive(true);
            pandillero.GetComponent<NetworkObject>().Spawn();
            break;
           
        }
    }

    #region Logica Clase Trampero
    public void LogicaServerTrampero()
    {
        ConseguirInfoPosicionRpc();
    }
    [Rpc(SendTo.Server)]
    private void ConseguirInfoPosicionRpc()
    {
        int posTablero = GameManagerMultiplayer.Instance.ownerPlayer.GetPosicionTablero();
        MandarInfoPosicionJugador(posTablero);
    }

    private void MandarInfoPosicionJugador(int posTablero)
    {
        ulong IDPlayer = (ulong)GameManagerMultiplayer.Instance.turnoPlayerActual.Value;
        MandarInfoAJugadorActualRpc(posTablero, RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void MandarInfoAJugadorActualRpc(int posTablero, RpcParams clientRpcParams)
    {
        OnModoTramperoActivado?.Invoke(posTablero);
        posTrampa = posTablero;

    }

    private void ModificarCanvasFlechasTrampero()
    {
        Transform transformCasillaActual = TableManager.Instance.GetCasillaArray()[posTrampa].transform;
        Vector3 nuevaPosCanvasTrampero = transformCasillaActual.position;
        nuevaPosCanvasTrampero.y += 0.5f;
        canvasFlechasTrampero.transform.position = nuevaPosCanvasTrampero;
        canvasFlechasTrampero.transform.rotation = Quaternion.Euler(0, transformCasillaActual.eulerAngles.y, 0);

        int siguientePos = (posTrampa + 1) % TableManager.Instance.GetCasillaArray().Length;
        int anteriorPos = (posTrampa - 1 + TableManager.Instance.GetCasillaArray().Length) % TableManager.Instance.GetCasillaArray().Length;

        Vector3 dirSiguiente = TableManager.Instance.GetCasillaArray()[posTrampa].transform.InverseTransformDirection(TableManager.Instance.GetCasillaArray()[siguientePos].transform.position - nuevaPosCanvasTrampero).normalized;
        float avanzarZ = Mathf.Atan2(dirSiguiente.z, dirSiguiente.x) * Mathf.Rad2Deg;
        botonAvanzarCasillaTrampero.transform.localRotation = Quaternion.Euler(90, 0, avanzarZ);

        Vector3 dirAnterior = TableManager.Instance.GetCasillaArray()[posTrampa].transform.InverseTransformDirection(TableManager.Instance.GetCasillaArray()[anteriorPos].transform.position - nuevaPosCanvasTrampero).normalized;
        float anteriorZ = Mathf.Atan2(dirAnterior.z, dirAnterior.x) * Mathf.Rad2Deg;
        botonRetrocederCasillaTrampero.transform.localRotation = Quaternion.Euler(90, 0, anteriorZ);
    }
    public void BotonAvanzarTrampero()
    {
        posTrampa++;
        posTrampa %= TableManager.Instance.GetCasillaArray().Length;

        ModificarCanvasFlechasTrampero();
        //Llamaremos aqui tambien a la camara de camara manager

    }

    public void BotonRetrocederTrampero()
    {
        posTrampa--;
        if(posTrampa < 0) posTrampa = TableManager.Instance.GetCasillaArray().Length - 1;

        ModificarCanvasFlechasTrampero();
        //Llamaremos aqui tambien a la camara de camara manager, ASIGNARLO AL BOTON Y A PASTAR

    }

    /// <summary>
    /// Este Metodo tiene que ir al boton de colocar trampa en el Modo trampero
    /// </summary>
    public void ColocarTrampa()
    {
        MandarInfoTrampaRpc();
    }
    [Rpc(SendTo.Server)]
    private void MandarInfoTrampaRpc()
    {
        TableManager.Instance.GetCasillaArray()[posTrampa].ModificarContieneTrampa(true);
    }

    public void SumarTurnoTrampero()
    {
        turnosRestantesReactivarTrampero++;
        if(turnosRestantesReactivarTrampero == TURNOS_REACTIVAR_TRAMPERO)
        {
            // <<<<<<<<<<<< 23/07 AQUI REACTIVAREMOS EL BOTON DEL TRAMPERO QUE TODAVIA NO HA SIDO CREADO

        }
    }
    #endregion
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        for (int i = 0; i < 10; i++)
        {
            //HAY QUE HACER pandillero.GetComponent<NetworkObject>().Spawn(); PARA QUE SE SINCRONICE Y APAREZCA EN LA ESCENA
            //Y DESPAWN CUANDO VAYAMOS A DESACTIVARLA

            FichaPandillero pandillero = Instantiate(prefabPandillero,padrePandilleros);
            listaPandilleros.Add(pandillero);
            pandillero.gameObject.SetActive(false);
        }

    }

    public ClassesSO GetClass(int numero)
    {
        return baseDatosClases[numero];
    }   
}
