using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject prefabUIDineroPlayer;
    [SerializeField] Transform padreUIDinero;
    public static UIManager Instance;

    [SerializeField] List<UIPlayerData> listaUI = new();

    [Header("Variables del inventario")]
    [SerializeField] GameObject panelInventarioUI;   
    [SerializeField] Transform inventarioCasillas; 
    [SerializeField] Transform inventarioTarot;

    [SerializeField] GameObject panelConfirmacionCompra;

    [Header("Variables panel ganancias final turno")]
    [SerializeField] GameObject panelInfoGeneracionPasiva;
    [SerializeField] TextMeshProUGUI textoGeneracionDinero;
    [SerializeField] TextMeshProUGUI textoGeneracionKarma;

    [Header("Variables selector clase")]
    [SerializeField] GameObject panelSeleccionClase;
    [SerializeField] BotonSelectorClase botonClaseOpcionUno;
    [SerializeField] BotonSelectorClase botonClaseOpcionDos;

    [Header("Variables Modo Trampero")]
    [SerializeField] GameObject panelUIModoTrampero;  
 

    [Header("Variables eleccion Impostor")]
    [SerializeField] GameObject panelClaseImpostor;
    [SerializeField] BotonSelectorClase[] arrayBotonesImpostor;
    private Casilla casillaAComprar;
    private Jugador ownerPlayer;

    public event Action OnBotonSiClickado;
    public event Action OnBotonNoClickado;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    private void Start()
    {
        GameManagerMultiplayer.Instance.OnCasillaComprable += GameManagerMultiplayer_OnCasillaComprable;
        GameManagerMultiplayer.Instance.OnActualizarDinero += GameManagerMultiplayer_OnActualizarDinero;
        GameManagerMultiplayer.Instance.OnTurnoVueltaDada += GameManagerMultiplayer_OnTurnoVueltaDada;

        ClassManager.Instance.OnClasesAEscoger += ClassManager_OnClasesAEscoger;
        ClassManager.Instance.OnClaseEscogida += ClassManager_OnClaseEscogida;
        ClassManager.Instance.OnClasesParaImpostor += ClassManager_OnClasesParaImpostor;
        ClassManager.Instance.OnModoTramperoActivado += ClassManager_OnModoTramperoActivado;
    }

    private void ClassManager_OnModoTramperoActivado(int pos)
    {       
        ModificarUITrampero(true);
    }

    /// <summary>
    /// Este metodo se hace de manera local en el jugador que tiene que modificar su clase debido a que es Impostor
    /// </summary>
    /// <param name="obj">El array con las clases que se pueden escoger en este turno</param>
    private void ClassManager_OnClasesParaImpostor(int[] obj)
    {
        for(int i = 0; i < obj.Length; i++)
        {
            arrayBotonesImpostor[i].ModificarValoresNumeroClase(obj[i]);
            arrayBotonesImpostor[i].gameObject.SetActive(true);
        }

        panelClaseImpostor.SetActive(true);
    }

    private void ClassManager_OnClaseEscogida(int obj)
    {
        Sprite imagenEscogida = ClassManager.Instance.GetClass(obj).iconoPlayer;

        // ACTUALIZAR EL ICONO DEL JUGADOR QUE HA ESCOGIDO LA CLASE, CUANDO TENGAMOS LA UI PREPARADA :D
    }

    private void ClassManager_OnClasesAEscoger(int arg1, int arg2)
    {
        botonClaseOpcionUno.ModificarValoresNumeroClase(arg1);
        botonClaseOpcionDos.ModificarValoresNumeroClase(arg2);
        
        panelSeleccionClase.SetActive(true);   

    }

    private void GameManagerMultiplayer_OnTurnoVueltaDada(int dineroGenerado, int karmaGenerado)
    {
        textoGeneracionDinero.text = dineroGenerado.ToString();
        textoGeneracionKarma.text = karmaGenerado.ToString();
        panelInfoGeneracionPasiva.SetActive(true);
        Invoke("DesactivarPanelInfoGeneracion", 5);
    }
    private void DesactivarPanelInfoGeneracion()
    {
        panelInfoGeneracionPasiva.SetActive(false);
    }

    public void DesactivarBotonesImpostor()
    {
        for(int i = 0; i < arrayBotonesImpostor.Length; i++)
        {
            arrayBotonesImpostor[i].gameObject.SetActive(false);
        }
    }
    private void GameManagerMultiplayer_OnActualizarDinero(int dineroActual, int playerID)
    {
        listaUI[playerID].ModificarTexto(dineroActual);
    }

    private void GameManagerMultiplayer_OnCasillaComprable(Casilla obj)
    {
        casillaAComprar = obj;
        RellenarPanelConfirmacion();
        panelConfirmacionCompra.SetActive(true);
    }

    //SI MODIFICAMOS EL PREFAB DE PANEL DE CONFIRMACION, HABRA QUE MODIFICAR ESTO TAMBIEN
    private void RellenarPanelConfirmacion()
    {
        TextMeshProUGUI textoNombre = panelConfirmacionCompra.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI textoPrecio = panelConfirmacionCompra.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        textoNombre.text = casillaAComprar.nameCasilla.ToString();
        textoPrecio.text = casillaAComprar.GetPrecioCasilla().ToString();
    }

    // METODOS DE LOS BOTONES DEL PANEL DE CONFIRMACION DE COMPRAR LA CASILLA DONDE SE HA CAIDO
    public void ConfirmacionCompraCasilla()
    {
        OnBotonSiClickado?.Invoke();

        panelConfirmacionCompra.SetActive(false);
    }
    public void RechazarCompraCasilla()
    {
        OnBotonNoClickado?.Invoke();
        panelConfirmacionCompra.SetActive(false);
    }

    // -------------------------------------------------------------------------------------------//

    public void ActivarPanelUIJugador(int ID)
    {
        UIPlayerData data = listaUI[ID];
        data.name = "Player " + ID;
        data.ModificarTexto(data.name);
        data.gameObject.SetActive(true);
        //data.CambiarEstadoResalto();
        
    }

    public void ActivarInventarioJugador()
    {
        //ESTO NO ESTA BIEN HECHO, ES FILLER
        panelInventarioUI.SetActive(true);
       
    }    

    public void SetOwnerPlayer(Jugador player)
    {
        ownerPlayer = player; 
    }
    public void MetodoBotonCambioTurno()
    {
        if (GameManagerMultiplayer.Instance.turnoPlayerActual.Value != ownerPlayer.GetPlayerID()) return;
        if (ownerPlayer.GetFaseActual() != Jugador.FaseJuego.Planificacion) return;
        GameManagerMultiplayer.Instance.ComienzoCambioTurno();
    }
    
    /// <summary>
    /// Método que va a llamar el Boton del Trampero, para conseguir la info necesaria y pasar a ese modo
    /// </summary>
    public void EntrarModoTrampero()
    {
        if (ownerPlayer.GetClass() != Classes.Trampero) return;
        //HABRA QUE CHEQUEAR EN ALGUN LADO EL CD PARA QUE NO PUEDA HACERLO TODOS LOS TURNOS, QUIZAS AL INICIAR EL TURNO?
        //Desactivar el interactuable

        ClassManager.Instance.LogicaServerTrampero();
    }

    /// <summary>
    /// Método para activar y desactivar todas las UI para entrar y salir del modo trampero. True es cuando se vaya a entrar
    /// al modo Trampero y false al salir
    /// </summary>
    public void ModificarUITrampero(bool nuevoEstado)
    {

        padreUIDinero.gameObject.SetActive(!nuevoEstado);
        panelInventarioUI.SetActive(!nuevoEstado);
        //  22/07 >>>>>>>> DESACTIVAR LAS UI LATERALES CUANDO SE HAGAN
        panelUIModoTrampero.SetActive(nuevoEstado);
       
    } 
 
    public Transform GetInventarioCasillas()
    {
        return inventarioCasillas;
    }

    public Transform GetInventarioTarot()
    {
        return inventarioTarot;
    }

    public Casilla GetCasillaComprar()
    {
        return casillaAComprar;
    }
    
}
