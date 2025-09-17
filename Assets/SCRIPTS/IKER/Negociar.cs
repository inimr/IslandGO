using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Negociar : MonoBehaviour
{

    private Economia economia;
    private CreacionCarta creacionCarta;
    private Turnos turnos;
    public List<Button> botones;

    public int IDboton;
    [Header ("Variables de la negociacion")]

    public GameObject panelNegociacion;

    [Header ("Variables de cada jugador")]

    [Header ("Jugador negociante")]

    public TextMeshProUGUI txtPlayer;
    public TextMeshProUGUI textoDinero;
    public int dineroIntercambio;
    [SerializeField] private GameObject inventarioJugador;
    public Button botonSumaP1;
    public Button botonRestaP1;   
    public List <GameObject> listaCartasJ1 = new();

    [Header ("Jugador negociado")]

    public TextMeshProUGUI txtPlayer2;
    [SerializeField] private TextMeshProUGUI textoDinero2;
    [SerializeField] public int dineroIntercambio2;
    [SerializeField] private GameObject inventarioJugador2;
    public Button botonSumaP2;
    public Button botonRestaP2;
    private List <GameObject> listaCartasJ2 = new();


    private void Awake()
    {
        economia = FindAnyObjectByType<Economia>();      

    }
    // Start is called before the first frame update
    void Start()
    {   
        panelNegociacion.SetActive(false);
        creacionCarta = FindFirstObjectByType<CreacionCarta>();
        turnos = FindFirstObjectByType<Turnos>();

    }  
    public void DesactivarBoton()
    {
        // Lo he intentado con corrutinas y eventos de animacion y todo pero esto parece lo mas practico y facil
        for(int i = 0; i < botones.Count; i++)
        {
            botones[i].interactable = true;
        }

        botones[turnos.ContadorTurnos()].interactable = false;
    }
    public void ActivarBoton()
    {
        botones[turnos.ContadorTurnos()].interactable = true;
    }

    //NumBoton y MostrarPanelNegociacion los hacen los botones de la izquierda, en ese orden
    public void NumBoton(int ID)
    {
        IDboton = ID;

    }
    public void MostrarPanelNegociacion()
    {        
        panelNegociacion.SetActive(true);
        txtPlayer.text = turnos.JugadorActual().ToString();      
        txtPlayer2.text = turnos.JugadorEscogido(IDboton).ToString();

        InstanciarInventarios();
    }
    // Aqui pasaremos las cartas a cada uno de los inventarios
    public void InstanciarInventarios()
    {

        //Accedemos al inventario del Jugador Negociante y creamos otra lista
        foreach (GameObject obj in turnos.JugadorActual().InventarioJugador)
        {
            //Si esta seleccionada lo deseleccionamos ahora
            Carta carta = obj.GetComponent<Carta>();
            if (carta.seleccionado)
            {
                //Hay que volverlo a su posicion NO clicada
                carta.seleccionado = false;
            }
            GameObject cartaNegociacion = Instantiate(obj, inventarioJugador.transform);
            listaCartasJ1.Add(cartaNegociacion);
        }

        //Accedemos al inventario del Jugador Negociado y creamos otra lista con esas cartas
        foreach (GameObject obj in turnos.JugadorEscogido(IDboton).InventarioJugador)
        {
            //Si esta seleccionada lo deseleccionamos ahora
            Carta carta = obj.GetComponent<Carta>();
            if (carta.seleccionado)
            {
                //Hay que volverlo a su posicion NO clicada
                carta.seleccionado = false;
            }
            GameObject cartaNegociacion = Instantiate(obj, inventarioJugador2.transform);
            listaCartasJ2.Add(cartaNegociacion);
        }     

    }

    //Metodo que lo ejecuta el boton de aceptar el intercambio
    public void Intercambio2()
    {       

        List<Carta> listaTempNegociante = new(); //Lista de las cartas seleccionadas del jugador negociante
        List<Carta> listaTempNegociado = new();  //Lista de las cartas seleccionadas del jugador negociado
        List<int> listaIDNegociante = new(); //Lista con los ID de las cartas seleccionadas del negociante
        List<int> listaIDNegociado = new(); // Lista con los ID de las cartas seleccionadas del negociado

        //Añadimos las cartas seleccionadas a cada una de las listas
        foreach(GameObject obj in listaCartasJ1)
        {
            Carta carta = obj.GetComponent<Carta>();
            if (carta.seleccionado)
            {
                listaTempNegociante.Add(carta);
                listaIDNegociante.Add(carta.ID);
            }          
        }
        foreach (GameObject obj in listaCartasJ2)
        {
            Carta carta = obj.GetComponent<Carta>();
            if (carta.seleccionado)
            {
                listaTempNegociado.Add(carta);
                listaIDNegociado.Add(carta.ID);
            }
        }
        
        //Eliminamos las cartas seleccionadas de la lista original
        //Como queremos ir eliminando las cosas de la lista, usamos el if inverso porque el foreach da error
   
        for(int i = turnos.JugadorActual().InventarioJugador.Count - 1; i >= 0; i--)
        {
            GameObject objeto = turnos.JugadorActual().InventarioJugador[i];
            Carta carta = objeto.GetComponent<Carta>();
            if (listaIDNegociante.Contains(carta.ID))
            {
                turnos.JugadorActual().InventarioJugador.Remove(objeto);

                //Movemos la ficha a su nueva lista y ubicacion con su nuevo dueño

                turnos.JugadorEscogido(IDboton).InventarioJugador.Add(objeto);
                objeto.transform.SetParent(turnos.JugadorEscogido(IDboton).inventario.transform);
                carta.dataCarta.dueno = IDboton;
                carta.dueno.text = IDboton.ToString();
                creacionCarta.CheckCarta(carta, IDboton);
            }
        }
        for (int i = turnos.JugadorEscogido(IDboton).InventarioJugador.Count - 1; i >= 0; i--)
        {
            GameObject objeto = turnos.JugadorEscogido(IDboton).InventarioJugador[i];
            Carta carta = objeto.GetComponent<Carta>();
            if (listaIDNegociado.Contains(carta.ID))
            {
                turnos.JugadorEscogido(IDboton).InventarioJugador.Remove(objeto);
                //Movemos la ficha a su nueva lista y ubicacion

                turnos.JugadorActual().InventarioJugador.Add(objeto);
                objeto.transform.SetParent(turnos.JugadorActual() .inventario.transform);
                carta.dataCarta.dueno = turnos.ContadorTurnos();
                carta.dueno.text = turnos.ContadorTurnos().ToString();
                creacionCarta.CheckCarta(carta, turnos.ContadorTurnos());
            }
        } 
        //Limpiamos las listas para reusarlas mas adelante
        listaCartasJ1.Clear();
        listaCartasJ2.Clear();

        //Borramos todos los hijos que hemos creado de las cartas
        for(int i = inventarioJugador.transform.childCount -1; i >=0 ; i--)
        {
            Destroy(inventarioJugador.transform.GetChild(i).gameObject);
        }
        for (int i = inventarioJugador2.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventarioJugador2.transform.GetChild(i).gameObject);
        }

        turnos.JugadorActual().OrdenarInventarioPropio(); //<<<<
        turnos.JugadorEscogido(IDboton).OrdenarInventarioPropio(); //<<<<

        //ACTUALIZAMOS LOS DINEROS

        economia.PerderDinero(dineroIntercambio, turnos.ContadorTurnos());
        economia.GanarDinero(dineroIntercambio, IDboton);
        economia.PerderDinero(dineroIntercambio2, IDboton);
        economia.GanarDinero(dineroIntercambio2, turnos.ContadorTurnos());

        
        dineroIntercambio = 0;
        dineroIntercambio2 = 0;

        textoDinero.text = "Cantidad = " + dineroIntercambio.ToString();
        textoDinero2.text = "Cantidad = " + dineroIntercambio2.ToString();

        panelNegociacion.SetActive(false);

        //_________________________________SEPARACION CODIGO NUEVO-VIEJO_____________________________________________

       /* CambioDeSitioCartas(IV_J1, IV_J2, IDboton, economia.Contador);
        CambioDeSitioCartas(IV_J2, IV_J1, economia.Contador, IDboton);

        
        //GM.UIPlayer[IDboton].transform.Find("Inventory").transform


        economia.JugadoresTurnos[economia.Contador].dinero -= dineroIntercambio;
        economia.JugadoresTurnos[IDboton].dinero += dineroIntercambio;


        dineroIntercambio = 0;
        textoDinero.text = "Cantidad = " + dineroIntercambio.ToString();


        economia.JugadoresTurnos[IDboton].dinero -= dineroIntercambio2;

        economia.JugadoresTurnos[economia.Contador].dinero = economia.JugadoresTurnos[economia.Contador].dinero + dineroIntercambio2;
        economia.txtDineros[IDboton].text = economia.JugadoresTurnos[IDboton].dinero.ToString();
        economia.txtDineros[economia.Contador].text = economia.JugadoresTurnos[economia.Contador].dinero.ToString();

        dineroIntercambio2 = 0;
        textoDinero2.text = "Cantidad = " + dineroIntercambio.ToString();*/
    }  

    //Boton Cancelar del panelNegociacion
    public void OcultarPanelNegociacion()
    {
        panelNegociacion.SetActive(false);

        //Limpiamos las listas para reusarlas mas adelante
        listaCartasJ1.Clear();
        listaCartasJ2.Clear();

        //Borramos todos los hijos que hemos creado de las cartas
        for (int i = inventarioJugador.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventarioJugador.transform.GetChild(i).gameObject);
        }
        for (int i = inventarioJugador2.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventarioJugador2.transform.GetChild(i).gameObject);
        }

        dineroIntercambio = 0;
        dineroIntercambio2 = 0;

        textoDinero.text = "Cantidad = " + dineroIntercambio.ToString();
        textoDinero2.text = "Cantidad = " + dineroIntercambio2.ToString();       
    } 
    //LOGICA DE LOS BOTONES DE SUMAR Y RESTAR DINERO AL INTERCAMBIO

    public void Suma(int cantidad) 
    {
        dineroIntercambio += cantidad; 
        textoDinero.text = "Cantidad = " + dineroIntercambio.ToString();
        BloquearBotonSuma(botonSumaP1, turnos.JugadorActual().dinero, dineroIntercambio, cantidad);
    }
    public void Resta(int cantidad)
    {
        dineroIntercambio -= cantidad;
        textoDinero.text = "Cantidad = " + dineroIntercambio.ToString();
        BloquearBotonSuma(botonSumaP1, turnos.JugadorActual().dinero,dineroIntercambio, cantidad);

    }

    public void Suma2(int cantidad) 
    {
        dineroIntercambio2 += cantidad; 
        textoDinero2.text = "Cantidad = " + dineroIntercambio2.ToString();
        BloquearBotonSuma(botonSumaP2, turnos.JugadorEscogido(IDboton).dinero,dineroIntercambio2, cantidad);

    }
    public void Resta2(int cantidad) 
    {
        dineroIntercambio2 -= cantidad;
        textoDinero2.text = "Cantidad = " + dineroIntercambio2.ToString();
        BloquearBotonSuma(botonSumaP2, turnos.JugadorEscogido(IDboton).dinero, dineroIntercambio2, cantidad);

    }

    public void BloquearBotonSuma(Button boton, int dinero, int dineroPuesto, int cantidad)
    {        
        boton.interactable = dinero - dineroPuesto >= cantidad;
        
    }
}
