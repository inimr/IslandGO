using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class Dado : MonoBehaviour
{
    //Lista en la que asignar a cada cara del dado un valor
    public Transform[] diceFaces;

    public List<int> results = new List<int>();

    public Rigidbody rbDice;

    //Variable que indica la cantidad de dados que hay
    private int diceIndex = -1;

    //Variable para saber si el dado esta quieto
    public bool _hasStoppedRolling;

    //variable que informa que el dado se ha detenido
    private bool _delayFinished;    

    public int resultadoDado;


    private void Awake()
    {
        //asignamos el rigidbody del objeto al del script
        rbDice = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        Destroy(this.gameObject, 10f); //POR SI ACASO SE BUGGEA EL ASUNTO 
    }
    private void Update()
    {
        //si el dado no esta quieto no queremos saber ningun numero aun
        if (!_delayFinished) return;

        //Comprobamos si el dado ha dejado de rodar
        if(!_hasStoppedRolling && rbDice.linearVelocity.sqrMagnitude == 0f)
        {
            _hasStoppedRolling = true;

            GetNumberOnTopFace();
            
        }
    }

    [ContextMenu("Get Top Face")]
    public void GetNumberOnTopFace()
    {
        var topFace = 0;
        var lastYPosition = diceFaces[0].position.y;

        for(int i=0;i<diceFaces.Length;i++)
        {
            //como la posicion de Y mas alta en un pricipio va a ser la del numero 1
            //en este if se comprueba al tirar el dado cual es en ese momento el numero en Y
            //mas alto para saber que numero ha salido
            if(diceFaces[i].position.y>lastYPosition)
            {
                lastYPosition = diceFaces[i].position.y;
                topFace = i;
            }
        }

        resultadoDado = topFace + 1;
        //diceResult = resultadoDado;
        //Debug.Log($"Dice result { resultadoDado }" );
       

        
    }

    public void LanzarDado(float throwForce, float rollForce, int i)
    {
        diceIndex = i;

        var randomVariance = Random.Range(-1f, 1f);

        rbDice.AddForce(transform.forward * (throwForce + randomVariance), ForceMode.Impulse);

        var randX = Random.Range(0f, 1f);
        var randY = Random.Range(0f, 1f);
        var randZ = Random.Range(0f, 1f);

        rbDice.AddTorque(new Vector3(randX, randY, randZ)*(rollForce+randomVariance), ForceMode.Impulse);

        DelayResult();

    }

    private async void DelayResult()
    {
        await Task.Delay(1000);
        _delayFinished = true;
    }
}
