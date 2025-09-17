 using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public enum EstadoTurnoJugador
{
    FaseDados,
    FaseCasillas,
    FasePlanificacion    
}

public class Ficha : MonoBehaviour
{
    [Header("DATOS PLAYER")]
    public int ID;
    public int dinero;
    public int posicionCasilla;
    public int resultadoSuma;
    public bool Turno;
    public List<GameObject>InventarioJugador;

    [Space(10)]
    [Header("Referencias a otros Scripts")]
    [Space(10)]

    private LanzarDados lanzaDados;

    private Ruta currentRoute;

    private Turnos turnos;

    private Economia controlEconomia;

    private ControlBotones controlBotones;

    private CaminosCasillas camino;

    private UIController controlUI;

     private Hipotecacion hipotecacion;

     private LogicaSuerte suerte;

    private ControlTarot controlTarot;

    [Space(10)]
    [Header("Variables varias")]
    [Space(10)]

    public int routePosition;    

    bool isMoving;

    public GameObject inventario;
    public GameObject scrollInventario;

    public GameObject inventarioCartas;
    public GameObject scrollInventarioCarta;

    public EstadoTurnoJugador estadoActual;

    public bool TetocaMover;

    public Vector3 posActual;

    int cantidadDobles;

    int turnosEnCarcel;

    public int casillasAvanzar;

    public bool enCarcel;

    public bool tarjetaLibreCarcel;

    public bool enDerrota;

    public GameObject panelDadosDobles;

    [SerializeField] GameObject pantallaCarcel;

    [SerializeField] GameObject botonLibreCarcel;

    public Button buttonSelecNegociador;

    private Animator animator;

    private int posicionTeleport;
    private readonly int posicionCarcel = 9; //SI SE MODIFICASE LA UBICACION DE LA PRISION, MODIFICAR ESTO TAMBIEN 
    public const string ACTIVAR_TELEPORT = "ActivarTeleport";
    public const string INICIAR_DESCENSO_TELEPORT = "FinalizarTeleport";

    // VARIABLES DE LAS CARTAS DE TAROT
    private bool isEmperatriz;
    private int numEmperatriz = 0;
    private bool isEmperador;
    private bool isPapa;
    private bool isErmitano;
    private bool isFuerza;
    private bool isTorre;
    private bool isLuna;
    private int numSol;
    private bool isTemplanza;
    private bool isCarro = false;
    private bool isColgado;
    private int numVueltasColgado;
    private int dineroApostado;
    private bool isJuicio;
    static int SortByGroupID(GameObject c1, GameObject c2)
    {
        Carta carta1 = c1.GetComponent<Carta>();
        Carta carta2 = c2.GetComponent<Carta>();

        return carta1.dataCarta.groupID.CompareTo(carta2.dataCarta.groupID);
    }
    private void Start()
    {
        estadoActual = EstadoTurnoJugador.FaseDados;
        turnos = FindAnyObjectByType<Turnos>();
        controlEconomia = FindAnyObjectByType<Economia>(); 
        controlUI = FindAnyObjectByType<UIController>();
        hipotecacion = FindAnyObjectByType<Hipotecacion>();
        suerte = FindFirstObjectByType<LogicaSuerte>();
        lanzaDados = FindAnyObjectByType<LanzarDados>();
        currentRoute = FindAnyObjectByType<Ruta>();
        controlBotones = FindAnyObjectByType<ControlBotones>();
        camino = FindAnyObjectByType<CaminosCasillas>();
        controlTarot = FindFirstObjectByType<ControlTarot>();
        animator = GetComponent<Animator>();

        TetocaMover = false;

    }

    private void Update()
    {    
        if(turnos.JugadorActual().ID == ID)
        {
            print("Soy el jugador actual y este es mi estado: " + estadoActual + " y el panel de Tarot es " + controlTarot.EstadoPanelConfirmacion());
        }

        if (Input.GetKeyDown(KeyCode.Space) && estadoActual == EstadoTurnoJugador.FaseDados && ID == turnos.ContadorTurnos() && !enCarcel)
        {
            StartCoroutine(lanzaDados.TirarDado());

        }

    }    
   public IEnumerator Move()
    {
        
        if(isMoving)
        {
            yield break;
        }
        isMoving = true;


        while(casillasAvanzar > 0)  //AVANZA
        {

            routePosition++;
            routePosition %= currentRoute.childNodeList.Count;

            //---------------------------------ESTO SE HA TOCADO EL 20/11 PARA LA CURVA, EN LA OTRA ESCENA NO FUNCIONA
            /* Vector3 nextPos = currentRoute.childNodeList[routePosition].position;

            Vector3 initialPoint = transform.position;
            Vector3 endPoint = nextPos;
            float height = 1.5f;
            Vector3 midPoint = new((initialPoint.x + endPoint.x) / 2, height, (initialPoint.z + endPoint.z) / 2);
            List<Vector3> points = new();
            float vertexCount = 12;
            for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
            {
                var tangent1 = Vector3.Lerp(initialPoint, midPoint, ratio);
                var tangent2 = Vector3.Lerp(midPoint, endPoint, ratio);
                var curve = Vector3.Lerp(tangent1, tangent2, ratio);
                points.Add(curve);
            }
            points.Add(nextPos);
            for(int i = 0; i < points.Count; i++)
            {
                while(transform.position != points[i])
                {
                    transform.position = Vector3.MoveTowards(transform.position, points[i], 10 * Time.deltaTime);
                    yield return null;
                }
            }           

            posActual = nextPos;
            */

            //---------------------------------NUEVO CODIGO MODIFICADO PARA LOS CAMINOS

            List<Vector3> points = new();
            Vector3 nextPos = currentRoute.childNodeList[routePosition].position;

            for (int i = 0; i <= camino.caminoCasilla[routePosition].posicionesCamino.Count; i++)
            {
                Vector3 initialPos = i == 0 ? transform.position : camino.caminoCasilla[routePosition].posicionesCamino[i - 1].position;
                Vector3 endPos = i == camino.caminoCasilla[routePosition].posicionesCamino.Count ? nextPos : camino.caminoCasilla[routePosition].posicionesCamino[i].position;
                float height = ((initialPos.y + endPos.y) / 2) + 1.5f;
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

            posActual = nextPos;

            yield return new WaitForSeconds(0.1f);

            casillasAvanzar--;

        }

        isMoving = false;       
     
        TetocaMover = false;

        ComprobarCasilla(currentRoute.infoCasillas[posicionCasilla]);


        if (lanzaDados.dado1 == lanzaDados.dado2 && !isCarro)
        {
            cantidadDobles++;
            estadoActual = EstadoTurnoJugador.FaseDados;
            StartCoroutine(DoblesConseguido());
            yield break;
        }
        if (isCarro)
        {
            isCarro = false;
        }
    }
    
    IEnumerator DoblesConseguido()
    {
        panelDadosDobles.SetActive(true);
        yield return new WaitForSeconds(2);
        panelDadosDobles.SetActive(false);
    }

    public void ComprobarCasilla(BuildingData casillaCaida)
    {
        // Para las casillas que se pueden comprar
        if (casillaCaida.comprable)
        {
            // Si NO esta comprada, ofrecer comprar
            if (!casillaCaida.comprado)
            {
                controlUI.RellenarPanelCompraEdificio(casillaCaida);
                controlUI.ActivacionPanelCompra(true);

                controlUI.BotonCompraEdificios().interactable = dinero > casillaCaida.precioCasilla;
                
            }
            // Si no, a pagar
            else
            {
                // Si esta hipotecada o el dueño en la carcel, no pasa nada
                Ficha duenoCasilla = turnos.Jugadores[casillaCaida.dueno];

                if (casillaCaida.hipoteca || duenoCasilla.enCarcel)
                {
                    if (!duenoCasilla.EnTorre())
                    {
                        CambioFase();
                        return;
                    }                  
                                        
                }

                // AQUI IRAN TODAS LAS VARIABLES QUE MODIFIQUEN EL PRECIO DE LA CALLE, COSAS DE TAROT Y DEMAS
                int precio = casillaCaida.precioAlquiler;
                if (duenoCasilla.EsEmperatriz())
                {
                    precio = precio + ((precio / 2) * duenoCasilla.NumBuffEmperatriz());
                }

                if (duenoCasilla.EsEmperador())
                {
                    precio *= 2;
                }

                if (isPapa) //ESTE TIENE QUE IR EL ULTIMO DE TODOS
                {
                    precio = 0;
                    isPapa = false;
                }

                if(dinero - precio > 0)
                {
                    controlEconomia.PerderDinero(precio, turnos.ContadorTurnos());

                    if(casillaCaida.dueno > -1)
                    {
                        controlEconomia.GanarDinero(precio, casillaCaida.dueno);
                    }
                    CambioFase();

                }
                else
                {
                    if (isJuicio)
                    {
                        isJuicio = false;
                        int dineroAPagar = dinero > 100 ? dinero - 100 : 0;
                        controlEconomia.PerderDinero(dineroAPagar, turnos.ContadorTurnos());
                        controlEconomia.GanarDinero(dineroAPagar, casillaCaida.dueno);
                    }
                    else  // Si no tienes dinero ni la carta de Juicio, a hipotecar
                    {
                        hipotecacion.ActivarPantallaHipotecacion(this, precio, casillaCaida.dueno);

                    }
                    CambioFase();
                }
            }

        }
        // Para todas las demas casillas que no se compren, vamos mirando que son
        else
        {
            DeteccionCasillasNoComprables(casillaCaida.IDentificador);
        }
    }

    public void DeteccionCasillasNoComprables(int identificador)
    {
        switch (identificador)
        {
            case 12://Casilla Suerte
                suerte.CasillaSuerte(this);
                break;
            case 18://Casilla Loteria
                controlEconomia.GanarDinero(controlEconomia.DineroTotalLoteria(), turnos.ContadorTurnos());
                controlEconomia.ResetearLoteria();
                CambioFase();
                break;
            case 27:
                //Añadir todos los efectos y asi aqui
                enCarcel = true;
                IniciarTeleport(posicionCarcel); //<-- 9 es la posicion actual de la carcel, cambiarla si se modifica
                break;
            case 34:
                //A pagar
                if (dinero - controlEconomia.Impuestos() > 0)
                {
                    // ------------------------------------ 03/12 -----------------------
                    // EL CONTADORTURNOS PARA REFERIRSE A SI MISMO ES SIEMPRE EL MISMO QUE EL ID????
                    controlEconomia.PerderDinero(controlEconomia.Impuestos(), turnos.ContadorTurnos());
                    controlEconomia.AumentarDineroALoteria(controlEconomia.Impuestos());
                    CambioFase();
                }
                //A hipotecar
                else
                {
                    hipotecacion.ActivarPantallaHipotecacion(this, controlEconomia.Impuestos(), -1);
                }
                break;
            case 36: //Casilla Tienda Tarot
                controlTarot.CrearTiendaTarot();
                //CambioFase(); //<<<-------------- La hemos pasado a Cerrar de Tarot para evitar errores
                break;
            default:
                CambioFase();
                break;
        }
    }


    #region Funciones de los cambios de fase
       
    public void Mover()
    {
        
        //Comprobar si la casilla pertenece a alguien
         if (estadoActual == EstadoTurnoJugador.FaseCasillas && ID == turnos.ContadorTurnos() &&TetocaMover==true) 
         {
            // <<<<<<<<<<<< AQUI TENEMOS QUE PONER A 0 PARA TESTEAR SI TODO VA BIEN AL SACAR UNA PAREJA >>>>>>>>>
            // <<<<<<<<<<<< 9 ES LA POSICION DE CARCEL SOLO VISITAS >>>>>>>>
            if (cantidadDobles == 2 && lanzaDados.dado1 == lanzaDados.dado2)
            {
                enCarcel = true;
                IniciarTeleport(posicionTeleport);
                return;
            }

            // LOGICA DE LA TIRADA SI ESTAS EN LA CARCEL
            if (enCarcel)
            {
                if(lanzaDados.dado2 == lanzaDados.dado1)
                {
                    enCarcel = false;
                    turnosEnCarcel = 0;
                }
                else
                {
                    Turno = false;
                    posicionCasilla = routePosition;
                    resultadoSuma = 0;
                    turnos.PasarTurnoAlSiguiente();
                    //CambioFase();
                    return;
                }
            }         
            StartCoroutine(Move());           
           
         }
    }


    public void IniciarTeleport(int destino)
    {
        posicionTeleport = destino;
        animator.SetTrigger(ACTIVAR_TELEPORT);

    }
    public void LogicaTeleport()
    {
        Vector3 nuevaPos = currentRoute.childNodeList[posicionTeleport].transform.position;
        transform.position = new Vector3(nuevaPos.x, nuevaPos.y + 3, nuevaPos.z);
        posActual = nuevaPos;
        posicionCasilla = posicionTeleport;
        routePosition = posicionTeleport;

        casillasAvanzar = 0;
        animator.SetTrigger(INICIAR_DESCENSO_TELEPORT);
        if (enCarcel)
        {
            cantidadDobles = 0;
            turnos.PasarTurnoAlSiguiente();

        }
    }


     public void Finalizar() 
     {
        print("Entra");
        if (controlTarot.EstadoPanelConfirmacion()) return;
        if (estadoActual == EstadoTurnoJugador.FasePlanificacion && ID == turnos.ContadorTurnos()) 
        {
            //Turno = false;
            cantidadDobles = 0;
            if (controlBotones.AnimPanelNegocio.GetBool("Negociar"))
            {
                controlBotones.EjecutarBotonOcultar();
            }
           
            turnos.PasarTurnoAlSiguiente();
            //CambioFase();
        }
     }


    #endregion

    public void CambioFase() 
    {
        // Para que el codigo solo se ejecute si es tu turno
        if (!Turno) return;
        switch (estadoActual)
        {
            case EstadoTurnoJugador.FaseDados:
                
                buttonSelecNegociador.interactable = false;

                //estadoActual = EstadoTurnoJugador.FaseCasillas;
                if (enCarcel)
                {
                    turnosEnCarcel++;
                    if (turnosEnCarcel < 3)
                    {
                        pantallaCarcel.SetActive(true);
                        if (tarjetaLibreCarcel)
                        {
                            botonLibreCarcel.SetActive(true);                            
                        }
                    }
                    else
                    {
                        turnosEnCarcel = 0;
                        enCarcel = false;
                    }
                }                 

            break;

            case EstadoTurnoJugador.FaseCasillas:
                estadoActual = EstadoTurnoJugador.FasePlanificacion;
                buttonSelecNegociador.interactable = true;
            break;
            
            case EstadoTurnoJugador.FasePlanificacion:
                estadoActual = EstadoTurnoJugador.FaseDados;
                buttonSelecNegociador.interactable = false;

            break;

        } 

    }


    public void ActualizarPosicion()
    {
        resultadoSuma = posicionCasilla + casillasAvanzar;
        if (resultadoSuma >= currentRoute.childNodeList.Count)
        {
            posicionCasilla = resultadoSuma - currentRoute.childNodeList.Count;
            
            int premioSalida = 200 + (200 * numSol);
          
            controlEconomia.GanarDinero(premioSalida, turnos.ContadorTurnos());
            if (EsColgado())
            {
                AumentarVueltaColgado();
            }

        }
        else
        {
            posicionCasilla = resultadoSuma;
        }
    }

    // METODOS DEL PANEL DE LA CARCEL
    public void PagarMulta()
    {
        if (ID != turnos.ContadorTurnos()) return;
        controlEconomia.PerderDinero(50, turnos.ContadorTurnos());
        controlEconomia.loteriaCantidad += 50;
        enCarcel = false;
        turnosEnCarcel = 0;
        pantallaCarcel.SetActive(false);
    }

    public void LanzarDadosCarcel()
    {

        if (ID != turnos.ContadorTurnos()) return;
        StartCoroutine(lanzaDados.TirarDado());
        pantallaCarcel.SetActive(false);
    }

    public void UsarTarjetaLibreCarcel()
    {
        if (ID != turnos.ContadorTurnos()) return;
        botonLibreCarcel.SetActive(false);
        pantallaCarcel.SetActive(false);
        enCarcel = false;
        tarjetaLibreCarcel = false;
        turnosEnCarcel = 0;
    }

    public void AsignarInventarios(GameObject inventarioPropiedades, GameObject inventarioTarot)
    {
        inventario = inventarioPropiedades;
        scrollInventario = inventario.GetComponentInParent<ScrollRect>().gameObject;
        inventarioCartas = inventarioTarot;
        scrollInventarioCarta = inventarioCartas.GetComponentInParent<ScrollRect>().gameObject;

        scrollInventarioCarta.SetActive(false); // Lo ponemos en false para que lo primero que se vea sean las propiedades
    }

    public void OrdenarInventarioPropio()
    {
        OrdenarInventario(InventarioJugador, inventario);
    }
    private void OrdenarInventario(List<GameObject> lista, GameObject inventarioPlayer)
    {
        lista.Sort(SortByGroupID);

        //Quitamos de hijos a todos los objetos para reordenarlos
        if (inventarioPlayer.transform.childCount > 0)
        {
            for (int i = inventarioPlayer.transform.childCount - 1; i >= 0; i--)
            {
                inventarioPlayer.transform.GetChild(i).SetParent(null);
            }
        }

        // Los volvemos a hacer hijos en el orden que queremos
        for (int i = 0; i < lista.Count; i++)
        {
            lista[i].transform.SetParent(inventarioPlayer.transform);
        }
    }

    public void CambiarInventario()
    {
        scrollInventario.SetActive(!scrollInventario.activeSelf);
        scrollInventarioCarta.SetActive(!scrollInventarioCarta.activeSelf);


    }


    #region LOGICAS CARTAS TAROT
    public bool EsEmperatriz()
    {
        return isEmperatriz;
    }
    public int NumBuffEmperatriz()
    {
        return numEmperatriz;
    }

    public bool EsEmperador()
    {
        return isEmperador;
    }

    public bool EsErmitano()
    {
        return isErmitano;
    }
    public void ComprobarBuffEmperatriz()
    {
        List<int> listaIDCasas = new();
        int numPropiedades = 0;
        foreach (GameObject obj in InventarioJugador)
        {
            BuildingData infoCarta = obj.GetComponent<Carta>().dataCarta;
            if (!listaIDCasas.Contains(infoCarta.groupID))
            {
                listaIDCasas.Add(infoCarta.groupID);
                numPropiedades++;
            }
        }

        isEmperatriz = listaIDCasas.Count >= 5;

        if (isEmperatriz)
        {
            numEmperatriz++;
        }
    }
    // HABRA QUE AÑADIR ICONO VISUAL QUE ESTA ACTIVADO/DESACTIVADO EL BUFO EN ALGUN MOMENTO
    public void ActivarEmperador()
    {
        isEmperador = true;
    }
    public void DesactivarEmperador()
    {
        isEmperador = false;
    }
    public void ActivarPapa()
    {
        isPapa = true;
    }
    public void ActivarErmitano()
    {
        isErmitano = true;
    }
    public void DesactivarErmitano()
    {
        isErmitano = false;
    }
    public void ActivarFuerza()
    {
        isFuerza = true;
    }
    public bool EnFuerza()
    {
        return isFuerza;
    }
    public void DesactivarFuerza()
    {
        isFuerza = false;
    }
    public void ActivarTorre()
    {
        isTorre = true;
    }
    public bool EnTorre()
    {
        return isTorre;
    }
    public bool EnLuna()
    {
        return isLuna;
    }

    public void ActivarLuna()
    {
        isLuna = true;
    }
    public void DesactivarLuna()
    {
        isLuna = false;
    }
    public void SumarSol()
    {
        numSol++;
    }

    public int NumeroSoles()
    {
        return numSol;
    }
    public bool EsTemplanza()
    {
        return isTemplanza;
    }

    public void ActivarTemplanza()
    {
        isTemplanza = true;
    }
    public void ActivarCarro()
    {
        isCarro = true;
    }

    public void ModificarValorInversionColgado(int cantidad)
    {
        dineroApostado = cantidad;
    }

    public bool EsColgado()
    {
        return isColgado;
    }

    public void ActivarColgado()
    {
        isColgado = true;
        int dineroRestante = dinero / 2;
        controlEconomia.ActualizarDinero(dineroRestante, ID);
        ModificarValorInversionColgado(dineroRestante);
    }
    public void AumentarVueltaColgado()
    {
        numVueltasColgado++;
        CheckColgado();
    }
    public void CheckColgado()
    {
        if(numVueltasColgado >= 3)
        {
            controlEconomia.GanarDinero(dineroApostado * 3, this.ID);
            isColgado = false;
            ModificarValorInversionColgado(0);
            numVueltasColgado = 0;
        }
    }

    public void ActivarJuicio()
    {
        isJuicio = true;
    }
    #endregion
}
