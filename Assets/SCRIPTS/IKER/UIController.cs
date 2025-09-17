using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


[System.Serializable]
public class PlayerUIData
{
    public GameObject baseUI;
    public GameObject resalto;
    public TextMeshProUGUI nickname;
    public TextMeshProUGUI textoDinero;
    public GameObject panelInventario;
  
}
// Usaremos la clase de arriba para asignar de manera dinamica la UI que haya.
// Teniendo en cuenta el valor recibido de algún otro sitio (en estos momentos Usernames).

// Lo logico sería usar un Horizontal / Vertical Layout group con el prefab del objeto
// en este caso del GameObject base e ir instanciondolo y asignando valores a medida que
// sepamos cuantos jugadores hay.

public class UIController : MonoBehaviour
{
    [SerializeField] private List <PlayerUIData> playerUIList;

    [Header("Paneles")]
    [SerializeField] private GameObject prefabUI; //Prefab que contiene todo la info del player
    [SerializeField] private Transform padreUI; //Padre de todos los prefabUI;

    [SerializeField] private GameObject panelUICompra;
    [SerializeField] private TextMeshProUGUI textoCompraNombreEdificio;
    [SerializeField] private TextMeshProUGUI textoCompraPrecioEdificio;
                                                
    [Space(10)]
    [Header("Scripts")]
    [Space(10)]
    public Turnos turnos;
    [SerializeField] private Usernames nombres;

    [Space(10)] 


    [SerializeField]private GameObject carta; //Prefab de la carta que aparecerá en el inventario de la UI
    [SerializeField] private GameObject panelInventario; // Prefab del panel donde apareceran las cartas
    [SerializeField] private Transform padrePaneles; //Padre de los paneles
    [Space(10)]

    [Header("Cosas de musica")]
    public Slider MusicSLIDER;
    public Slider EffectSLIDER;
    public VolumeSettings VolumeSettings;


    private void Awake()
    {
        nombres = FindAnyObjectByType<Usernames>();
    }

    // Start is called before the first frame update
    void Start()
    {
        turnos = FindAnyObjectByType<Turnos>();
      
        InstanciarUIJugadores();
        MetodoTemporalActualizarDinero();
        /*
        VolumeSettings.Musica_SLIDER = MusicSLIDER;
        VolumeSettings.Efectos_SLIDER = EffectSLIDER;
        */
        
    } 

    
    public void MetodoTemporalActualizarDinero()
    {
        for(int i = 0; i < turnos.Jugadores.Count; i++)
        {
            ActualizarTextoDinero(i);
        }
    }

    public void ActualizarTextoDinero(int jugador)
    {
        playerUIList[jugador].textoDinero.text = turnos.Jugadores[jugador].dinero.ToString();
    }
    public void InstanciarUIJugadores()
    {
        // Aqui tendremos que checkear cuantos jugadores vienen y crear a partir de eso        
        // RECORDAR CAMBIAR LOS NUMEROS SI CAMBIAMOS EL PREFAB   


        if(nombres != null)
        {
            for (int i = 0; i < nombres.userdataList.Count; i++)
            {
                print("Hello");
            }
        }
       

        //BACKUP QUE FUNCIONA POR SI SOLO SIN EL SCRIPT USERNAMES
        for (int i = 0; i < turnos.Jugadores.Count; i++)
        {
            PlayerUIData newData = new();
            playerUIList.Add(newData);
            GameObject objUI = Instantiate(prefabUI, padreUI);
            objUI.name = $"P{i + 1}UIData";

            playerUIList[i].resalto = objUI.transform.GetChild(0).gameObject;
            playerUIList[i].resalto.SetActive(false);

            playerUIList[i].baseUI = objUI.transform.GetChild(1).gameObject;

            playerUIList[i].textoDinero = objUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            playerUIList[i].nickname = objUI.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            playerUIList[i].nickname.text = nombres != null ? nombres.userdataList[i].nickName : "P" + (i + 1);

            playerUIList[i].panelInventario = Instantiate(panelInventario, padrePaneles);
            playerUIList[i].panelInventario.name = nombres != null ? $"Panel {nombres.userdataList[i].nickName}" : $"Panel {i+1}";
            playerUIList[i].panelInventario.transform.SetSiblingIndex(i);

            //Si modificamos el prefab de PanelInventario habra que modificar esto tambien
            Button botonFinalizarTurno = playerUIList[i].panelInventario.transform.GetChild(0).GetComponent<Button>();
            botonFinalizarTurno.onClick.AddListener(turnos.Jugadores[i].Finalizar);

            GameObject inventarioJugador = playerUIList[i].panelInventario.transform.GetChild(1).GetChild(1).GetChild(0).gameObject;
            GameObject inventarioTarot = playerUIList[i].panelInventario.transform.GetChild(3).GetChild(1).GetChild(0).gameObject;
            Button botonCambiarInventario = playerUIList[i].panelInventario.transform.GetChild(2).GetComponent<Button>();
            botonCambiarInventario.onClick.AddListener(turnos.Jugadores[i].CambiarInventario);
            

            turnos.Jugadores[i].AsignarInventarios(inventarioJugador, inventarioTarot);            
            playerUIList[i].panelInventario.SetActive(false);
        }

        ResaltoUIData(0, true); //Para que el primero esté resaltado, en el Start no le da tiempo
        PanelInventarioJugador(0, true);
    }

    public void ResaltoUIData(int numJugador, bool estado)
    {
        playerUIList[numJugador].resalto.SetActive(estado);
    }

    public void PanelInventarioJugador(int numJugador, bool estado)
    {
        playerUIList[numJugador].panelInventario.SetActive(estado);
    }
    public GameObject CartaInventario()
    {
        return carta;
    }
    public void RellenarPanelCompraEdificio(BuildingData casilla)
    {
        textoCompraNombreEdificio.text = casilla.nombreEdificio;
        textoCompraPrecioEdificio.text = casilla.precioCasilla.ToString();
    }

    public void ActivacionPanelCompra(bool estado)
    {
        panelUICompra.SetActive(estado);
    }

    public Button BotonCompraEdificios()
    {
        return panelUICompra.transform.GetChild(3).GetComponent<Button>();
    }
   
    
}
