using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Hipotecacion : MonoBehaviour
{
    public Transform padreHipotecacion;
    public List<Carta> listaCartas = new();
    public GameManager gameManager;
    public GameObject panelHipoteca;
    public Button botonHipotecar;
    public TextMeshProUGUI textoDineroActual;
    public TextMeshProUGUI textoDineroAPagar;

    private int dineroActual;
    private int dineroAPagar;

    private int jugadorARecibir;

    private int jugadoresDerrotados;
    private Economia economia;
    private Turnos turnos;
    private Ficha jugadorEnHipotecacion;
    public List<int> cartasAHipotecarID = new(); // Lista de ID para que al aceptar el boton de pago, hipotequemos las casas con ese ID
    static int SortByID(Carta c1, Carta c2)
    {
        return c1.dataCarta.groupID.CompareTo(c2.dataCarta.groupID);
    }

    private void Start()
    {
        economia = GameObject.FindFirstObjectByType<Economia>();
        turnos = GameObject.FindFirstObjectByType<Turnos>();
    }

    public void ActivarPantallaHipotecacion(Ficha jugadorActual, int deuda, int jugadorAPagar)
    {
        // Activamos el panel, desactivamos el boton y borramos la lista. Actualizamos los dineros que tenemos y debemos
        panelHipoteca.SetActive(true);
        botonHipotecar.interactable = false;
        listaCartas.Clear();
        cartasAHipotecarID.Clear();

        dineroActual = jugadorActual.dinero;
        dineroAPagar = deuda;
        textoDineroActual.text = "Dinero actual: " + dineroActual.ToString();
        textoDineroAPagar.text = "Deuda: " + dineroAPagar.ToString();

        jugadorARecibir = jugadorAPagar;
        jugadorEnHipotecacion = jugadorActual;

        // Creamos la lista con los objetos del inventario del jugador que le toque y la ordenamos (actualmente por ID)
        foreach(GameObject obj in jugadorActual.InventarioJugador)
        {            
            GameObject cartaInstanciada = Instantiate(obj);
            Carta cartaCreada = cartaInstanciada.GetComponent<Carta>();
            cartaCreada.enHipotecacion = true;
            int posicionPadreBotones = cartaInstanciada.transform.childCount - 1;
            Transform padreBotones = cartaInstanciada.transform.GetChild(posicionPadreBotones);
            if (cartaCreada.dataCarta.nivelCasilla > 0)
            {
                padreBotones.gameObject.SetActive(true);
            }                      

            cartaCreada.botonPlus.onClick.AddListener(() => RecuperarNivelMejora(cartaCreada));
            cartaCreada.botonMinus.onClick.AddListener(() => BajarNivelMejora(cartaCreada));
            
            listaCartas.Add(cartaCreada);
        }
       
        listaCartas.Sort(SortByID);

        // Ordenamos las cartas en el orden de la lista para que aparezcan en el orden que hemos hecho
        for(int i= 0; i < listaCartas.Count; i++)
        {
            listaCartas[i].transform.SetParent(padreHipotecacion);
            listaCartas[i].transform.localScale = Vector3.one; //No se porque esto es necesario pero sino, la escala esta en 2,algo
        }        
    }   

    // Funcion al clicar la carta y seleccionarla. HARA FALTA ALGO VISUAL PARA SABERLO
    public void SumarHipoteca(Carta carta)
    {
        if (carta.dataCarta.nivelCasilla > 0) return;
        cartasAHipotecarID.Add(carta.ID);
        dineroActual += carta.dataCarta.precioCasilla / 2;
        textoDineroActual.text = "Dinero actual: " + dineroActual.ToString();


        if (dineroActual >= dineroAPagar)
        {
            botonHipotecar.interactable = true;
        }
        Debug.LogWarning("Carta añadida a hipotecacion");
        carta.clicadoHipotecacion = true; 
    }

    // Funcion al clicar la carta y deseleccionarla
    public void QuitarHipoteca(Carta carta)
    {
        if (carta.dataCarta.nivelCasilla > 0) return;
        cartasAHipotecarID.Remove(carta.ID);
        dineroActual -= carta.dataCarta.precioCasilla / 2;
        textoDineroActual.text = "Dinero actual: " + dineroActual.ToString();



        if (dineroActual < dineroAPagar)
        {
            botonHipotecar.interactable = false;
        }
        Debug.LogWarning("Carta quitada de hipotecacion");

        carta.clicadoHipotecacion = false; 
    }

    // Botones Plus y Minus para recuperar la mejora que has hecho o destruirla
    public void BajarNivelMejora(Carta carta)
    {
        if (carta.dataCarta.nivelCasilla <= 0) return;
        carta.dataCarta.nivelCasilla--;
        dineroActual += carta.dataCarta.precioMejora / 2;
        textoDineroActual.text = "Dinero actual: " + dineroActual.ToString();


        if (dineroActual >= dineroAPagar)
        {
            botonHipotecar.interactable=true;
        }
    }
    public void RecuperarNivelMejora(Carta carta)
    {
        if(carta.dataCarta.nivelCasilla < carta.nivelMejoraActual)
        {
            carta.dataCarta.nivelCasilla++;
            dineroActual -= carta.dataCarta.precioMejora / 2;
            textoDineroActual.text = "Dinero actual: " + dineroActual.ToString();


            if (dineroActual <= dineroAPagar)
            {
                botonHipotecar.interactable = false;
            }
        }
    }

    // Boton Hipotecar
    public void HipotecarLoSeleccionado()
    {
        // Buscamos todos los obj seleccionados que se vayan a hipotecar en la lista del Jugador y les cambiamos el dueño
        foreach(GameObject obj in jugadorEnHipotecacion.InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();
            if (cartasAHipotecarID.Contains(carta.dataCarta.IDentificador))
            {
                carta.dataCarta.hipoteca = true; // Hipotecamos la carta
                Destroy(obj); //No se si los destruimos? Supongo que si
            }
            
            // Esto implicara que se ha modificado el nivel de la casilla de esa carta
            if(carta.dataCarta.nivelCasilla != carta.nivelMejoraActual)
            {
                //Destruir el prefab actual e instanciar la que toca en el mismo sitio
                //Instantiate(carta.dataCarta.arrayPrefabs[carta.dataCarta.nivelCasilla]);
                carta.nivelMejoraActual = carta.dataCarta.nivelCasilla;
            }

        }
        

        // Actualizamos el dinero de ambos (si hay dos)
        jugadorEnHipotecacion.dinero = dineroActual - dineroAPagar;
        if (jugadorARecibir >= 0)
        {
            turnos.JugadorEscogido(jugadorARecibir).dinero += dineroAPagar;
        }

        // turnos.PasarTurnoAlSiguiente();
        LimpiarLista();
        // Hace falta el cambioFase
        jugadorEnHipotecacion.CambioFase();
        panelHipoteca.SetActive(false);
    }


    // Boton Abandonar el juego    

    public void AbandonarJugador()
    {
        foreach(GameObject obj in jugadorEnHipotecacion.InventarioJugador)
        {
            Carta cartaEnInventario = obj.GetComponent<Carta>();
            cartaEnInventario.dataCarta.dueno = jugadorARecibir;
            cartaEnInventario.dataCarta.comprado = jugadorARecibir != -1; // -1 es el banco ahora mismo
            cartaEnInventario.botonMinus.onClick.RemoveAllListeners();
            cartaEnInventario.botonPlus.onClick.RemoveAllListeners();
            if(jugadorARecibir > -1)
            {
                turnos.JugadorEscogido(jugadorARecibir).InventarioJugador.Add(obj); //Añadimos los edificios al nuevo jugador
            }
        }               
        jugadorEnHipotecacion.InventarioJugador.Clear();

        //Para asegurarnos de que es un jugador humano, recordar -1 = banco ahora mismo
        if (jugadorARecibir > -1)
        {
            economia.GanarDinero(dineroActual, jugadorARecibir);
            turnos.Jugadores[jugadorARecibir].OrdenarInventarioPropio();
        }

        jugadorEnHipotecacion.dinero = 0;

        //------------29/11 ------------
       // economia.txtDineros[jugadorEnHipotecacion.ID].text = jugadorEnHipotecacion.dinero.ToString();
        
        jugadorEnHipotecacion.enDerrota = true;
        LimpiarLista();
        DeteccionFinalPartida();
        // No se si hace falta el cambioFase la verdad, pero en otros lados se ponen los dos.
        // Quiza haya que crear una variante de pasar el turno al siguiente porque estamos modificando la lista. 
        turnos.PasarTurnoAlSiguiente();
       
        //Eliminamos de las listas el jugador actual
        //economia.JugadoresTurnos.Remove(jugadorEnHipotecacion);
        panelHipoteca.SetActive(false);


        
    }

    void DeteccionFinalPartida()
    {
        jugadoresDerrotados = 0;

        //-----------------------------29/11------------------------
        // REFACTORIZACION DEL CODIGO
        /*
        foreach (Ficha player in economia.JugadoresTurnos)
        {
            if (player.enDerrota)
            {
                jugadoresDerrotados++;
            }
        }
        // Si hay 3 jugadores derrotados implica que ya tenemos un ganador
        if (jugadoresDerrotados == 3)
        {
            foreach(Ficha player in economia.JugadoresTurnos)
            {
                if (!player.enDerrota)
                {
                    Debug.LogWarning("Enhorabuena " + player.ID + ". Has ganado la partida!!!");

                    // Toda la logica de victoria va aqui
                }
            }
        }*/
    }

    void LimpiarLista()
    {
        for(int i = padreHipotecacion.childCount - 1; i >= 0; i--)
        {
            Destroy(padreHipotecacion.GetChild(i).gameObject);
        }
    }
  
}
