using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ClassesSO", menuName = "Scriptable Objects/ClassesSO")]
public class ClassesSO : ScriptableObject
{

    new public string name;
    public Classes clase;

    [TextArea(1, 5)] public string textoDescriptivo;

    public Sprite spriteBoton;

    public Sprite iconoPlayer;
}
