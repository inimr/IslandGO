using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class Casilla : MonoBehaviour
{
    [SerializeField] BuildingData data;

  
    public int ID { get; private set; } 
    private int groupID;

    [SerializeField] int posTablero; //ESTO HAY QUE RELLENARLO A MANO
    public string nameCasilla { get; private set; }
    public int precioCasilla {  get; private set; }
    private int propietario = -1; 
    public Color colorCasa { get; private set; }

    public bool hasTarot { get; private set; }

    public bool hasPandillero { get; private set; }

    public bool hasTrampa { get; private set; }
    public bool esBloqueable { get; private set; }

    [SerializeField] bool esAtajable;
    [ShowIf("esAtajable")]
    [SerializeField] List<Transform> listaTransformsAtajo = new();

    public int posFinalAtajoTablero { get; private set; }
    private bool esComprable;
    public bool EsComprable => esComprable; //Esto hace que no pueda editarse solo leerse publicamente, es muy similar a los get private set, solo que para valores que no se van a modificar
    public bool esComprado {  get; private set; }
    public bool esHipotecado { get; private set; }
    public bool esGrupoCompleto { get; private set; }

    public bool esPasivaKarma { get; private set; }

    public int ingresoPasivo { get; private set; }

    [Header("Economia de la casilla")]
    private int precioAlquiler; //La variable que almacenara el precio actual que costaria caer.
    public int nivelAlquiler {  get; private set; }
    public int precioMejora {  get; private set; }  
    public int[] arrayPreciosAlquiler { get; private set; }

    public List<EventosCasillas> listaEventos = new();

    [Space(10)]
    [Header("Recursos 3D prefabs de los niveles")]
    [Space(10)]   
    [SerializeField] GameObject[] arrayPrefabs;

    private void Start()
    {
        RellenarValores();
    }

    private void RellenarValores()
    {
        ID = data.IDentificador;
        esComprable = data.comprable;
        groupID = data.groupID;      
        nameCasilla = data.nombreEdificio;
        gameObject.name = nameCasilla;
        esComprable = data.comprable;
        colorCasa = data.colorCasa;        
        precioMejora = data.precioMejora;
        precioCasilla = data.precioCasilla;
        esBloqueable = data.bloqueable;
        arrayPreciosAlquiler = new int[data.arrayPreciosAlquiler.Length];
        esPasivaKarma = data.esPasivaKarma;
        ingresoPasivo = data.generacionPasiva;
        for(int i = 0; i < data.arrayPreciosAlquiler.Length; i++)
        {
            arrayPreciosAlquiler[i] = data.arrayPreciosAlquiler[i];
        }

        if (esAtajable)
        {
            posFinalAtajoTablero = data.posFinalAtajoTablero;
        } 

    }
    public void RellenarPosTablero(int pos) //HAGO ESTO POR VAGANCIA A HACER TODOS UNO POR UNO LA VERDAD
    {
        posTablero = pos;
    }

    public int GetPrecioCasilla()
    {
        int precioCas = precioCasilla;
        int precioFinal = GameManagerMultiplayer.Instance.GetActualPlayer().GetClass() == Classes.Inversor ? (int)(precioCas * ClassManager.PAGO_EXTRA_INVERSOR) : precioCas;
        return precioFinal;
    }
    #region ACCESO A VALORES
    public int GetValorAlquiler() 
    {
        precioAlquiler = arrayPreciosAlquiler[nivelAlquiler];

        int precioFinalAlquiler = GameManagerMultiplayer.Instance.GetPlayer(propietario).GetClass() == Classes.Inversor ? (int)(precioAlquiler * ClassManager.INGRESO_EXTRA_INVERSOR) : precioAlquiler; 

        return precioFinalAlquiler;
    }
  
    public int GetPropietario()
    {
        return propietario;
    }

    public int GetGroupID()
    {
        return groupID; 
    }
    /// <summary>
    /// Modificamos los valores de propietario y esComprado de la casilla, el primero es el ID del Jugador y 
    /// el segundo tiene que ser True si el que lo tiene en su propiedad es un Jugador y no la banca
    /// </summary>
    public void ModificarDatosAlCambiarPropietario(int valor, bool b)
    {
        propietario = valor;
        esComprado = b;

    }
    
    public void ModificarEsGrupoCompleto(bool b)
    {
        esGrupoCompleto = b;        
    }

    public void SetNivelAlquiler(int nuevoNivel)
    {
        nivelAlquiler = nuevoNivel;
    }
    public int GetNivelAlquiler()
    {
        return nivelAlquiler;
    }

    public void ModificarContieneTarot(bool valor)
    {
        hasTarot = valor;
    }
    public void ModificarContieneTrampa(bool valor)
    {
        hasTrampa = valor;
    }
    public void ModificarContienePandillero(bool valor)
    {
        hasPandillero = valor;
    }

    public bool GetEsAtajable()
    {
        return esAtajable;
    }

    public int GetPosTablero()
    {
        return posTablero;
    }
    public List<Transform> GetListaTransforms()
    {
        return listaTransformsAtajo;
    }
    #endregion  
}
