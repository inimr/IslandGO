using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CartaCasilla : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI tituloCarta;

    public bool seleccionado;
    public bool enHipotecacion;

    [SerializeField] GameObject mejorar;
    [SerializeField] GameObject hipotecar;

    // Botones que me haran falta para el modo Hipotecacion
    [SerializeField] Button botonPlus;
    [SerializeField] Button botonMinus;

    public TextMeshProUGUI dueno;
    public Image colorCasa;
    public Color colorNivelActual;
    public Color colorBlanco;
    // public Color[] coloresCasa;

    private Casilla casillaCarta;

    [Header("Variables de textos")]
    [Header("Nombre de los textos")] //No modificar el .text de estos, solo el color

    public TextMeshProUGUI alquilerBaseConColorName;

    public TextMeshProUGUI[] arrayAlquilerNombres;
    [Header("Valor de los textos")] //Modificar tanto el color como el .text

    public TextMeshProUGUI alquilerBaseConColorTxt;

    public TextMeshProUGUI[] arrayAlquilerTxt;

    public TextMeshProUGUI precioMejoraTxt;
    public TextMeshProUGUI reembolsoHipotecaMejoraTxt;
    public TextMeshProUGUI hipotecaTxt;

    public void OnPointerClick(PointerEventData eventData)
    {
        //IREMOS RELLENANDOLO MAS ADELANTE
    }



    public void RellenarDatosCarta()
    {
        // SI MODIFICAMOS EL PREFAB DE LA CARTA HABRA QUE MODIFICAR ESTE SCRIPT TAMBIEN <<<<<<<<<<
        gameObject.name =  "Carta de" + casillaCarta.name;

        tituloCarta.text = casillaCarta.nameCasilla;

        for(int i = 0; i < arrayAlquilerNombres.Length; i++)
        {
            arrayAlquilerTxt[i].text = casillaCarta.arrayPreciosAlquiler[i].ToString();
        }

        alquilerBaseConColorTxt.text = (casillaCarta.arrayPreciosAlquiler[0] * 2).ToString();
        arrayAlquilerNombres[0].color = colorNivelActual;
        arrayAlquilerTxt[0].color = colorNivelActual;
        precioMejoraTxt.text = casillaCarta.precioMejora.ToString();
        reembolsoHipotecaMejoraTxt.text = (casillaCarta.precioMejora / 2).ToString();
        hipotecaTxt.text = (casillaCarta.precioCasilla / 2).ToString();
        colorCasa.color = casillaCarta.colorCasa;


    }

    public void SetCasillaCarta(Casilla casilla)
    {
        casillaCarta = casilla;
        RellenarDatosCarta();
    }
    public Casilla GetCasilla()
    {
        return casillaCarta;
    }
}
