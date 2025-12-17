using UnityEngine;

public class BotonSelectorDiario : MonoBehaviour
{

    [SerializeField] int ID; //Se asigna en el inspector
    [SerializeField] DiaryManager diaryManager;

    
    

    public void MetodoBotonDatos()
    {
        diaryManager.CambiarPlayerDatos(ID);
    }


    public void MetodoBotonNegociacion()
    {
        diaryManager.ActualizarNegociado(ID);
    }
}
