using UnityEngine;

[CreateAssetMenu(fileName = "TarotSO", menuName = "Scriptable Objects/CartaTarotSO")]
public class TarotSO : ScriptableObject
{
    public string nombre;
    public int ID;
   [TextArea(1,5)] public string descripcionCarta;
    public int precio;

}
