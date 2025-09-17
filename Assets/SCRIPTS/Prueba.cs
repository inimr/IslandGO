using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class Prueba : MonoBehaviour
{

    public const float ALTURA_MINIMA_CAMARA = 10f;
    public Button botonAvanzar;
    public Button botonRetroceder;

    public Canvas canvasBotones;
    public Camera camara;

    public int posActual;

    public List<GameObject> listaObjetos;



    public void BotonAvanzar()
    {
        Debug.Log("Boton avanzar pulsado");
        posActual++;

        posActual %= listaObjetos.Count;

        LogicaCamara(posActual);
        
    }
   
    public void BotonRetroceder()
    {
        posActual--;
        Debug.Log("Boton retroceder pulsado");

        if (posActual < 0) posActual = listaObjetos.Count-1;


        LogicaCamara(posActual);
    }
    private void LogicaCamara(int pos)
    {
        Vector3 nuevaPosCamara = listaObjetos[pos].transform.position;
        nuevaPosCamara.y += ALTURA_MINIMA_CAMARA;
        camara.transform.position = nuevaPosCamara;

        Vector3 nuevaPosCanvas = listaObjetos[pos].transform.position;
        nuevaPosCanvas.y += 0.5f;
        canvasBotones.transform.position = nuevaPosCanvas;
        canvasBotones.transform.rotation = Quaternion.Euler(0, listaObjetos[pos].transform.eulerAngles.y,0);
        
        int siguientePos = (pos +1) % listaObjetos.Count;
        int anteriorPos = (pos -1 + listaObjetos.Count) % listaObjetos.Count;

        Vector3 dirSiguiente = listaObjetos[pos].transform.InverseTransformDirection(listaObjetos[siguientePos].transform.position - nuevaPosCanvas).normalized;
        float avanzarZ = Mathf.Atan2(dirSiguiente.z, dirSiguiente.x) * Mathf.Rad2Deg;
        botonAvanzar.transform.localRotation = Quaternion.Euler(90, 0, avanzarZ);       

        Vector3 dirAnterior = listaObjetos[pos].transform.InverseTransformDirection(listaObjetos[anteriorPos].transform.position - nuevaPosCanvas).normalized;
        float anteriorZ = Mathf.Atan2(dirAnterior.z, dirAnterior.x) * Mathf.Rad2Deg;
        botonRetroceder.transform.localRotation = Quaternion.Euler(90, 0, anteriorZ);
       
        /*
        Vector3 dirSiguiente = listaObjetos[siguientePos].transform.position - listaObjetos[pos].transform.position;
        float angleSiguiente = Mathf.Atan2(dirSiguiente.z, dirSiguiente.x) * Mathf.Rad2Deg;
        botonAvanzar.transform.localRotation = Quaternion.Euler(90f, 0f, angleSiguiente);

        Vector3 dirAnterior = listaObjetos[anteriorPos].transform.position - listaObjetos[pos].transform.position;
        float angleAnterior = Mathf.Atan2(dirAnterior.z, dirAnterior.x) * Mathf.Rad2Deg; 
        botonRetroceder.transform.localRotation = Quaternion.Euler(90f, 0f, angleAnterior);*/
    }

    /*

    private void Update()
    {
        int siguientePos = (posActual + 1) % listaObjetos.Count;
        int anteriorPos = (posActual - 1 + listaObjetos.Count) % listaObjetos.Count;

        Vector3 dirSiguiente = listaObjetos[siguientePos].transform.position - listaObjetos[posActual].transform.position;
        float angleSiguiente = Mathf.Atan2(dirSiguiente.z, dirSiguiente.x) * Mathf.Rad2Deg;
        botonAvanzar.transform.localRotation = Quaternion.Euler(0f, 0f, angleSiguiente);

        Vector3 dirAnterior = listaObjetos[anteriorPos].transform.position - listaObjetos[posActual].transform.position;
        float angleAnterior = Mathf.Atan2(dirAnterior.z, dirAnterior.x) * Mathf.Rad2Deg;
        botonRetroceder.transform.localRotation = Quaternion.Euler(0f, 0f, angleAnterior);
    }*/
}
