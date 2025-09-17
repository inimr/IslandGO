using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuInicio : MonoBehaviour
{
    [SerializeField]private Usernames Nombres;
    public List<GameObject> nombresOBJ;
    public int index;
    private GameObject botonSumar;
    private GameObject botonRestar;
    private bool activo;
    private GameObject opciones;

    public Usernames usernamesScript;
    public TextMeshProUGUI bigUsername;
    public TMP_InputField UsernameInput;
    //private Image userImage;

   
    // Update is called once per frame

    public void Awake()
    {
        
    }
    private void Start()
    {
        if (bigUsername.text == null) { bigUsername.text = " Username";}
    }



    public void Iniciar_partida(int escena) {

      //GameManager.instancia.CargarEscena("Juego");
        SceneManager.LoadScene(escena);
        Nombres.Inicio = true;
    }
    public void Salir() { Application.Quit(); }

    public void SumarNombre() 
    { 
        if (index < 5) 
        { 
         index++;

         nombresOBJ[index].SetActive(true);

         usernamesScript.userdataList.Add(null); 
            
        }
       else {index = 5; }

    }

    public void RestarNombre() 
    { 
         if (index > 0)
         { 
          nombresOBJ[index].SetActive(false);

          index--;

          usernamesScript.userdataList.RemoveAt(usernamesScript.userdataList.Count-1);
         } 

         else {index = 0; }
        
    }

    public void UserNameImage() 
    {
        if (activo == true) {opciones.SetActive(false); }
        else {opciones.SetActive(true);}
    }

    public void OpcionesFuncion(int indexImage) 
    {
        /*
        userImage.sprite =imagensUserOpciones[indexImage].sprite; 
        userImage.color = imagensUserOpciones[indexImage].color;
        */


    }

    public void BotonGuardar() 
    { 
      if (UsernameInput.text == null) {bigUsername.text = "Username"; }
      else {bigUsername.text = UsernameInput.text;}   
    }

    
}
