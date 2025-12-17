using UnityEngine;

public class AuthenticationUIBotton : MonoBehaviour
{

    [SerializeField] int ID;

    private AuthenticationUI authenticationUI;
    void Start()
    {
        authenticationUI = FindFirstObjectByType<AuthenticationUI>();
    }

  
   
    public void ClickMethod()
    {
        authenticationUI.ModifyChosenIcon(ID);
    }




}
