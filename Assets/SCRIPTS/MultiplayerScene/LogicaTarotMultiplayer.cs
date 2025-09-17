using UnityEngine;

public class LogicaTarotMultiplayer : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TableManager.Instance.OnCasillaTarotCaida += TableManager_OnCasillaTarotCaida;
    }

    private void TableManager_OnCasillaTarotCaida()
    {

    }
}
