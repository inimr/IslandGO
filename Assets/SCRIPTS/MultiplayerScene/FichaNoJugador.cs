using Unity.Netcode;
using UnityEngine;

public class FichaNoJugador : NetworkBehaviour
{
    public int posTablero;
    public const float ALTURA_MINIMA_SALTO = 1.5f;
    public const int VALOR_MINIMO_MOVIMIENTO = 2;
    public const int VALOR_MAXIMO_MOVIMIENTO = 13; //13 porque en Random.Range los int el maximo es excluyente

   
}
