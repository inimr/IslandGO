using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FichaPandillero : FichaNoJugador
{

    private int turnosVida = 3;


    public void Movimiento() 
    {
        int casillasMover = Random.Range(VALOR_MINIMO_MOVIMIENTO, VALOR_MAXIMO_MOVIMIENTO);
        StartCoroutine(Movement(casillasMover));
    }

    public IEnumerator Movement(int numCasillas)
    {
        int posInicial = posTablero;

        // MODIFICAR EL VALOR DE LA CASILLA DONDE ESTA ACTUALMENTE EL TAROT
        TableManager.Instance.GetCasillaArray()[posInicial].ModificarContienePandillero(false);
        while (numCasillas > 0)
        {
            posTablero++;
            posTablero %= TableManager.Instance.GetCasillaArray().Length;
            List<Transform> posCamino = TableManager.Instance.GetCaminoTablero()[posTablero].GetPosicionesCamino();
            List<Vector3> points = new();

            Vector3 nextPos = TableManager.Instance.GetCasillaArray()[posTablero].transform.position;

            for (int i = 0; i <= posCamino.Count; i++)
            {
                Vector3 initialPos = i == 0 ? transform.position : posCamino[i - 1].position;
                Vector3 endPos = i == posCamino.Count ? nextPos : posCamino[i].position;
                float height = ((initialPos.y + endPos.y) / 2) + ALTURA_MINIMA_SALTO;
                Vector3 midPoint = new((initialPos.x + endPos.x) / 2, height, (initialPos.z + endPos.z) / 2);
                float vertexCount = 12;
                for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
                {
                    var tangent1 = Vector3.Lerp(initialPos, midPoint, ratio);
                    var tangent2 = Vector3.Lerp(midPoint, endPos, ratio);
                    var curve = Vector3.Lerp(tangent1, tangent2, ratio);
                    points.Add(curve);
                }
                points.Add(endPos);
            }
            for (int i = 0; i < points.Count; i++)
            {
                while (transform.position != points[i])
                {
                    transform.position = Vector3.MoveTowards(transform.position, points[i], 10 * Time.deltaTime);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.1f);

            numCasillas--;
        }
        TableManager.Instance.GetCasillaArray()[posTablero].ModificarContienePandillero(true);
        GameManagerMultiplayer.Instance.TareaEntreTurnosTerminada();

    }

    public void ReducirTurnosVida()
    {
        turnosVida--;      
    }

    public int GetTurnosVida()
    {
        return turnosVida; 
    }


}
