using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventoAnimacionNegociar : MonoBehaviour
{

    public Negociar negociar;
    //Por ahora dejaremos este apaño asi
    public void ActivarBotonNegociacion()
    {

        negociar.ActivarBoton();
    }
}
