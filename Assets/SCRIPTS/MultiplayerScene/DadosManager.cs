using System;
using UnityEngine;

public class DadosManager : MonoBehaviour
{
    public static DadosManager Instance;
    private int resultadoDado1;
    private int resultadoDado2;

   
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


    public void LanzarDosDados()
    {
        // HACER LA LOGICA DE LANZAR LOS DOS DADOS

        //Son 7 porque los Random.Range con ints el valor maximo es exclusive y no inclusive
        resultadoDado1 = UnityEngine.Random.Range(0, 7);
        resultadoDado2 = UnityEngine.Random.Range(0, 7);
        
        
        GameManagerMultiplayer.Instance.GetActualPlayer().Moverse(resultadoDado1, resultadoDado2);
        
    }
}
