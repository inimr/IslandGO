using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



[Serializable]
public class DiaryData
{
    public string name;
    public Image playerIconImage;
    public Image playerClassImage;
    public Slider karmaSlider;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI moneyText;
}

public class DiaryManager : MonoBehaviour
{

    [SerializeField] DiaryData[] playerDiaryDataArray;

    private bool isDiaryUpdated = true;

  

    public void CheckIfDiaryUpdated()
    {


        if (isDiaryUpdated) return;

        ulong IDPlayer = (ulong)GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
        GameManagerMultiplayer.Instance.ActualizarDiarioRpc(IDPlayer);

    }


    public void ActualizarUpdate(bool value)
    {
        isDiaryUpdated = value;
    }

    public void ActualizarValoresDiario()
    {
        isDiaryUpdated = true;

        //CREAR TODAS LAS CARTAS NECESARIAS ETC PARA QUE EL DIARIO FUNCIONE AL 100% SIN NECESIADAD DE HACER
        // MAS COSAS
    }
}
