using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BotonSelectorClase : MonoBehaviour
{
    [SerializeField] Image iconoBoton;
    [SerializeField] TextMeshProUGUI textoDescripcionBoton;
    
    private int numeroClaseSeleccionado;

    [SerializeField] bool selectorImpostor; // Se activa en el inspector, en el panel que solo se activa si el jugador es Impostor
                                            // Se podria sustituir por player.GetImpostor()??
    public void MetodoClickClase()
    {
        ClassesSO claseData = ClassManager.Instance.GetClass(numeroClaseSeleccionado);

        Classes claseEscogida = claseData.clase;
        Jugador player = GameManagerMultiplayer.Instance.ownerPlayer;
        player.SetClass(claseEscogida);
        if (numeroClaseSeleccionado == ClassManager.NUMERO_IMPOSTOR)
        {
            player.SetImpostor(true);
        }
        ClassManager.Instance.MandarInfoClickadoServerRpc(numeroClaseSeleccionado, player.GetImpostor());

      
    }

    public void ModificarValoresNumeroClase(int nuevoNumero)
    {
        numeroClaseSeleccionado = nuevoNumero;
        iconoBoton.sprite = ClassManager.Instance.GetClass(nuevoNumero).spriteBoton;
        textoDescripcionBoton.text = ClassManager.Instance.GetClass(nuevoNumero).textoDescriptivo;
    }
     
  
   
}
