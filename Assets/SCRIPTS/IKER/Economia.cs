using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEditor.Rendering;


public class Economia : MonoBehaviour
{
    [SerializeField] private List<Ficha>JugadoresTurnos;
    public int Contador;

    private LogicaSuerte cartaSuerte;

    [SerializeField] private List<TextMeshProUGUI> txtDineros;

    [SerializeField] private Ruta tablero;    

    [SerializeField] private Hipotecacion hipotecacion;
    private TextMeshProUGUI txtPrecioEdif;
    private TextMeshProUGUI txtNomEdif;
    [SerializeField] private GameObject UICompra;
    [SerializeField] private UIController controladorUI;
    [SerializeField] private Turnos turnos;
    [SerializeField] private CreacionCarta creacionCarta;
    public int suma = 200;

    private GameManager gameManager;
    public GameObject inventario;

    public int loteriaCantidad = 0;
    private int impuesto = 200;


    static int SortByGroupID(GameObject c1, GameObject c2)
    {
        Carta carta1 = c1.GetComponent<Carta>();
        Carta carta2 = c2.GetComponent<Carta>();

        return carta1.dataCarta.groupID.CompareTo(carta2.dataCarta.groupID);
    }

    void Start()
    {
        controladorUI = FindFirstObjectByType<UIController>();
        txtNomEdif = GameObject.Find("NombrePropiedad").GetComponent<TextMeshProUGUI>();
        txtPrecioEdif = GameObject.Find("PrecioPropiedad").GetComponent<TextMeshProUGUI>();
        UICompra = GameObject.Find("UICompra");
        turnos = FindFirstObjectByType<Turnos>();
        cartaSuerte = GetComponentInChildren<LogicaSuerte>();
        creacionCarta = FindFirstObjectByType<CreacionCarta>();

        UICompra.SetActive(false);
        gameManager = FindAnyObjectByType<GameManager>();

        
    }

    public int Impuestos()
    {
        return impuesto;
    }

    public void AumentarDineroALoteria(int cantidad)
    {
        loteriaCantidad += cantidad;
    }
    public int DineroTotalLoteria()
    {
        return loteriaCantidad;
    }

    public void ResetearLoteria()
    {
        loteriaCantidad = 0;
    }
    public void GanarDinero(int dineroAumento, int contador)
    {
        JugadoresTurnos[contador].dinero += dineroAumento;
       
        controladorUI.ActualizarTextoDinero(contador); 

    }

    public void PerderDinero(int dineroPerdido, int contador)
    {
        JugadoresTurnos[contador].dinero -= dineroPerdido;
       

        controladorUI.ActualizarTextoDinero(contador); 

    }

    public void ActualizarDinero(int dineroTotalActual, int contador)
    {
        JugadoresTurnos[contador].dinero = dineroTotalActual;

        controladorUI.ActualizarTextoDinero(contador);
    }

    public void CompraCasilla()
    {
        BuildingData infoCasilla = tablero.infoCasillas[turnos.Jugadores[turnos.ContadorTurnos()].posicionCasilla];

        infoCasilla.dueno = turnos.ContadorTurnos();
        infoCasilla.comprado =true;
     
        PerderDinero(infoCasilla.precioCasilla, turnos.ContadorTurnos());
        controladorUI.ActivacionPanelCompra(false);
        creacionCarta.CrearCarta(infoCasilla);
        turnos.Jugadores[turnos.ContadorTurnos()].CambioFase();

    }
    public void NoComprarCasilla() 
    {
        /*Button botonCompra = UICompra.transform.GetChild(3).GetComponent<Button>();
        botonCompra.interactable = true;
        UICompra.SetActive(false);
        JugadoresTurnos[Contador].CambioFase();
        */
        controladorUI.BotonCompraEdificios().interactable = true;
        controladorUI.ActivacionPanelCompra(false);
        turnos.Jugadores[turnos.ContadorTurnos()].CambioFase();

    }
  

  
  
  
}
