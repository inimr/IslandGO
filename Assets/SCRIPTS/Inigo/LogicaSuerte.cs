using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaSuerte : MonoBehaviour
{
    //Por ahora lo haremos con ints pero se puede hacer con GameObjects para cada carta etc.
    public Ruta tablero;
    public List<int> listaCartas = new ();
   [TextArea (1,5)] public List<string> listaDescripcion = new ();
    public Economia economia;
    public Turnos turnos;
    public GameObject panelSuerte;

    void Start()
    {
        economia = GetComponentInParent<Economia>();
        Randomizar();
    }
    private void Randomizar()
    {
        for (int i = 0; i < listaCartas.Count; i++)
        {
            int temp = listaCartas[i];
            int randomIndex = Random.Range(i, listaCartas.Count);
            listaCartas[i] = listaCartas[randomIndex];
            listaCartas[randomIndex] = temp;
        }

     
    }

    public void CasillaSuerte(Ficha numeroJugador)
    {
        int numeroCartaSacada = listaCartas[0];
        
        //numeroCartaSacada = 13; //<<<<<<<<<<<<<<<<<<<<<<<PARA SACAR LA CARTA QUE QUEREMOS
        MoverAlFinal(numeroCartaSacada);

        CartaSacada(numeroCartaSacada, numeroJugador);
    }
    public void MoverAlFinal(int numeroSacado)
    {
        listaCartas.RemoveAt(0);
        listaCartas.Add(numeroSacado);
    }

    IEnumerator ActivarImagenSuerte(int numeroSacado)
    {
        TextMeshProUGUI textoPanel = panelSuerte.GetComponentInChildren<TextMeshProUGUI>();
        textoPanel.text = listaDescripcion[numeroSacado];
        panelSuerte.SetActive(true);
        

        yield return new WaitForSeconds(3);
        panelSuerte.SetActive(false);
    }
    // LOGICA DE LAS CARTAS DE SUERTE
    public void CartaSacada(int numeroSacado, Ficha numeroJugador)
    {
        StartCoroutine(ActivarImagenSuerte(numeroSacado));
        // Aqui tendremos que ir creando las cartas que queramos
        switch (numeroSacado)
        {
            //Retrocede tres casillas, lo hace mediante TP ahora mismo
            case 0:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                numeroJugador.posicionCasilla-= 3;
                numeroJugador.routePosition -= 3;
                numeroJugador.transform.position = tablero.childNodeList[numeroJugador.routePosition].transform.position;
                numeroJugador.posActual = numeroJugador.transform.position;
                numeroJugador.ComprobarCasilla(tablero.infoCasillas[numeroJugador.routePosition]);
            break;

            // Gana 100
            case 1:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.GanarDinero(100, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            // Paga 100 a la banca
            case 2:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.PerderDinero(100, numeroJugador.ID);
                economia.loteriaCantidad += 100;
                numeroJugador.CambioFase();
                break;

            // Mover a la siguiente casilla sin dueño
            case 3:
                Debug.LogWarning("Has sacado el " + numeroSacado);

                // Puede que falten lineas para el correcto funcionamiento porque no se todo lo que hace falta realmente
                for (int i = numeroJugador.posicionCasilla; i < tablero.infoCasillas.Count; i++)
                {
                    if (tablero.infoCasillas[i].comprable && !tablero.infoCasillas[i].comprado)
                    {
                        MoverseALugar(i, numeroJugador);                       
                        break;
                    }
                    else if(i == tablero.infoCasillas.Count - 1)
                    {
                        for (int j = 0; i < numeroJugador.posicionCasilla; j++)
                        {
                            if (tablero.infoCasillas[j].comprable && !tablero.infoCasillas[j].comprado)
                            {
                                MoverseALugar(j, numeroJugador);
                                
                                break;
                            }
                        }
                    }
                }
            break;

            // Ir a la carcel
            case 4:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                // El 9 es en estos momentos la carcel solo visita, el 27 es la casilla de IR a la carcel   
                
                numeroJugador.transform.position = tablero.childNodeList[9].transform.position;
                numeroJugador.posActual = numeroJugador.transform.position;
                numeroJugador.posicionCasilla = 9;
                numeroJugador.routePosition = 9;
                numeroJugador.enCarcel = true;
                turnos.PasarTurnoAlSiguiente();
                //numeroJugador.CambioFase();
                break;

            // Paga x a cada jugador (programado en caso de que no se hayan eliminado jugadores, MODIFICAR a posterior)
            case 5:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                int numeroAEntregar = 1;
                foreach (Ficha fichaScript in turnos.ListaJugadores()) 
                {
                    if (fichaScript.enDerrota) continue;
                    if (fichaScript.Turno == false)
                    {
                        economia.GanarDinero(50, fichaScript.ID);
                        numeroAEntregar++;
                    }
                }
                economia.PerderDinero(50 * numeroAEntregar, numeroJugador.ID);
                economia.loteriaCantidad += 50;
                numeroJugador.CambioFase();
                break;
          
            //Gana 10
            case 6:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.GanarDinero(10, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            //Pierde 10
            case 7:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.PerderDinero(10, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            //Gana 50
            case 8:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.GanarDinero(50, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            //Pierde 50
            case 9:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.PerderDinero(50, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            //Gana 200
            case 10:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.GanarDinero(200, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            //Pierde 200
            case 11:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                economia.PerderDinero(200, numeroJugador.ID);
                numeroJugador.CambioFase();
                break;

            // Cada jugador te paga x (programado en caso de que no se hayan eliminado jugadores, modificar a posterior)
            case 12:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                int jugadores = 1;
                foreach (Ficha fichaScript in turnos.ListaJugadores()) 
                {
                    if (fichaScript.enDerrota) continue;
                    if (fichaScript.Turno == false) 
                    { 
                        economia.PerderDinero(50, fichaScript.ID);
                        jugadores++;
                    }
                }
                numeroJugador.CambioFase();
                economia.GanarDinero(50 * jugadores, numeroJugador.ID);
                break;

            // Tarjeta "Quedas libre de la carcel"
            case 13:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                numeroJugador.tarjetaLibreCarcel = true;
                numeroJugador.CambioFase();
                // Aqui se tendrá que generar la imagen o lo que sea de UI de la tarjeta
             break;

            //Ve a la siguiente estación ==> 4,22,32
            case 14:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                if (numeroJugador.posicionCasilla < 4 || numeroJugador.posicionCasilla > 32)
                {
                    //Esta ira a la estacion autobuses (num 4)
                    MoverseALugar(4, numeroJugador);
                }
                else if(numeroJugador.posicionCasilla > 4 || numeroJugador.posicionCasilla < 22)
                {
                    //Esta ira a la estacion de tren
                    MoverseALugar(22, numeroJugador);
                }
                else
                {
                    //Estacion de barcos
                    MoverseALugar(32, numeroJugador);
                }
            break;

             // Vete a la salida y cobra 200, MIRAR LA LOGICA DE COBRAR 200
            case 15:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                MoverseALugar(0, numeroJugador);
                economia.GanarDinero(200, numeroJugador.ID);
            break;

            //Inspeccion de edificios, ahora mismo hay 4 niveles
            case 16:
                Debug.LogWarning("Has sacado el " + numeroSacado);
                foreach (GameObject carta in numeroJugador.InventarioJugador)
                {
                    Carta infoCarta = carta.GetComponent<Carta>();
                    if(infoCarta.dataCarta.nivelCasilla > 0)
                    {
                        if(infoCarta.dataCarta.nivelCasilla == 1)
                        { 
                            economia.PerderDinero(40, numeroJugador.ID);
                            economia.loteriaCantidad += 40;                        
                        }
                        else if(infoCarta.dataCarta.nivelCasilla == 2)
                        { 
                            economia.PerderDinero(80, numeroJugador.ID);
                            economia.loteriaCantidad += 80;
                        }
                        else 
                        {
                            economia.PerderDinero(120, numeroJugador.ID);
                            economia.loteriaCantidad += 120;  
                            
                        }
                    }
                   
                }
                numeroJugador.CambioFase();
            break;
        }
       
    }


    void MoverseALugar(int numeroLoop, Ficha numeroJugador)
    {
        numeroJugador.transform.position = tablero.childNodeList[numeroLoop].transform.position;
        numeroJugador.posActual = numeroJugador.transform.position;
        numeroJugador.posicionCasilla = numeroLoop;
        numeroJugador.routePosition = numeroLoop;
        numeroJugador.ComprobarCasilla(tablero.infoCasillas[numeroJugador.posicionCasilla]);
    }

}
