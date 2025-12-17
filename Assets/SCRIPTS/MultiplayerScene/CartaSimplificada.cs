using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CartaSimplificada : MonoBehaviour, IPointerClickHandler
{
    [Header("Variables a modificar de la propia carta")]
    [SerializeField] Image fondoCarta;
    [SerializeField] TextMeshProUGUI nombreCasilla;
    [SerializeField] Button botonSubirLvl;
    [SerializeField] Button botonBajarLvl;

    private Casilla casillaTablero; //Esto nos hara falta cuando vayamos a clicarlo en intercambios y demas
    private DiaryManager diaryManager;
    private bool isClicked;


    private void Start()
    {
        diaryManager = FindFirstObjectByType<DiaryManager>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!diaryManager.GetModoNegociacion()) return;
        isClicked = !isClicked;

        //Añadir aqui todo lo visual cuando se haya hecho

        if (isClicked)
        {
            diaryManager.AddCartaOfertaNegociacion(casillaTablero.GetPosTablero());
        }
        else
        {
            diaryManager.RemoveCartaOfertaNegociacion(casillaTablero.GetPosTablero());
        }

    }

    public void RellenarDatosCarta(Sprite fondo, Casilla cas)
    {
        fondoCarta.sprite = fondo;
        nombreCasilla.text = cas.nameCasilla;
        casillaTablero = cas;

    }


    

}
