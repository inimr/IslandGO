using System;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class CaminoTablero
{
    [SerializeField] string name;
    [SerializeField] List<Transform> posicionesCamino;

    public List<Transform> GetPosicionesCamino()
    {
        return posicionesCamino;
    }
}

[Serializable]
public class GruposEdificios
{
    [SerializeField] string name; // Si queremos modificar el nombre de los grupos quitar el Hide y modificarlo por el inspector
                                  // Luego, volver a ponerlo una vez modificado para que no ocupe mas en el array
    public List<Casilla> groupInfo = new();
}
public class TableManager : MonoBehaviour
{
    [SerializeField] CaminoTablero[] caminoEntreCasillas;
    [SerializeField] Casilla[] listaCasillas;
    [SerializeField] GruposEdificios[] infoGruposEdificios;

    public static TableManager Instance;

    public event Action<Jugador> OnCasillaSuerteCaida;
    public event Action OnCasillaTarotCaida;
    public event Action OnALaCarcel;

    private int boteLoteria;
    private const int GRUPO_ID_ESTACIONES = 8;
    private const int PRECIO_IMPUESTOS = 200;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        for(int i = 0; i < listaCasillas.Length; i++)
        {
            listaCasillas[i].RellenarPosTablero(i);
        }
    }
    public Casilla[] GetCasillaArray()
    {
        return listaCasillas;
    }

    public CaminoTablero[] GetCaminoTablero()
    {
        return caminoEntreCasillas;
    }

    public void ComprobarCasilla(int posTablero)
    {    
        Casilla casilla = listaCasillas[posTablero];
        Jugador player = GameManagerMultiplayer.Instance.GetActualPlayer();


        //<<<<<<<<<<<<<<<<<<<<<<<<13/07>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        //AQUI PODEMOS METER LA LOGICA DEL PANDILLERO, O HACERLO DE DISTINTAS FORMAS
        //DEPENDIENDO DE QUE SE QUIERA HACER
        if (casilla.hasPandillero) 
        {
            if(player.GetClass() != Classes.Pandillero)
            {

            }
        }
         //<<<<<<<<  ATAJO A MODIFICAR CON EL PANEL ETC >>>>>>>>>>>
         /*
        if (casilla.GetEsAtajable())
        {
            if (!GameManagerMultiplayer.Instance.GetActualPlayer().GetHaAtajado())
            {
                int numeroAleatorio = UnityEngine.Random.Range(0, 100);
                int porcentajeFinal = player.GetClass() == Classes.Trotamundos ? ClassManager.PORCENTAJE_ATAJABLE_TROTAMUNDOS : ClassManager.PORCENTAJE_ATAJABLE;
                if (numeroAleatorio < porcentajeFinal)
                {
                    //ACTIVAREMOS EL ATAJO
                    StartCoroutine(player.MovementAtajo(casilla.GetListaTransforms(), casilla.posFinalAtajoTablero));
                    return;
                }
            }
            else
            {
                GameManagerMultiplayer.Instance.GetActualPlayer().CambiarValorHaAtajado();
            }

        }*/


        int dineroJugadorActual = player.dinero.Value;

        if (casilla.EsComprable)
        {
            if (casilla.esComprado)
            {
                int IDPropietario = casilla.GetPropietario();
                int costeCasilla = casilla.GetValorAlquiler();
                int costeCasillaFinal = player.GetClass() == Classes.Escapista ? (int) (costeCasilla * ClassManager.DESCUENTO_ESCAPISTA) : costeCasilla;
                if (costeCasillaFinal < dineroJugadorActual)
                {
                    GameManagerMultiplayer.Instance.PerderDinero(costeCasillaFinal, player);
                    GameManagerMultiplayer.Instance.GanarDinero(costeCasillaFinal, GameManagerMultiplayer.Instance.GetPlayer(IDPropietario));
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                else
                {
                    //MODO HIPOTECACION AQUI
                    Debug.Log("No tiene dinero suficiente para pagar, entramos en el modo Hipoteacion");
                }
            }           
            else if(dineroJugadorActual > casilla.precioCasilla)
            {                
                GameManagerMultiplayer.Instance.ActivarEventoCompraCasilla();
            }
            else
            {
                GameManagerMultiplayer.Instance.CheckDobles();
            }
        }
        // SI LA CARTA NO ES UNA CARTA COMPRABLE, HABRA QUE MIRAR QUE CARTA ES Y HAREMOS ESE EFECTO DEPENDIENDO DE ELLO
        else
        {
           LogicaCasillasNoComprables(casilla);
        }       
    }

  
  
    private void LogicaCasillasNoComprables(Casilla casilla)
    {
        int ID = casilla.ID; //ESTO NOS SERVIRA PARA LAS CASILLAS NO COMPRABLES FIJAS, COMO LA LOTERIA, CON EL CODIGO DE ARRIBA
        Jugador playerActual = GameManagerMultiplayer.Instance.GetActualPlayer();
        switch (ID)
        {
           
            case 9: //CARCEL SOLO VISITA
                GameManagerMultiplayer.Instance.CheckDobles();
                break;

            case 12: //CASILLA SUERTE
                OnCasillaSuerteCaida?.Invoke(playerActual);
                break;
            case 18: //CASILLA LOTERIA
                //Dar el dinero almacenado en la Loteria
                GameManagerMultiplayer.Instance.GanarDinero(boteLoteria, playerActual);
                boteLoteria = 0;
                GameManagerMultiplayer.Instance.CheckDobles();
                break;
            case 27: // IR A LA CARCEL DIRECTAMENTE
                //Toda la logica de ir a la carcel ira aqui
                OnALaCarcel?.Invoke();
                break;
            case 34: //IMPUESTOS
                if (playerActual.dinero.Value < PRECIO_IMPUESTOS)
                {
                    //Entraremos en el modo hipotecacion
                }
                else
                {
                    GameManagerMultiplayer.Instance.PerderDinero(PRECIO_IMPUESTOS, playerActual);
                    boteLoteria += PRECIO_IMPUESTOS;
                    GameManagerMultiplayer.Instance.CheckDobles();
                }
                break;
            case 36: //CASILLA TAROT
                OnCasillaTarotCaida?.Invoke();
                break;
            default: //PARA LAS CASILLAS NO ESPECIFICAS
                CasillasEventManager.Instance.EscogerEventoCasilla(casilla.GetPosTablero());
                break;

        }       
       
    }

    public void ComprobarGrupoCompleto(Casilla casilla)
    {
        int IDGrupo = casilla.GetGroupID();


        if(IDGrupo == GRUPO_ID_ESTACIONES)
        {
            ComprobarEstaciones(casilla);
        }
        else
        {
            ComprobarPropiedades(casilla, IDGrupo);
        }
             
    }

    private void ComprobarEstaciones(Casilla casilla)
    {
        List<Casilla> grupoCasilla = infoGruposEdificios[GRUPO_ID_ESTACIONES].groupInfo;

        int numEstaciones = 0;
        foreach(Casilla c in grupoCasilla)
        {
            if(c.GetPropietario() == casilla.GetPropietario())
            {
                numEstaciones++;
            }
        }

        foreach(Casilla c in grupoCasilla)
        {
            if (c.GetPropietario() == casilla.GetPropietario())
            {
                c.SetNivelAlquiler(numEstaciones - 1);
            }
        }

    }

    private void ComprobarPropiedades(Casilla casilla, int IDGrupo)
    {
        List<Casilla> grupoCasilla = infoGruposEdificios[IDGrupo].groupInfo;

        foreach (Casilla c in grupoCasilla)
        {
            if (c.GetPropietario() != casilla.GetPropietario())
            {
                foreach(Casilla cas in grupoCasilla)
                {
                    cas.ModificarEsGrupoCompleto(false);
                }
                return;

            }
        }

        foreach (Casilla c in grupoCasilla)
        {
            c.ModificarEsGrupoCompleto(true);
        }
    }
    public void TriggerEventoCarcel()
    {
        OnALaCarcel?.Invoke();
    }
    public void SumarFondosBote(int cantidad)
    {
        boteLoteria += cantidad;
    }
}
