using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName ="EdificiosSO",menuName ="InfoEdificios",order =1)]
public class BuildingData : ScriptableObject
{
    [Header("Información Casilla")]
    [Space(10)]
    public int IDentificador;
    public int groupID;

    public string nombreEdificio;  

    public bool atajable;

    [ShowIf("atajable")]
    public int posFinalAtajoTablero;

    public bool bloqueable;
    public bool comprable;

    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public bool esPasivaKarma;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int generacionPasiva;


    [BoxGroup("Opciones Comprable"),ShowIf("comprable")]
    public int precioCasilla;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public bool comprado;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int dueno=4;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public Color colorCasa;

    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    [Range (0,3)]public int nivelCasilla;

    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public bool hipoteca;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public bool grupoCompleto;
  

    [BoxGroup("Opciones Comprable"), ShowIf("comprable"), Header ("Economia de la casilla")]
    public int precioAlquiler; //La variable que almacenara el precio actual que costaria caer.
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]    
    public int NivelAlquiler;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int alquilerBase;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int alquilerColorCompleto; //podria ser un bool
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int precioMejora;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int alquilerLvl1;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int alquilerLvl2;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int alquilerLvl3;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public int[] arrayPreciosAlquiler;
    
   
    [BoxGroup("Opciones Comprable"), ShowIf("comprable"), Header("Prefabs 3D")]
    public GameObject prefabEdificioBase;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public GameObject prefabEdificioMejora1;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public GameObject prefabEdificioMejora2;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public GameObject prefabEdificioMejora3;
    [BoxGroup("Opciones Comprable"), ShowIf("comprable")]
    public GameObject[] arrayPrefabs;




}

