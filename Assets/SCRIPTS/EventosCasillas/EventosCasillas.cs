using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "EventosCasillas", menuName = "Scriptable Objects/EventosCasillas")]
public class EventosCasillas : ScriptableObject
{
    public int ID;

    public bool esKarma;
    [ShowIf("esKarma")]
    public int cantidadKarma;

    public bool esDinero;
    [ShowIf("esDinero")]
    public int cantidadDinero;
    [TextArea(1, 5)] public string cuerpoBase;

    [TextArea(1,5)] public string cuerpoTexto;

    public bool tieneEleccion;
    [ShowIf("tieneEleccion")]
    public string textoBotonSi, textoBotonNo;


    private void OnValidate()
    {
        if (esDinero)
        {
            cuerpoTexto = cuerpoBase + cantidadDinero.ToString() + ".";
        }
    }


}
