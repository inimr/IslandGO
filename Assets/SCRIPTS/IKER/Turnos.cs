using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class Turnos : MonoBehaviour
{
    public List<Ficha>Jugadores;
    UIController uiController;
    Usernames usernames;
   // public int contadorTurnos = 0;
    [Space(10)]
    [Header("Players")]
    [Space(10)]
    private GameObject p1;
    private GameObject p2;
    private GameObject p3;
    private GameObject p4;
    [Space(10)]
    [Header("Scripts de jugadores")]
    [Space(10)]
    private Ficha Jugador_1;
    private Ficha Jugador_2;
    private Ficha Jugador_3;
    private Ficha Jugador_4;
    private int contadorTurno;

    [SerializeField] private Ficha prefabJugador;
    

    // Start is called before the first frame update
    void Awake()
    {
        ///-----------------05/12-------------------
        ///TENDREMOS QUE CREAR MANUALMENTE LOS OBJETOS TIPO FICHA Y ASIGNARLES TODO LO NECESARIO EN UN LOOP
        ///PARA ELLO, TENDREMOS QUE ENCONTRAR CUANTOS JUGADORES SON DEL SCRIPT QUE VENGA DE LA OTRA ESCENA
        usernames = FindAnyObjectByType<Usernames>();

        if(usernames != null)
        {
            for(int i = 0; i < usernames.userdataList.Count; i++)
            {
                InstanciarJugador(i); //MAS INFO ABAJO
            }
        }

        //Referenciamos los objetos JUGADOR
        p1 =GameObject.Find("Ficha");
        p2 =GameObject.Find("Ficha1");
        p3 =GameObject.Find("Ficha2");
        p4 =GameObject.Find("Ficha3");

        //sacamos los scripts de esos objetos

        Jugador_1 = p1.GetComponent<Ficha>();
        Jugador_2 = p2.GetComponent<Ficha>();
        Jugador_3 = p3.GetComponent<Ficha>();
        Jugador_4 = p4.GetComponent<Ficha>();

        //a?adimos a la lista los scripts

        Jugadores.Add(Jugador_1);
        Jugadores.Add(Jugador_2);
        Jugadores.Add(Jugador_3);
        Jugadores.Add(Jugador_4);

        //hacemos refencia a los scripts
        uiController = FindAnyObjectByType<UIController>();

        Jugadores[0].Turno = true;
        
    }
    public void InstanciarJugador(int numero)
    {
        //Aqui tendremos que acceder a la lista de posibles piezas, e instanciar esas
        //Añadir el componente Ficha (si no lo hemos añadido al prefab)
        //Y modificar los valores que queramos para que salgan bien
    }
 
    public void IniciarTurnoJugador(int id)
    {
        uiController.PanelInventarioJugador(ContadorTurnos(), true);
        uiController.ResaltoUIData(ContadorTurnos(), true);

        Jugadores[id].Turno = true;
        Jugadores[id].DesactivarEmperador();
        Jugadores[id].estadoActual = EstadoTurnoJugador.FaseDados;
        Jugadores[id].CambioFase();

    }

    public void PasarTurnoAlSiguiente()
    {

        uiController.PanelInventarioJugador(ContadorTurnos(), false);

        uiController.ResaltoUIData(ContadorTurnos(), false);
        // Cambiar al siguiente jugador de manera ciclica
        JugadorActual().Turno = false;
        contadorTurno++;


        if(contadorTurno >= Jugadores.Count)
        {
            contadorTurno = 0; // Volver al primer jugador cuando termine el ciclo
        }

        if (JugadorActual().enDerrota || JugadorActual().EsErmitano())
        {
            JugadorActual().DesactivarErmitano();
            PasarTurnoAlSiguiente();
            return;
        }
        IniciarTurnoJugador(ContadorTurnos());

    }
    #region  Metodos para variables privadas
    public int ContadorTurnos()
    {
        return contadorTurno;
    }

    public Ficha JugadorActual()
    {
        return Jugadores[ContadorTurnos()];
    }

    public Ficha JugadorEscogido(int jugador)
    {
        return Jugadores[jugador];
    }
    public List<Ficha> ListaJugadores()
    {
        return Jugadores;
    }
    #endregion 


}

