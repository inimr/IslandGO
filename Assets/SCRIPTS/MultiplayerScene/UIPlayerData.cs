using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerData : MonoBehaviour
{
    [SerializeField] private GameObject resalto;
    [SerializeField] private TextMeshProUGUI dineroText;
    [SerializeField] private TextMeshProUGUI nombreText;
    [SerializeField] private Image imagenPlayer;

    /// <summary>
    /// Método que se usa para actualizar el texto del dinero 
    /// </summary>
    /// <param name="nuevoDinero"></param>
    public void ModificarTexto(int nuevoDinero)
    {
        dineroText.text = nuevoDinero.ToString();
    }
    /// <summary>
    /// Método que se usa para actualizar el texto del nombre del Jugador
    /// </summary>
    /// <param name="nombre"></param>
    public void ModificarTexto(string nombre)
    {
        nombreText.text = nombre;
    }

    public void CambiarEstadoResalto()
    {
        resalto.SetActive(!resalto.activeSelf);
    }
}
