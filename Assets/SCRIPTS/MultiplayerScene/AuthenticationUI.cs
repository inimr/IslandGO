using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthenticationUI : MonoBehaviour
{


    [SerializeField] TMP_InputField setNameIP;


    private int iconChosen = -1;

    public void ModifyChosenIcon(int chosenIcon)
    {
        iconChosen = chosenIcon;
    }



    public void AuthenticatePlayer()
    {
        if (iconChosen == -1) return;

        if (setNameIP.text == null) return;

        LobbyManager.instance.Authenticate(setNameIP.text, iconChosen);


    }
}
