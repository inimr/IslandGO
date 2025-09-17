using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Caminos
{
    public string name;
    public List<Transform> posicionesCamino;
}


// Las posiciones de los caminos dependeran del numero de la casilla que se llega.
// Ej: posicionesCamino[1] ser�n los transform que har�n falta para pasar del 0 al 1
public class CaminosCasillas : MonoBehaviour
{

    public Caminos[] caminoCasilla;
    
}
