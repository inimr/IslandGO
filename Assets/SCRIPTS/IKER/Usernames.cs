using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class UserData 
{ 
public string nickName;
public GameObject pieza;
} 

[System.Serializable]
public class selector 
{ 
 public int contadorInterno;   
 public TMP_InputField imputfieldSelector;  
 public Image imagenSelector;
}


public class Usernames : MonoBehaviour
{
    public List<string> names = new(); 
    public List<selector> selectorList;
   

    public InputField Username;
    public List<GameObject>piezasList;


    public bool Inicio;
    public MenuInicio MInicio;

    public UserData user;
    public List<UserData>userdataList = new List<UserData>();

    public int selectorContador;
    
    public List<Sprite> imagenes;
    [SerializeField]  private List<Sprite> imagenesSeleccionadas;


    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CargarDatos();

    }

    private void Start()
    {
        userdataList.Add(null);

    }

    public void Confirmar() {
        
      

        for (int i = 0; i<userdataList.Count; i++) 
        {
           userdataList[i].nickName = selectorList[i].imputfieldSelector.text;           
           userdataList[i].pieza = piezasList[selectorList[i].contadorInterno];

        }

      GuardarDatos();
    }

   public void SeleccionSuma(int posicionlista) 
   {
        selectorList[posicionlista].contadorInterno ++;

        int ultimoContadorAceptado = -1;
        List<int> listaContadorInterno = new();
        int cantidadPlayers = userdataList.Count;

        for (int i = 0; i < cantidadPlayers; i++)
        {
            listaContadorInterno.Add(selectorList[i].contadorInterno);
        }

        for(int i = imagenes.Count; i >= 0; i--)
        {
            if (!listaContadorInterno.Contains(i))
            {
                ultimoContadorAceptado = i;
                break;
            }
            ultimoContadorAceptado = -1; //Dara este valor si todas las fichas estan ocupadas
        }
        if (ultimoContadorAceptado == -1) return; //Paramos esto para que no haga nada porque no hay mas 
                                                  //fichas disponibles

        //Miramos si el contadorInterno esta usado
        while (listaContadorInterno.Contains(selectorList[posicionlista].contadorInterno))
        {
            selectorList[posicionlista].contadorInterno++; //Este sera el siguiente int disponible
        }
        
        if ( selectorList[posicionlista].contadorInterno >= 5) {selectorList[posicionlista].contadorInterno = ultimoContadorAceptado;}

        selectorList[posicionlista].imagenSelector.sprite = imagenes[selectorList[posicionlista].contadorInterno];

    
   }


   public void SeleccionResta(int posicionlista) 
   {
    selectorList[posicionlista].contadorInterno --; 
        
    if ( selectorList[posicionlista].contadorInterno <= 0) {selectorList[posicionlista].contadorInterno = 0;}

    selectorList[posicionlista].imagenSelector.sprite = imagenes[selectorList[posicionlista].contadorInterno];
        
   }


    public void GuardarDatos() 
    { 
        PlayerPrefs.SetString("NombreJugador", MInicio.UsernameInput.text); 
       // PlayerPrefs.SetInt("Contadorinterno",selectorList[posicionlista].contadorInterno);

         selectorList[0].imputfieldSelector.text = PlayerPrefs.GetString("NombreJugador");
    }
    public void CargarDatos() 
    {
        MInicio.bigUsername.text =  PlayerPrefs.GetString("NombreJugador");
        selectorList[0].imputfieldSelector.text = PlayerPrefs.GetString("NombreJugador"); 
       
    }


}
