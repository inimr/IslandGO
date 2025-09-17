using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using NUnit.Framework.Constraints;
using System.Security.Cryptography;
using JetBrains.Annotations;


[System.Serializable]
public class SpotTienda
{
    [HideInInspector]public string name; //Ocultamos despues para que el nombre no ocupe
    public CartaTarot cartaTienda;
    public Button botonComprar;
    public TextMeshProUGUI textBotonPrecio;
    public Button botonReroll;
    public int precioReroll = 50;
    public bool comprado = false;
    public bool rerolled = false;
}

[System.Serializable]

public class VariablesSacerdotisa
{
    public Button botonCarta;
    public CartaTarot carta;
}
public class ControlTarot : MonoBehaviour
{
    public TarotSO[] baseDatosTarot;

    public SpotTienda[] spots;

    public GameObject panelUITarot;
    private Turnos controlTurnos;
    public CartaTarot prefabTarot;
    private Economia economia;
    private Ruta ruta;
    private CreacionCarta creacionCarta;
    
    
    [SerializeField] GameObject panelConfirmarTarot;
    private bool panelTarotActivo = false;
    private int seleccionIDTarot;

    [Header("Variables del intercambio debido a tarot")] //Quiza mas adelante unificarlos ambos la verdad
    [SerializeField] GameObject panelIntercambioTarot;
    [SerializeField] GameObject panelSeleccionPsjIntercambio;
    [SerializeField] Transform inventarioJugadorActual;
    [SerializeField] Transform inventarioJugadorSeleccionado;
    [SerializeField] Button prefabSeleccionJugador;
    [SerializeField] Transform padreButtons;

    private List<GameObject> listaCartasPActual;
    private List<GameObject> listaCartasPSeleccionado;
    public static Carta cartaSeleccionadaPlayer;
    public static Carta cartaSeleccionadaOtro;
    public static bool enMago;

    [Header("Variables Sacerdotisa")]
    [SerializeField] GameObject panelSacerdotisa;
    [SerializeField] VariablesSacerdotisa[] variablesSacerdotisa;
    private void Start()
    {
        controlTurnos = FindFirstObjectByType<Turnos>();
        economia = FindFirstObjectByType<Economia>();
        ruta = FindFirstObjectByType<Ruta>();
        creacionCarta = FindFirstObjectByType<CreacionCarta>();
        
    }

    #region Logica Tienda Tarot
    public void CrearTiendaTarot()
    {
        panelUITarot.SetActive(true);
        int numeroRandom = Random.Range(0, baseDatosTarot.Length);       
        int numeroRandom2 = Random.Range(0, baseDatosTarot.Length);

        while(numeroRandom2 == numeroRandom)
        {
           numeroRandom2 = Random.Range(0, baseDatosTarot.Length);
        }

        PasarInfoCarta(numeroRandom, 0);
        PasarInfoCarta(numeroRandom2, 1);
        DetectarSiHayDinero();


    }
    private void PasarInfoCarta(int ID, int spot)
    {
        TarotSO infoCartaTarot = baseDatosTarot[ID];
        CartaTarot cartaActual = spots[spot].cartaTienda;

        cartaActual.titulo.text = infoCartaTarot.nombre;
        cartaActual.descripcion.text = infoCartaTarot.descripcionCarta;
        cartaActual.ID = infoCartaTarot.ID;
        cartaActual.precio = infoCartaTarot.precio;
        cartaActual.enTienda = true; // Pasar a false cuando se compre, para evitar clicks innecesarios
        spots[spot].textBotonPrecio.text = infoCartaTarot.precio.ToString();

    }
    private void DetectarSiHayDinero()
    {
        int dineroPlayer = controlTurnos.JugadorActual().dinero;
        foreach (SpotTienda spot in spots)
        {
            if (spot.comprado) continue;                        
            
            spot.botonComprar.interactable = dineroPlayer >= spot.cartaTienda.precio;
            spot.botonReroll.interactable = dineroPlayer >= spot.precioReroll;

            if (spot.rerolled) { spot.botonReroll.interactable = false; }
          
        }
    }
    public void RerollTienda(int spot)
    {
        economia.PerderDinero(spots[spot].precioReroll, controlTurnos.JugadorActual().ID);

        List<int> listaInts = new();
        foreach(SpotTienda hueco in spots)
        {
            listaInts.Add(hueco.cartaTienda.ID);
        }
        
        int nuevoNumero = Random.Range(0, baseDatosTarot.Length);

        while (listaInts.Contains(nuevoNumero))
        {
            nuevoNumero = Random.Range(0, baseDatosTarot.Length);
        }

        spots[spot].botonReroll.interactable = false;
        spots[spot].rerolled = true;
        PasarInfoCarta(nuevoNumero, spot);
        DetectarSiHayDinero();        
    }

    public void ComprarCartaTarot(int posBoton)
    {        
        economia.PerderDinero(spots[posBoton].cartaTienda.precio, controlTurnos.JugadorActual().ID);


        CartaTarot cartaComprada = Instantiate(spots[posBoton].cartaTienda, controlTurnos.JugadorActual().inventarioCartas.transform);
        cartaComprada.enTienda = false;
        cartaComprada.name = cartaComprada.titulo.text;
        cartaComprada.dueno = controlTurnos.JugadorActual().ID;
        spots[posBoton].botonComprar.interactable = false;
        spots[posBoton].botonReroll.interactable = false;
        spots[posBoton].comprado = true;
    }

    public void CerrarTiendaTarot()
    {
        foreach(SpotTienda spot in spots)
        {
            spot.comprado = false;
        }
        controlTurnos.JugadorActual().CambioFase();
    }
    #endregion

    #region Confirmacion Uso Carta
    public void CambiarValorSeleccionTarot(int nuevoID)
    {
        seleccionIDTarot = nuevoID;
    }

    public bool EstadoPanelConfirmacion()
    {
        return panelTarotActivo;
    }
    public void PanelConfirmacionTarot()
    {
        panelConfirmarTarot.SetActive(!panelConfirmarTarot.activeSelf);
        panelTarotActivo = panelConfirmarTarot.activeSelf;
        print(panelTarotActivo + " es el estado del panel");
    }
    #endregion
    //AQUI IRAN TODOS LOS EFECTOS DE LAS CARTAS DE TAROT
    public void ActivarEfectoCartaTarot()
    {
        Debug.Log("Ha Entrado en activar el efecto");
        Debug.Log("El ID de la carta es" + seleccionIDTarot);
        switch (seleccionIDTarot)
        {
            case 0: //El Loco --> Teleport Random (Sigues lanzando dados y no caes en casilla)
                int nuevaPos = Random.Range(0, ruta.infoCasillas.Count);
                controlTurnos.JugadorActual().IniciarTeleport(nuevaPos);
                break;
            case 1: //El Mago --> Intercambio forzado si no hay mejoras
                SeleccionOtroPersonaje();
                break;
            case 2: // La Sacerdotisa --> Tienda aleatoria entre tres cartas
                CrearCartasSacerdotisa();
                break;
            case 3: // La Emperatriz --> 5 propiedades diferentes, precio 150% (acumulable)
                controlTurnos.JugadorActual().ComprobarBuffEmperatriz();
                break;
            case 4: //El Emperador --> Doble renta una ronda
                controlTurnos.JugadorActual().ActivarEmperador();
                break;
            case 5: //El Papa --> Siguiente vez NO pagas
                controlTurnos.JugadorActual().ActivarPapa();
                break;
            case 6: //Los Enamorados --> Escoges un jugador, sumas un 50% de su dinero
                SeleccionOtroPersonaje();
                break;
            case 7: //El Carro --> Avanzar a siguiente casilla comprable

                LogicaCarro();

                break;
            case 8: //La Justicia --> El dinero de todos se iguala
                LogicaJusticia();
                break;
            case 9: //El Ermitaño --> Saltas tu siguiente turno
                controlTurnos.JugadorActual().ActivarErmitano();
                break;
            case 10: //La Rueda Fortuna --> Apuestas par/impar
                //PEREZA HACER TODO EL MECANISMO, LO DEJAREMOS PARA DESPUES
                break;
                    
            case 11: //La Fuerza --> Valor dados x2 una ronda
                controlTurnos.JugadorActual().ActivarFuerza();
                break;
                    
            case 12: //El Colgado --> Pierdes 1/2 dinero, tras dos vueltas triplicas lo invertido
                controlTurnos.JugadorActual().ActivarColgado();

                break;
                    
            case 13: //La Muerte --> Pierdes una propiedad no edificada por otra (azar)
                LogicaMuerte();   
                break;
                    
            case 14: //La Templanza --> NO interes al deshipotecar
                controlTurnos.JugadorActual().ActivarTemplanza();
                break;
                    
            case 15: //El Diablo --> Robas carta aleatoria a jugador concreto
                LogicaDiablo(); 
                break;
                    
            case 16://La Torre --> Sigues cobrando aun en la carcel
                controlTurnos.JugadorActual().ActivarTorre();
                break;
                    
            case 17://La Estrella --> Si te falta una carta para grupo, la consigues
                LogicaEstrella();  
                break;
                    
            case 18://La Luna --> Valor máximo dados 4 siguiente ronda
                controlTurnos.JugadorActual().ActivarLuna(); 
                break;
                    
            case 19://El Sol --> Casilla salida +200 (acumulable)
                controlTurnos.JugadorActual().SumarSol();
                break;
                    
            case 20://El Juicio --> Si no tienes suficiente dinero, pierdes todo el dinero salvo 100 (máximo)
                controlTurnos.JugadorActual().ActivarJuicio();
                break;
            case 21://El Mundo --> Te mueves a la casilla que quieres (cuenta como lanzar dados)
                break;
            default:
                Debug.LogError("El ID de la carta no se encuentra en la lista");
                break;
        }
        EliminarCartaClicada();
        PanelConfirmacionTarot();
    }
   
    private void EliminarCartaClicada()
    {
        for(int i = 0; i < controlTurnos.JugadorActual().inventarioCartas.transform.childCount; i++)
        {
            CartaTarot carta = controlTurnos.JugadorActual().inventarioCartas.transform.GetChild(i).GetComponent<CartaTarot>();
            if (carta.ID == seleccionIDTarot)
            {
                Destroy(carta.gameObject);
                seleccionIDTarot = -1;
            }
        }
    }
    #region MAGO
    private void SeleccionOtroPersonaje()
    {
        panelIntercambioTarot.SetActive(true);
        panelSeleccionPsjIntercambio.SetActive(true);
        
        foreach(Ficha player in controlTurnos.Jugadores)
        {
            if(player.ID != controlTurnos.JugadorActual().ID)
            {
                Button boton = Instantiate(prefabSeleccionJugador, padreButtons);
                if(seleccionIDTarot == 1)
                {
                    boton.onClick.AddListener(() => LogicaMago(player.ID));
                }
                else if(seleccionIDTarot == 6)
                {
                    boton.onClick.AddListener(() => LogicaEnamorados(player.ID));
                }
            }
        }
    }
    
    public void LogicaMago(int ID)
    {
        //Instanciar todas las calles que puedan ser intercambiadas del player y del otro
        inventarioJugadorActual = controlTurnos.JugadorActual().inventario.transform;
        inventarioJugadorSeleccionado = controlTurnos.Jugadores[ID].inventario.transform;

        InstanciarInventarios(controlTurnos.JugadorActual().ID, listaCartasPActual, inventarioJugadorActual);
        InstanciarInventarios(ID, listaCartasPSeleccionado, inventarioJugadorSeleccionado);
        panelSeleccionPsjIntercambio.SetActive(false);
    }
    private void InstanciarInventarios(int ID, List<GameObject> listaCartas, Transform inventario)
    {
        foreach(GameObject obj in controlTurnos.Jugadores[ID].InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();
            if (carta.seleccionado)
            {
                carta.seleccionado = false;
            }
            if (carta.dataCarta.grupoCompleto)
            {
                int contador = 0;
                foreach(BuildingData data in ruta.infoGrupos[carta.dataCarta.groupID].groupInfo)
                {
                    if(data.nivelCasilla > 0)
                    {
                        contador++;
                        break;
                    }
                }
                if (contador == 0)
                {
                    GameObject cartaNegociacion = Instantiate(obj, inventario);
                    listaCartas.Add(cartaNegociacion);
                }
            }
            else
            {
                GameObject cartaNegociacion = Instantiate(obj, inventario);
                listaCartas.Add(cartaNegociacion);
            }
        }
    }

    private void IntercambiarCartas()
    {
        //MIRAR EL INTERCAMBIO DE NEGOCIAR Y A VER QUE PODEMOS IMITAR. RECUERDA: TENEMOS DOS CARTAS STATIC 
        // QUE SE PUEDEN LLAMAR DESDE AQUI FACILITO
        if (cartaSeleccionadaPlayer == null || cartaSeleccionadaOtro == null) return;

        int IDJugadorSeleccionado = cartaSeleccionadaPlayer.dataCarta.dueno; //Jugador que usa la carta
        int otroID = cartaSeleccionadaOtro.dataCarta.dueno; //El otro jugador

        cartaSeleccionadaPlayer = RecuperarCartaDeInventario(IDJugadorSeleccionado, cartaSeleccionadaPlayer.ID);
        cartaSeleccionadaOtro = RecuperarCartaDeInventario(otroID, cartaSeleccionadaOtro.ID);

        //Pasamos la carta del jugador de la carta al otro
        controlTurnos.JugadorEscogido(otroID).InventarioJugador.Add(cartaSeleccionadaPlayer.gameObject);
        cartaSeleccionadaPlayer.transform.SetParent(controlTurnos.JugadorEscogido(otroID).inventario.transform);
        cartaSeleccionadaPlayer.dataCarta.dueno = otroID;
        cartaSeleccionadaPlayer.dueno.text = otroID.ToString();
        creacionCarta.CheckCarta(cartaSeleccionadaPlayer, otroID);

        // Y lo contrario aqui
        controlTurnos.JugadorEscogido(IDJugadorSeleccionado).InventarioJugador.Add(cartaSeleccionadaOtro.gameObject);
        cartaSeleccionadaOtro.transform.SetParent(controlTurnos.JugadorEscogido(IDJugadorSeleccionado).inventario.transform);
        cartaSeleccionadaOtro.dataCarta.dueno = IDJugadorSeleccionado;
        cartaSeleccionadaOtro.dueno.text = IDJugadorSeleccionado.ToString();
        creacionCarta.CheckCarta(cartaSeleccionadaOtro, IDJugadorSeleccionado);

        LimpiarBotones();
        LimpiarListas();
        panelIntercambioTarot.SetActive(false);

    }

    private Carta RecuperarCartaDeInventario(int IDJugador, int IDCarta)
    {
        for(int i = 0; i < controlTurnos.JugadorEscogido(IDJugador).InventarioJugador.Count; i++)
        {
            Carta objeto = controlTurnos.Jugadores[IDJugador].InventarioJugador[i].GetComponent<Carta>();
            if (objeto.ID != IDCarta) continue;
            return objeto;
        }
        Debug.LogError("Error: No se ha encontrado el ID de la carta clicada en el intercambio en la lista del jugador");
        return null;

    }

    private void LimpiarBotones()
    {
        for(int i = padreButtons.childCount; i >=0; i--)
        {
            Destroy(padreButtons.GetChild(i).gameObject);
        }
        PanelConfirmacionTarot();
    }

    private void LimpiarListas()
    {
        //Borramos todos los hijos que hemos creado de las cartas
        for (int i = inventarioJugadorActual.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventarioJugadorActual.transform.GetChild(i).gameObject);
        }
        for (int i = inventarioJugadorSeleccionado.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventarioJugadorSeleccionado.transform.GetChild(i).gameObject);
        }
        inventarioJugadorActual = null;
        inventarioJugadorSeleccionado = null;
        listaCartasPActual.Clear();
        listaCartasPSeleccionado.Clear();
    }
    #endregion

    #region SACERDOTISA
    private void CrearCartasSacerdotisa()
    {
        panelSacerdotisa.SetActive(true);
        int cartaCero = Random.Range(0, baseDatosTarot.Length);
        int cartaUno = Random.Range(0, baseDatosTarot.Length);
        while(cartaCero == cartaUno)
        {
            cartaCero = Random.Range(0, baseDatosTarot.Length);
        }
        int cartaDos = Random.Range(0, baseDatosTarot.Length);

        while(cartaCero == cartaDos || cartaUno == cartaCero)
        {
            cartaDos = Random.Range(0, baseDatosTarot.Length);
        }       
        EscribirDatosCartas(0, cartaCero);
        EscribirDatosCartas(1, cartaUno);
        EscribirDatosCartas(2, cartaDos);
       
    }

    private void EscribirDatosCartas(int pos, int numCarta)
    {
        CartaTarot carta = variablesSacerdotisa[pos].carta;
        TarotSO tarotInfo = baseDatosTarot[numCarta];

        carta.gameObject.name = tarotInfo.name;
        carta.ID= tarotInfo.ID;
        carta.precio = tarotInfo.precio;
        carta.titulo.text = tarotInfo.nombre;
        carta.descripcion.text = tarotInfo.descripcionCarta;
        
    }
    public void SeleccionCartaSacerdotisa(int posBoton)
    {
        CartaTarot cartaComprada = Instantiate(variablesSacerdotisa[posBoton].carta, controlTurnos.JugadorActual().inventarioCartas.transform);
        cartaComprada.enTienda = false;
        cartaComprada.name = cartaComprada.titulo.text;
        cartaComprada.dueno = controlTurnos.JugadorActual().ID;
        panelSacerdotisa.SetActive(false);
        PanelConfirmacionTarot();
    }
    #endregion

    #region ENAMORADOS
    public void LogicaEnamorados(int playerSeleccionado)
    {
        int dineroASumar = controlTurnos.Jugadores[playerSeleccionado].dinero / 2;
        economia.GanarDinero(dineroASumar, controlTurnos.JugadorActual().ID);
        LimpiarBotones();
    }
    #endregion

    #region JUSTICIA
    private void LogicaJusticia()
    {
        int dineroTotal = 0;

        for(int i = 0; i < controlTurnos.Jugadores.Count; i++)
        {
            dineroTotal += controlTurnos.Jugadores[i].dinero;
        }

        int dineroParaCadaJugador = dineroTotal / controlTurnos.Jugadores.Count;
        
        for (int i = 0; i < controlTurnos.Jugadores.Count; i++)
        {
            economia.ActualizarDinero(dineroParaCadaJugador, i);
        }
    }
    #endregion

    #region CARRO
    private void LogicaCarro()
    {
        controlTurnos.JugadorActual().ActivarCarro();
        int posPlayer = controlTurnos.JugadorActual().routePosition;
        int numSiguienteComprable = -1;
        for(int i = posPlayer + 1; posPlayer < ruta.infoCasillas.Count; i++)
        {
            if (ruta.infoCasillas[i].comprable)
            {
                numSiguienteComprable = i;
                break;
            }
        }

        if(numSiguienteComprable == -1) // SI ES -1 IMPLICA QUE EN ESA VUELTA NO HAY NINGUNA DISPONIBLE
        {
            for(int i = 0; i < ruta.infoCasillas.Count; i++)
            {
                if (ruta.infoCasillas[i].comprable)
                {
                    numSiguienteComprable = i;
                    break;
                }
            }
        }

        if(numSiguienteComprable == -1) //ESTO ES LO QUE OCURRIRA SI NO HAY NINGUNA CASILLA COMPRABLE
        {
            controlTurnos.JugadorActual().casillasAvanzar = 0;
            return;
        }
        else
        {
            int casillasAMover = 0;
            if(numSiguienteComprable < posPlayer)
            {
                casillasAMover = ruta.infoCasillas.Count - posPlayer + numSiguienteComprable;
                int dineroSalida = 200 + (200 * controlTurnos.JugadorActual().NumeroSoles());
                economia.GanarDinero(dineroSalida, controlTurnos.ContadorTurnos());
                if (controlTurnos.JugadorActual().EsColgado())
                {
                    controlTurnos.JugadorActual().AumentarVueltaColgado();                    
                }
            }
            else
            {
                casillasAMover = numSiguienteComprable - posPlayer;
            }

            controlTurnos.JugadorActual().casillasAvanzar = casillasAMover;
            controlTurnos.JugadorActual().posicionCasilla = numSiguienteComprable;
            StartCoroutine(controlTurnos.JugadorActual().Move());
        }
              

    }

    #endregion

    #region MUERTE
    private void LogicaMuerte()
   {
        List<GameObject> casillasEscogibles = new();
        List<BuildingData> casillasComprables = new();
        foreach(GameObject obj in controlTurnos.JugadorActual().InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();
            if (!carta.dataCarta.grupoCompleto)
            {
                casillasEscogibles.Add(obj);
            }
            else
            {               
                int contador = 0;
                foreach (BuildingData data in ruta.infoGrupos[carta.dataCarta.groupID].groupInfo)
                {
                    if (data.nivelCasilla > 0)
                    {
                        contador++;
                        break;
                    }
                }
                if (contador == 0)
                {
                    casillasEscogibles.Add(obj);
                }
            }
        }

        foreach(BuildingData data in ruta.infoCasillas)
        {
            if (!data.comprado && data.comprable)
            {
                casillasComprables.Add(data);
            }
        }
        if (casillasComprables.Count == 0) return; //NO SE SI HACERLO ASI O NO LA VERDAD, Y DEJARLES PERDER

        int numeroAleatorioInventario = Random.Range(0, casillasEscogibles.Count);
        int numeroAleatorioComprable = Random.Range(0, casillasComprables.Count);

        GameObject objetoSeleccionado = casillasEscogibles[numeroAleatorioInventario];
        controlTurnos.JugadorActual().InventarioJugador.Remove(objetoSeleccionado);
        Destroy(objetoSeleccionado);

        BuildingData cartaComprableData = casillasComprables[numeroAleatorioComprable];
        cartaComprableData.dueno = controlTurnos.JugadorActual().ID;
        cartaComprableData.comprado = true;
        creacionCarta.CrearCarta(cartaComprableData);
        

    }
    #endregion

    #region DIABLO
    private void LogicaDiablo()
    {
        List<Ficha> jugadoresConCartas = new();
        foreach(Ficha player in controlTurnos.Jugadores)
        {
            if(player.ID == controlTurnos.JugadorActual().ID)
            {
                continue;
            }

            if(player.inventarioCartas.transform.childCount > 0)
            {
                jugadoresConCartas.Add(player);
            }

        }

        int jugadorAEscoger = Random.Range(0, jugadoresConCartas.Count);
        int numCartaEscogida = Random.Range(0, jugadoresConCartas[jugadorAEscoger].inventarioCartas.transform.childCount);

        CartaTarot cartaEscogida = jugadoresConCartas[jugadorAEscoger].transform.GetChild(numCartaEscogida).GetComponent<CartaTarot>();
        cartaEscogida.transform.SetParent(controlTurnos.JugadorActual().inventarioCartas.transform);
        cartaEscogida.dueno = controlTurnos.JugadorActual().ID;

    }
    #endregion

    #region ESTRELLA
    private void LogicaEstrella()
    {
       //AÑADIMOS TODAS LAS CALLES QUE CUMPLAN LOS SIGUIENTES REQUISITOS: 
       // QUE FALTE UNA SOLA CALLE PARA QUE EL JUGADOR CUMPLA EL GRUPO, DA IGUAL DE QUIEN SEA POR AHORA
        
        List<BuildingData> listaPosiblesCalles = new();


        foreach(GameObject obj in controlTurnos.JugadorActual().InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();

            BuildingData data = carta.dataCarta;

            int contadorCasillas = 0;
            foreach(BuildingData dataGrupo in ruta.infoGrupos[data.groupID].groupInfo)
            {
                if(dataGrupo.dueno == controlTurnos.JugadorActual().ID)
                {
                    contadorCasillas++; 
                }
            }

            if(contadorCasillas == ruta.infoGrupos[data.groupID].groupInfo.Count - 1)
            {
                foreach(BuildingData dataCalle in ruta.infoGrupos[data.groupID].groupInfo)
                {
                    if(dataCalle.dueno != controlTurnos.JugadorActual().ID)
                    {
                        listaPosiblesCalles.Add(dataCalle);
                    }
                }
            }
        }

        // CREAMOS DOS LISTAS, UNA PARA LA BANCA Y OTRA PARA EL RESTO PARA QUE SEA ALEATORIO
        List<BuildingData> listaBanca = new();
        List<BuildingData> listaOtrosPlayers = new();
        for(int i = 0; i < listaPosiblesCalles.Count; i++)
        {
            if (listaPosiblesCalles[i].dueno == -1) //USAREMOS -1 PARA LA BANCA
            {
                listaBanca.Add(listaPosiblesCalles[i]);
            }
            else if (listaPosiblesCalles[i].dueno != controlTurnos.JugadorActual().ID)
            {
                listaOtrosPlayers.Add(listaPosiblesCalles[i]);
            }
        }

        if(listaBanca.Count > 0) // ESTO SERA SI EL BANCO TIENE ALGUNA, PRIORIZAREMOS ESA
        {
            int numCalleAleatoria = Random.Range(0, listaBanca.Count);
            creacionCarta.CrearCarta(listaBanca[numCalleAleatoria]);

        }
        else //SI NO, SE LO ROBAREMOS A LOS JUGADORES
        {
            if (listaOtrosPlayers.Count < 1) return;  // <<<<< QUIZA AQUI HAYA QUE METER MAS LOGICA, COMO DESACTIVAR PANEL ETC

            int numCalleAleatoria = Random.Range(0, listaOtrosPlayers.Count);
            BuildingData dataCalleEscogida = listaOtrosPlayers[numCalleAleatoria];
            int duenoAnterior = dataCalleEscogida.dueno;
            foreach(GameObject obj in controlTurnos.Jugadores[duenoAnterior].InventarioJugador)
            {
                BuildingData infoCarta = obj.GetComponent<Carta>().dataCarta;
                if (infoCarta.IDentificador == dataCalleEscogida.IDentificador)
                {
                    controlTurnos.Jugadores[duenoAnterior].InventarioJugador.Remove(obj);
                    Destroy(obj);
                }
            }
            creacionCarta.CrearCarta(dataCalleEscogida);
            
        }

      
    }
    #endregion
}

