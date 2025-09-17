using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlBotones : MonoBehaviour
{
    public Animator AnimPanelNegocio;

    public GameObject BotonNegocio;

    public GameObject BotonOcultar; 
    
    public void SelecionNegocio() {AnimPanelNegocio.SetBool("Negociar",true);}

    public void OcultarNegocio() 
    {
        AnimPanelNegocio.SetBool("Negociar",false);
       // BotonNegocio.SetActive(true);
        
    }

    public void EjecutarBotonOcultar()
    {
        BotonOcultar.GetComponent<Button>().onClick.Invoke();
    }

}
