using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Grupos
{
    [HideInInspector]public string name; // Si queremos modificar el nombre de los grupos quitar el Hide y modificarlo por el inspector
                                         // Luego, volver a ponerlo una vez modificado para que no ocupe mas en el array
    public List<BuildingData> groupInfo = new ();
}

public class Ruta : MonoBehaviour
{
    public LanzarDados dadosTirar;
    Transform[] childObjects;
    public List<Transform> childNodeList = new List<Transform>();
    public List<BuildingData> infoCasillas = new List<BuildingData>();
    //Estas variables se llaman desde el script LanzarDados
    public int posicionCasilla;
    public int resultadoSuma;
    public Grupos[] infoGrupos;
    

    private void Start()
    {
        
        FillNodes();
        RecorrerLosScriptableObject();
    }
    
    public void RecorrerLosScriptableObject()
    {
        for (int i = 0; i < infoCasillas.Count; i++)
        {
            infoCasillas[i].IDentificador = i;
            infoCasillas[i].comprado = false;
            infoCasillas[i].grupoCompleto = false;
            if (infoCasillas[i].precioCasilla == 0)
            {
                infoCasillas[i].comprable = false;
                infoCasillas[i].dueno = -1;
            }
            else
            {
                infoCasillas[i].comprable = true;
                infoCasillas[i].dueno = -1;
                infoCasillas[i].precioAlquiler = infoCasillas[i].alquilerBase;
                infoCasillas[i].hipoteca = false;
                infoCasillas[i].nivelCasilla = 0;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        

        //Recorre la lista de casillas para ordenarlas
        for(int i = 0; i<childNodeList.Count; i++)
        {
            Vector3 currentPos = childNodeList[i].position;
            
            

            //Calcula la casilla enterior para dibujar una linea de una casilla a otra
            if(i>0)
            {
                Vector3 prevPos = childNodeList[i - 1].position;
                Gizmos.DrawLine(prevPos, currentPos);
            }
        }
    }

    //Rellena la lista de casillas con las casillas
    void FillNodes()
    {
        childNodeList.Clear();

        childObjects = GetComponentsInChildren<Transform>();

        foreach(Transform child in childObjects)
        {
            if(child != this.transform)
            {
                childNodeList.Add(child);
            }
        }
    }
}
