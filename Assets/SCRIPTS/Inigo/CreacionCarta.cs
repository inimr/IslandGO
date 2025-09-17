using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CreacionCarta : MonoBehaviour
{

    private Turnos turnos;
    private UIController controladorUI;
    private Ruta tablero;

  
    private void Start()
    {
        turnos = FindFirstObjectByType<Turnos>();
        controladorUI = FindFirstObjectByType<UIController>();
        tablero = FindFirstObjectByType<Ruta>();
    }

  
    //<<<<<<<<<<<<<<<<< 2025/04/10 >>>>>>>>>>>>>>>>>>>>>>
    // MIRAR SI turnos.Jugadores[turnos.ContadorTurnos()] se puede cambiar por turnos.JugadorActual()
    // para hacer el codigo menos cargado visualmente

    public void CrearCarta(BuildingData infoCasilla)
    {
        Ficha jugador = turnos.Jugadores[turnos.ContadorTurnos()];

        GameObject cartaInstanciada = EscribirDatosCarta(infoCasilla);//, inventario.transform);       
        Carta cartaCreada = cartaInstanciada.GetComponent<Carta>();
        jugador.InventarioJugador.Add(cartaInstanciada);
        CheckCarta(cartaCreada, turnos.ContadorTurnos());
        jugador.OrdenarInventarioPropio();
        jugador.ComprobarBuffEmperatriz();
        cartaInstanciada.transform.localScale = Vector3.one; //No se porque la escala hay que reajustarsela

    }
    // En este metodo añadimos todos los valores que necesita la carta para que se vea como tiene que verse
    public GameObject EscribirDatosCarta(BuildingData data)//, Transform inventarioJugador)
    {
        GameObject cartaInstanciada = Instantiate(controladorUI.CartaInventario());//, inventarioJugador);
        Carta cartaCreada = cartaInstanciada.GetComponent<Carta>();

        cartaCreada.ID = data.IDentificador;
        cartaCreada.dataCarta = data;
        cartaInstanciada.name = data.nombreEdificio;
        cartaCreada.tituloCarta.text = cartaCreada.dataCarta.nombreEdificio;
        cartaCreada.dueno.text = cartaCreada.dataCarta.dueno.ToString();

        for (int i = 0; i < cartaCreada.arrayAlquilerNombres.Length; i++)
        {
            cartaCreada.arrayAlquilerTxt[i].text = cartaCreada.dataCarta.arrayPreciosAlquiler[i].ToString();
        }

        cartaCreada.alquilerBaseConColorTxt.text = cartaCreada.dataCarta.alquilerColorCompleto.ToString();
        cartaCreada.arrayAlquilerNombres[cartaCreada.nivelMejoraActual].color = cartaCreada.colorNivelActual;
        cartaCreada.arrayAlquilerTxt[cartaCreada.nivelMejoraActual].color = cartaCreada.colorNivelActual;

        cartaCreada.precioMejoraTxt.text = cartaCreada.dataCarta.precioMejora.ToString();
        cartaCreada.reembolsoHipotecaMejoraTxt.text = (cartaCreada.dataCarta.precioMejora / 2).ToString();
        cartaCreada.hipotecaTxt.text = (cartaCreada.dataCarta.precioCasilla / 2).ToString();

        cartaCreada.colorCasa.color = cartaCreada.dataCarta.colorCasa;

        return cartaInstanciada;
    }

    public void CheckCarta(Carta cartaCreada, int jugador)
    {
        //Miramos si el jugador tiene todas las cartas del grupo, por ahora todo funcionara igual, incluso estaciones, luego, ya veremos.
        //Para mirar todas las cartas que no sean del grupo Transportes que es el numero 9
        if (cartaCreada.dataCarta.groupID != 9)
        {
            CheckGrupoCarta(cartaCreada, jugador);

        }
        else
        {
            CheckTransportes();
        }

    }  
    public void CheckGrupoCarta(Carta carta, int jugador)
    {
        // Miramos cual es el numero del grupo de la carta comprada
        int numeroGrupo = carta.dataCarta.groupID;

        // Conseguimos una lista con todos los ID en el inventario
        List<int> IDCartasEnInventario = new();

        foreach (GameObject obj in turnos.Jugadores[jugador].InventarioJugador)
        {
            Carta cartaObj = obj.GetComponent<Carta>();
            IDCartasEnInventario.Add(cartaObj.ID);
        }
        // Tenemos que preguntarnos si el IDCartasEnInventario contiene TODOS los ID de la posicion concreta de la lista
        for (int i = 0; i < tablero.infoGrupos[numeroGrupo].groupInfo.Count; i++)
        {
            // Si NO contiene algun identificador de ese grupo, implicaria que no tiene todos asique pasariamos 
            // la booleana de grupo completo a False
            if (!IDCartasEnInventario.Contains(tablero.infoGrupos[numeroGrupo].groupInfo[i].IDentificador))
            {
                foreach (BuildingData data in tablero.infoGrupos[numeroGrupo].groupInfo)
                {
                    data.grupoCompleto = false;
                    data.precioAlquiler = data.alquilerBase;
                }
                return;
            }
        }

        // A priori si llega hasta aqui es que la lista contiene todos los ID del grupo
        // Actualizamos que el grupo está completo
        foreach (BuildingData data in tablero.infoGrupos[numeroGrupo].groupInfo)
        {
            data.grupoCompleto = true;

        }
        // Accedemos a cada carta de la mejor manera posible tristemente, para cambiar los colores de los textos
        foreach (GameObject objCarta in turnos.Jugadores[jugador].InventarioJugador)
        {
            Carta cartaInventario = objCarta.GetComponent<Carta>();
            if (cartaInventario.dataCarta.groupID == numeroGrupo)
            {
                cartaInventario.alquilerBaseConColorName.color = cartaInventario.colorNivelActual;
                cartaInventario.alquilerBaseConColorTxt.color = cartaInventario.colorNivelActual;
                cartaInventario.arrayAlquilerNombres[0].color = cartaInventario.colorBlanco;
                cartaInventario.arrayAlquilerTxt[0].color = cartaInventario.colorBlanco;
                cartaInventario.dataCarta.precioAlquiler = cartaInventario.dataCarta.alquilerColorCompleto;
            }

        }
    }

    public void CheckTransportes()
    {
        // Conseguimos una lista con todos los ID en el inventario
        List<int> IDCartasEnInventario = new();

        int numeroEstaciones = 0;
        foreach (GameObject obj in turnos.Jugadores[turnos.ContadorTurnos()].InventarioJugador)
        {
            Carta cartaObj = obj.GetComponent<Carta>();
            IDCartasEnInventario.Add(cartaObj.ID);
        }

        //Comparamos cuantos transportes tenemos y los sumamos. Ahora mismo el grupo 9 es transportes, MODIFICAR si se cambia
        List<BuildingData> listaEstaciones = new();
        for (int i = 0; i < tablero.infoGrupos[9].groupInfo.Count; i++)
        {
            if (IDCartasEnInventario.Contains(tablero.infoGrupos[9].groupInfo[i].IDentificador))
            {
                listaEstaciones.Add(tablero.infoGrupos[9].groupInfo[i]);
                numeroEstaciones++;
            }
        }
        if (numeroEstaciones == 0) return;

        switch (numeroEstaciones)
        {
            case 1:
                foreach (BuildingData data in listaEstaciones)
                {
                    data.alquilerBase = 25;
                }
                break;
            case 2:
                foreach (BuildingData data in listaEstaciones)
                {
                    data.alquilerBase = 50;
                }
                break;
            case 3:
                foreach (BuildingData data in listaEstaciones)
                {
                    data.alquilerBase = 100;
                }
                break;
        }


    }

}
