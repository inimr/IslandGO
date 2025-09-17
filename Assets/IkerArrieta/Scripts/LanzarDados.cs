using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class LanzarDados : MonoBehaviour
{
    [Header("Referencias a otros Scripts")]
    [Space(10)]
    public Ruta ruta;
    public Ficha ficha;
    public Dado lanzarDado;
    public Economia economia;
    private Turnos controlTurnos;
    [Space(10)]
    [Header("Info de Dados")]
    [Space(10)]
    public int cantDados = 2;
    public float throwForce=2;
    public float rollForce=5;

    public List<GameObject> _dadoSpawneado = new List<GameObject>();

    public int dado1;
    public int dado2;

    private void Start()
    {
        controlTurnos = FindFirstObjectByType<Turnos>();
    }
    public IEnumerator TirarDado()
    {
        if(_dadoSpawneado.Count>=1)
        {
            _dadoSpawneado.Clear();
        }        

        foreach(var die in _dadoSpawneado)
        {
            Destroy(die);
        }

        for(int i=0; i<cantDados; i++)
        {
            Dado dado = Instantiate(lanzarDado, transform.position, transform.rotation);
            _dadoSpawneado.Add(dado.gameObject);
            dado.LanzarDado(throwForce, rollForce, i);
        }

        yield return new WaitForSeconds(5);

        CalculaResultado(controlTurnos.JugadorActual());

    }

    public void CalculaResultado(Ficha jugador) 
    {
        
        //Recopilamos los numeros que han salido en la tirada de los dados
        dado1 = _dadoSpawneado[0].GetComponent<Dado>().resultadoDado;
        dado2 = _dadoSpawneado[1].GetComponent<Dado>().resultadoDado;

        jugador.casillasAvanzar = dado1 + dado2;

        if (jugador.EnLuna())
        {
            if (jugador.casillasAvanzar > 4)
            {
                jugador.casillasAvanzar = 4;
            }
            jugador.DesactivarLuna();
        }

        if (jugador.EnFuerza())
        {
            jugador.casillasAvanzar *= 2;
            jugador.DesactivarFuerza();
        }
        //dado1 = dado2; //<<<<<<<<<< PARA QUE SEA SIEMPRE DOBLES
        //jugador.casillasAvanzar = 2; //<<<<<<<<<<<<<< PARA MOVERNOS LO QUE QUERAMOS

        // Aqui hay que revisar los datos de la casilla en la que te encuentras
        if (dado1>0&&dado2>0)
        {
            jugador.ActualizarPosicion();
        }
        else
        {
            return;
        }

        Destroy(_dadoSpawneado[0]);
        Destroy(_dadoSpawneado[1]);

        jugador.estadoActual = EstadoTurnoJugador.FaseCasillas;        
        jugador.TetocaMover = true;
        jugador.Mover();
        
    }

}
