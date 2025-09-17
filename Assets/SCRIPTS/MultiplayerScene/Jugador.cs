
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Jugador : NetworkBehaviour
{
    private int ID;


    [SerializeField] Transform padreCamara;
    private const int PRECIO_VUELTA = 200;
    private const float ALTURA_MINIMA_SALTO = 1.5f;

    private bool esImpostor;

    private bool haAtajado;
    private int posTablero;
    private bool esDobles; 
    private int numDobles;
    private bool enCarcel = false;
    private int turnosEnCarcel;
    private bool tarjetaLibreCarcel;

    private int cooldownTrampero;

    private const int TURNO_ACTIVACION_CLASES = 3;

    public NetworkVariable<int> dinero = new();
    public enum FaseJuego
    {
        None,
        LanzaDados,
        Casillas,
        Planificacion
    }
    private FaseJuego faseActual;
    private Classes clase = Classes.None;
    private bool esLuna = false;
    private bool esFuerza = false;
    public bool enDerrota {  get; private set; }
    private int cantidadEnSol = 0;

    [SerializeField] List<CartaCasilla> inventarioCartasCasillas;

    static int SortByGroupID(CartaCasilla c1, CartaCasilla c2)
    {
        int carta1 = c1.GetCasilla().GetGroupID();
        int carta2 = c2.GetCasilla().GetGroupID();

        int result = carta1.CompareTo(carta2);
        if (result == 0)
        {
            // Si el GroupID es igual, comparar otro int (por ejemplo, un ID secundario)
            result = c1.GetCasilla().ID.CompareTo(c2.GetCasilla().ID);
        }
        return result;
    }

    private void Start()
    {
        GameManagerMultiplayer.Instance.OnCambiarFaseJugador += GameManagerMultiplayer_OnCambiarFaseJugador;
        CartaManager.Instance.OnCartaCreada += CartaManager_OnCartaCreada;
        
        //LogicaSuerteMultiplayer.Instance.OnTarjetaLibreConseguida += LogicaSuerte_OnTarjetaLibreConseguida;

        
        
    }

    private void LogicaSuerte_OnTarjetaLibreConseguida(Jugador obj)
    {
        if (obj != this) return;
        tarjetaLibreCarcel = true;
    }

    private void CartaManager_OnCartaCreada(CartaCasilla nuevaCarta)
    {
        if (!IsOwner) return;
        inventarioCartasCasillas.Add(nuevaCarta);
        inventarioCartasCasillas.Sort(SortByGroupID);
    }

    private void GameManagerMultiplayer_OnCambiarFaseJugador(FaseJuego obj, int contadorTurno)
    {
        if (!IsOwner)
        {
            return;
        }
        faseActual = obj;
        if(esImpostor) clase = Classes.Impostor;
        if(faseActual == FaseJuego.LanzaDados)
        {

            if (clase == Classes.Impostor)
            {
                //AQUI TENDREMOS QUE ACTIVAR EL PANEL DEL IMPOSTOR CON LA INFO, LO PONEMOS ANTES PARA QUE EN EL TURNO 3 NO 
                // PASE NADA. PREGUNTAR AL SERVIDOR LAS CLASES O QUE AL ESCOGERLAS LAS COMPARTAN CON TODOS, Y ACTIVAR EL PANEL
                // PARA QUE EL JUGADOR ESCOJA LA OTRA CLASE QUE NO SE PUEDE REPETIR EL PROXIMO TURNO
                ClassManager.Instance.PrepararImpostor();
            }
            if(clase == Classes.Trampero)
            {
                ClassManager.Instance.SumarTurnoTrampero();
            }
            if (contadorTurno == TURNO_ACTIVACION_CLASES)
            {
                
                IniciarSeleccionClaseRpc();
            }

          
        }

      
    }
   
    public override void OnNetworkSpawn()
    {        
        ID = (int)OwnerClientId;

        // Todo lo de la CameraManager quiza se pueda simplicar al hacer LoadScene y tener las camaras desactivadas y tal,
        // Por ahora, con el testeo lo dejaremos asi pero habra que ver mas adelante

        CameraManager.Instance.EscogerCamara(ID).transform.SetParent(padreCamara);
        CameraManager.Instance.ResetCamaraValues(ID);
        CameraManager.Instance.EscogerCamara(ID).enabled = false;
        CameraManager.Instance.EscogerCamara(ID).gameObject.GetComponent<AudioListener>().enabled = false;
        this.gameObject.name = ID.ToString();
        if (IsServer)
        {
            GameManagerMultiplayer.Instance.RegistrarJugadores(this);
            dinero.Value = 2000;
        }
        UIManager.Instance.ActivarPanelUIJugador(ID);

        if (IsOwner)
        {
            // <<<<<<02/07>>>>>> MIRAR SI TRAS CREAR LA VARIABLE EN EL GAMEMANAGER EL DEL UIMANAGER NO ES REDUNDANTE
            UIManager.Instance.ActivarInventarioJugador();
            UIManager.Instance.SetOwnerPlayer(this);
            GameManagerMultiplayer.Instance.SetOwnerPlayer(this);
            CameraManager.Instance.EscogerCamara(ID).enabled = true;
            CameraManager.Instance.EscogerCamara(ID).gameObject.GetComponent<AudioListener>().enabled = true;
            transform.position = TableManager.Instance.GetCasillaArray()[0].transform.position;
            transform.LookAt(TableManager.Instance.GetCasillaArray()[1].transform.position);
        }
        faseActual = FaseJuego.LanzaDados;

    }

    [Rpc(SendTo.Server)]
    private void IniciarSeleccionClaseRpc()
    {
        if (!IsOwner) return;
        ClassManager.Instance.ElegirClases();
    }
    private void Update()
    {

        if (!IsOwner) return;
        Debug.Log(faseActual);
        if (GameManagerMultiplayer.Instance.turnoPlayerActual.Value == ID && Input.GetKeyDown(KeyCode.Space))
        {
            //INICIAMOS EL LANZAMIENTO DE DADOS
            if (faseActual != FaseJuego.LanzaDados) return;
            IniciarLanzamientoDadosRpc();
            faseActual = FaseJuego.Casillas;
        }
    }


    [Rpc(SendTo.Server)]
    private void IniciarLanzamientoDadosRpc()
    {
        DadosManager.Instance.LanzarDosDados();
    }
    
    public void Moverse(int dadoUno, int dadoDos)
    {
        int sumaDados = dadoUno + dadoDos;
        
        //MODIFICAR ESTO PARA MOVERNOS LA CASILLA QUE QUERAMOS
        sumaDados = 2;

        //PARA HACER DOBLES
        //dadoUno = dadoDos;
        
        if (esLuna)
        {
            if(sumaDados > 4)
            {
                sumaDados = 4;
            }
            esLuna = false;
        }

        if (esFuerza)
        {
            sumaDados *= 2;
            esFuerza = false;
        }

        int sumaDadosFinal = clase == Classes.Trotamundos ? (int)(sumaDados * ClassManager.AUMENTO_TROTAMUNDOS) : sumaDados;
        //LOGICA DOBLES
        if(dadoUno == dadoDos)
        {
            if (enCarcel)
            {
                enCarcel = false;
                turnosEnCarcel = 0;
            }
            else
            {
                esDobles = true;
                numDobles++;
                if (numDobles > 2)
                {
                    //TIENE QUE IRSE A LA CARCEL AQUI
                    Debug.LogError("A LA CARCEL CHAVAL");
                    enCarcel = true;
                    return;
                }
            }           
        }
        if (enCarcel)
        {
            turnosEnCarcel++;

            bool carcelCumplida = clase == Classes.Escapista ? turnosEnCarcel == ClassManager.TURNOS_ENCARCEL_ESCAPISTA : turnosEnCarcel > 2;

            if (carcelCumplida)
            {
                enCarcel = false;
                turnosEnCarcel = 0;
                if(dinero.Value < 50)
                {
                    //MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(50, this);
                }
            }
        }
        StartCoroutine(Movement(sumaDados));
    }

    public IEnumerator Movement(int numCasillas)
    {
        int posInicial = posTablero;
        
        while(numCasillas > 0)
        {
            posTablero++;
            posTablero %= TableManager.Instance.GetCasillaArray().Length;
            List<Transform> posCamino = TableManager.Instance.GetCaminoTablero()[posTablero].GetPosicionesCamino();
            List <Vector3> points = new();

            Vector3 nextPos = TableManager.Instance.GetCasillaArray()[posTablero].transform.position;

            for(int i = 0; i <= posCamino.Count; i++)
            {
                Vector3 initialPos = i == 0 ? transform.position : posCamino[i - 1].position;
                Vector3 endPos = i == posCamino.Count ? nextPos : posCamino[i].position;
                float height = ((initialPos.y + endPos.y) / 2) + ALTURA_MINIMA_SALTO;
                Vector3 midPoint = new((initialPos.x + endPos.x) / 2, height, (initialPos.z + endPos.z) / 2);
                float vertexCount = 12;
                for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
                {
                    var tangent1 = Vector3.Lerp(initialPos, midPoint, ratio);
                    var tangent2 = Vector3.Lerp(midPoint, endPos, ratio);
                    var curve = Vector3.Lerp(tangent1, tangent2, ratio);
                    points.Add(curve);
                }
                points.Add(endPos);
            }
            for (int i = 0; i < points.Count; i++)
            {
                while (transform.position != points[i])
                {
                    transform.position = Vector3.MoveTowards(transform.position, points[i], 10 * Time.deltaTime);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.1f);

            numCasillas--;
        }

        //HEMOS DADO UNA VUELTA AL TABLERO AQUI

        if (posInicial > posTablero)
        {
            DarVuelta();
            //SUMAR VUELTA COLGADO DE CARTA TAROT
        }
       
        TableManager.Instance.ComprobarCasilla(posTablero);      

        // AQUI TENDREMOS LA LOGICA DEL CARRO
    }


    public IEnumerator MovementAtajo(List<Transform> lista, int posFinal)
    {
        
            List<Vector3> points = new();
            Vector3 nextPos = TableManager.Instance.GetCasillaArray()[posFinal].transform.position;

            for (int i = 0; i <= lista.Count; i++)
            {
                Vector3 initialPos = i == 0 ? transform.position : lista[i - 1].position;
                Vector3 endPos = i == lista.Count ? nextPos : lista[i].position;
                float height = ((initialPos.y + endPos.y) / 2) + ALTURA_MINIMA_SALTO;
                Vector3 midPoint = new((initialPos.x + endPos.x) / 2, height, (initialPos.z + endPos.z) / 2);
                float vertexCount = 12;
                for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
                {
                    var tangent1 = Vector3.Lerp(initialPos, midPoint, ratio);
                    var tangent2 = Vector3.Lerp(midPoint, endPos, ratio);
                    var curve = Vector3.Lerp(tangent1, tangent2, ratio);
                    points.Add(curve);
                }
                points.Add(endPos);
            }
            for (int i = 0; i < points.Count; i++)
            {
                while (transform.position != points[i])
                {
                    transform.position = Vector3.MoveTowards(transform.position, points[i], 10 * Time.deltaTime);
                    yield return null;
                }
            }
     
        posTablero = posFinal;
        haAtajado = true;
        TableManager.Instance.ComprobarCasilla(posTablero);
    }

    public void DarVuelta()
    {
        int dineroVuelta = PRECIO_VUELTA + (PRECIO_VUELTA * cantidadEnSol);
        GameManagerMultiplayer.Instance.GanarDinero(dineroVuelta, this);

    }


   

    public void SetImpostor(bool valor)
    {
        esImpostor = valor;
    }

    public bool GetImpostor()
    {
        return esImpostor;
    }
    public void SetClass(Classes claseEscogida)
    {
        clase = claseEscogida;

    }

    public Classes GetClass()
    {

        return clase;
    }

    public bool GetEnCarcel()
    {
        return enCarcel;
    }

    public void SumarTurnoEnCarcel()
    {
        turnosEnCarcel++;
    }

    public int GetTurnosEnCarcel()
    {
        return turnosEnCarcel;
    }

    public void SetCarcel(bool valor)
    {
        enCarcel = valor;
    }
    public int GetPlayerID()
    {
        return ID;
    }

    public FaseJuego GetFaseActual()
    {
        return faseActual; 
    
    }  
 
    public bool GetEsDobles()
    {
        return esDobles;
    }

    public int GetPosicionTablero()
    {
        return posTablero;
    }

    public void SetPosicionTablero(int nuevaPos)
    {
        posTablero = nuevaPos;
    }
    public void ModificarEsDobles(bool b)
    {
        esDobles = b;
    }

    public void SetNumDobles(int cantidad)
    {
        numDobles = cantidad;
    }

    public bool GetHaAtajado()
    {
        return haAtajado;
    }
    public void CambiarValorHaAtajado()
    {
        haAtajado = !haAtajado;
    }
}

   

