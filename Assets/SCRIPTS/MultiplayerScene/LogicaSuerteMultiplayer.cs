using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaSuerteMultiplayer : MonoBehaviour
{
    [SerializeField] List<int> listaCartas = new();
    [TextArea(1, 5)][SerializeField] List<string> listaDescripcion = new();
    [SerializeField] GameObject panelSuerte;
    public static LogicaSuerteMultiplayer Instance;

    public event Action<Jugador> OnTarjetaLibreConseguida;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TableManager.Instance.OnCasillaSuerteCaida += TableManager_OnCasillaSuerteCaida;
        Randomizar();
    }

    private void TableManager_OnCasillaSuerteCaida(Jugador player)
    {
        int numeroCartaSacada = listaCartas[0];

        //numeroCartaSacada = 13; //<<<<<<<<<<<<<<<<<<<<<<<PARA SACAR LA CARTA QUE QUEREMOS
        MoverAlFinal(numeroCartaSacada);
        CartaSacada(numeroCartaSacada, player);
    }

    private void Randomizar()
    {
        for (int i = 0; i < listaCartas.Count; i++)
        {
            int temp = listaCartas[i];
            int randomIndex = UnityEngine.Random.Range(i, listaCartas.Count);
            listaCartas[i] = listaCartas[randomIndex];
            listaCartas[randomIndex] = temp;
        }

    }
    private void MoverAlFinal(int numeroSacado)
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

    public void CartaSacada(int numeroSacado, Jugador player)
    {
        StartCoroutine(ActivarImagenSuerte(numeroSacado));
        // Aqui tendremos que ir creando las cartas que queramos
        switch (numeroSacado)
        {
            //Retrocede tres casillas, lo hace mediante TP ahora mismo
            case 0:

                //<<<< FALTA HACER ESTE PARA SABER OPINIONES DE COMO DESPLAZARSE
                        
            break;

            // Gana 100
            case 1:
                GameManagerMultiplayer.Instance.GanarDinero(100, player);
                GameManagerMultiplayer.Instance.CheckDobles();

                break;

            // Paga 100 a la banca
            case 2:
                if(player.dinero.Value < 100)
                {
                    //MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(100, player);
                    TableManager.Instance.SumarFondosBote(100);
                    GameManagerMultiplayer.Instance.CheckDobles();
                }                
            break;

            // Mover a la siguiente casilla sin dueño
            case 3:
                int casillaActual = player.GetPosicionTablero();
                Casilla[] arrayCasillas = TableManager.Instance.GetCasillaArray();
                int casillasAMover = 0;
                for (int i = casillaActual; i < arrayCasillas.Length; i++)
                {
                    if (arrayCasillas[i].EsComprable && !arrayCasillas[i].esComprado)
                    {
                        casillasAMover = i - casillaActual;
                        break;
                    }
                    else if(i == arrayCasillas.Length - 1)
                    {
                        for (int j = 0; i < casillaActual; j++)
                        {
                            if (arrayCasillas[j].EsComprable && !arrayCasillas[j].esComprado)
                            {
                                casillasAMover = arrayCasillas.Length - casillaActual + j;
                                break;
                            }
                        }
                    }
                }
                if(casillasAMover == 0) //ESTO SOLO OCURRIRA SI NO QUEDA NINGUNA CASILLA COMPRADA
                {
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                else
                {
                    StartCoroutine(player.Movement(casillasAMover));
                }
                break;

            // Ir a la carcel
            case 4:
                // El 9 es en estos momentos la carcel solo visita, el 27 es la casilla de IR a la carcel   
                TableManager.Instance.TriggerEventoCarcel();
            break;

            // Paga x a cada jugador 
            case 5:
                foreach(GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
                {

                    if(data.player != player)
                    {
                        if (data.player.enDerrota) continue;
                        if(player.dinero.Value < 50)
                        {
                            //MODO HIPOTECACION, habra que ver que pasa si el jugador no puede pagar a mas de uno la verdad

                        }
                        else
                        {
                            GameManagerMultiplayer.Instance.GanarDinero(50, data.player);
                            GameManagerMultiplayer.Instance.PerderDinero(50, player);
                        }
                    }
                }
                if (player.dinero.Value < 50)
                {
                    //MODO HIPOTECACION, habra que ver que pasa si el jugador no puede pagar a mas de uno la verdad

                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(50, player);
                    TableManager.Instance.SumarFondosBote(50);
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
            break;

            //Gana 10
            case 6:
                GameManagerMultiplayer.Instance.GanarDinero(10, player);
                GameManagerMultiplayer.Instance.CheckDobles();
                break;

            //Pierde 10
            case 7:
                if (player.dinero.Value < 10)
                {
                    //MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(10, player);
                    TableManager.Instance.SumarFondosBote(10);

                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                break;

            //Gana 50
            case 8:
                GameManagerMultiplayer.Instance.GanarDinero(50, player);
                GameManagerMultiplayer.Instance.CheckDobles();
                break;

            //Pierde 50
            case 9:
                if (player.dinero.Value < 50)
                {
                    //MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(50, player);
                    TableManager.Instance.SumarFondosBote(50);
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                break;

            //Gana 200
            case 10:
                GameManagerMultiplayer.Instance.GanarDinero(50, player);
                GameManagerMultiplayer.Instance.CheckDobles(); 
                break;

            //Pierde 200
            case 11:

                if (player.dinero.Value < 200)
                {
                    //MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(200, player);
                    TableManager.Instance.SumarFondosBote(200);
                    GameManagerMultiplayer.Instance.CheckDobles();

                }
                break;

            // Cada jugador te paga x (programado en caso de que no se hayan eliminado jugadores, modificar a posterior)
            case 12:
                foreach (GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
                {

                    if (data.player != player)
                    {
                        if (data.player.enDerrota) continue;
                        if(data.player.dinero.Value < 50)
                        {
                            //MODO HIPOTECACION
                        }
                        else
                        {
                            GameManagerMultiplayer.Instance.PerderDinero(50, data.player);
                            GameManagerMultiplayer.Instance.GanarDinero(50, player);

                        }

                    }
                }   
                
                GameManagerMultiplayer.Instance.GanarDinero(50, player);
                GameManagerMultiplayer.Instance.CheckDobles();
                
                break;

            // Tarjeta "Quedas libre de la carcel"
            case 13:

                OnTarjetaLibreConseguida?.Invoke(player);
                // Aqui se tendrá que generar la imagen o lo que sea de UI de la tarjeta
                break;

            //Ve a la siguiente estación ==> 4,22,32. MODIFICAR SI SE MUEVE SU ID
            case 14:
                int posTablero = player.GetPosicionTablero();
                int numerosMoverse = 0;
                if(posTablero < 4)
                {
                    numerosMoverse = 4 - posTablero;
                }
                else if(posTablero > 4 || posTablero < 22)
                {
                    numerosMoverse = 22 - posTablero;
                }
                else if(posTablero > 22 || posTablero < 32)
                {
                    numerosMoverse = 32 - posTablero;
                }
                else
                {
                    numerosMoverse = TableManager.Instance.GetCasillaArray().Length - posTablero + 4; //<< ESTE CUATRO NO SE SI ES CUATRO O TRES
                }

                if(numerosMoverse > 0)
                {
                    StartCoroutine(player.Movement(numerosMoverse));

                }
                else
                {
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                break;

            // Vete a la salida y cobra 200, MIRAR LA LOGICA DE COBRAR 200
            case 15:

                int numerosAvanzar = TableManager.Instance.GetCasillaArray().Length - player.GetPosicionTablero();
                StartCoroutine(player.Movement(numerosAvanzar));

                break;

            //Inspeccion de edificios, ahora mismo hay 4 niveles
            case 16:
                int totalAPagar = 0;

                foreach(Casilla casilla in GameManagerMultiplayer.Instance.listaPlayers[GameManagerMultiplayer.Instance.turnoPlayerActual.Value].listaPropiedades)
                {
                    if(casilla.nivelAlquiler > 0)
                    {
                        if(casilla.nivelAlquiler == 1) 
                        {
                            totalAPagar += 40;
                        }
                        if(casilla.nivelAlquiler == 2)
                        {
                            totalAPagar += 80;
                        }
                        if(casilla.nivelAlquiler == 3)
                        {
                            totalAPagar += 120;
                        }
                    }
                }

                if(player.dinero.Value < totalAPagar)
                {
                    // MODO HIPOTECACION
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(totalAPagar, player);
                    TableManager.Instance.SumarFondosBote(totalAPagar);
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
             break;
        }

    }

}
