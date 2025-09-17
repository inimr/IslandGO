using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CartaTarot : MonoBehaviour, IPointerClickHandler
{
    [Header("Variables de la propia carta")]
    public TextMeshProUGUI titulo;
    public TextMeshProUGUI descripcion;
    public int ID;
    public int dueno;
    public int precio;
    public bool enTienda = false; //Para que no se pueda clicar en la tienda de las cartas
    
    private ControlTarot controlTarot;
    private void Start()
    {
        controlTarot = FindFirstObjectByType<ControlTarot>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Logica al clicar
        if (enTienda) return;
        print(this + " carta clicada");
        controlTarot.CambiarValorSeleccionTarot(ID);
        controlTarot.PanelConfirmacionTarot();
        
    }

   
}
