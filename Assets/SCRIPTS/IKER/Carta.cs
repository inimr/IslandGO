using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using JetBrains.Annotations;

public class Carta : MonoBehaviour,IPointerClickHandler //, IPointerDownHandler, IPointerUpHandler
{
    private Turnos turnos;
    public Hipotecacion logicaHipotecar;
    public Economia economia;
    public Negociar negociar;
    public Ruta ruta;

    public TextMeshProUGUI tituloCarta;

    public bool seleccionado;
    public bool enHipotecacion;

    public GameObject mejorar;
    public GameObject hipotecar;

    public float selectionOffset = 50;
    private Vector3 escalaInicial;
    // Botones que me haran falta para el modo Hipotecacion
    public Button botonPlus;
    public Button botonMinus;

    public bool clicadoHipotecacion;
    public int nivelMejoraActual; //Esto lo usaremos en el modo Hipotecacion para saber hasta cuando podemos recuperar el nivel de mejora
                                 // Una vez salido del modo Hipotecacion, lo igualaremos al BuildingData para el futuro 
    public int ID;
    public TextMeshProUGUI dueno;
    public Image colorCasa;
    public Color colorNivelActual;
    public Color colorBlanco;
   // public Color[] coloresCasa;

    public BuildingData dataCarta;
    private Casilla casillaCarta;
    
    [Header("Variables de textos")]
    [Header("Nombre de los textos")] //No modificar el .text de estos, solo el color
    
    public TextMeshProUGUI alquilerBaseConColorName;
    
    public TextMeshProUGUI[] arrayAlquilerNombres;
    [Header("Valor de los textos")] //Modificar tanto el color como el .text
    
    public TextMeshProUGUI alquilerBaseConColorTxt;
    
    public TextMeshProUGUI[] arrayAlquilerTxt;

    public TextMeshProUGUI precioMejoraTxt;
    public TextMeshProUGUI reembolsoHipotecaMejoraTxt;
    public TextMeshProUGUI hipotecaTxt;
   

    private void Awake()
    {
        
        turnos = FindAnyObjectByType<Turnos>();
        economia = FindAnyObjectByType<Economia>();
        negociar = FindAnyObjectByType<Negociar>();
        logicaHipotecar = FindFirstObjectByType<Hipotecacion>();
        ruta = FindFirstObjectByType<Ruta>();

        escalaInicial = transform.localScale;
        transform.localPosition = economia.inventario.transform.localPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerClick.GetComponent<Carta>() == null) return; //Para asegurarnos que solo lo haga en las cartas, no se si hace falta

        if (eventData.button != PointerEventData.InputButton.Left) return; //Para que solo funcione con el click izquierdo ahora mismo
            
        Carta cartaClicada = eventData.pointerClick.GetComponent<Carta>(); // No se si esto funcionara bien la verdad, o habria que hacer un this

        print("El nombre de esta carta, usando this es  " + this.name);
        //Logica al clickar la ficha en modo hipotecacion

        if (enHipotecacion)
        {
            //Detectar si la carta ha sido clicada anteriormente, si NO
            if (!cartaClicada.clicadoHipotecacion)
            {
                logicaHipotecar.SumarHipoteca(cartaClicada);
            }
            //si SI ha sido clicada
            else if (cartaClicada.clicadoHipotecacion)
            {
                logicaHipotecar.QuitarHipoteca(cartaClicada);
            }
        }
        if (ControlTarot.enMago)
        {
            if(ID == turnos.JugadorActual().ID)
            {
                ControlTarot.cartaSeleccionadaPlayer = this;
            }
            else
            {
                ControlTarot.cartaSeleccionadaOtro = this;
            }
        }
        // Si no estamos en modo hipotecacion
        else
        {
            seleccionado = !seleccionado;
            Vector3 escalaSeleccionado = new(1.1f, 1.1f, 1.1f);

            if (seleccionado)
            {
                if (negociar.panelNegociacion.activeInHierarchy)
                {
                    transform.localPosition += (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaSeleccionado;
                }
                else
                {
                    transform.localPosition += (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaSeleccionado;
                    mejorar.transform.localPosition += ((mejorar.transform.up * selectionOffset) / 2);
                    hipotecar.transform.localPosition += ((hipotecar.transform.up * selectionOffset) / 2);
                }
            }
            else
            {
                if (negociar.panelNegociacion.activeInHierarchy)
                {
                    transform.localPosition -= (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaInicial;
                }
                else
                {
                    transform.localPosition -= (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaInicial;
                    mejorar.transform.localPosition -= ((mejorar.transform.up * selectionOffset) / 2);
                    hipotecar.transform.localPosition -= ((hipotecar.transform.up * selectionOffset) / 2);
                }
            }
        }

    }
    /*
    public void OnPointerDown(PointerEventData eventData)
    {
        print("Soy On Pointer Down");
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!enHipotecacion)
        {
            PointerDownEvent.Invoke(this);
            pointerDownTime = Time.time;
        }
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print("Soy On Pointer Up");
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // EN EL INVENTARIO ESTANDAR
        if (!enHipotecacion) 
        {
            pointerUpTime = Time.time;

            PointerUpEvent.Invoke(this);

            if (pointerUpTime - pointerDownTime > .2f)
                return;

            seleccionado = !seleccionado;
            SelectEvent.Invoke(this, seleccionado);
            Vector3 escalaSeleccionado = new (1.1f, 1.1f, 1.1f);

            if (seleccionado)
            {
                if (negociar.panelNegociacion.activeInHierarchy)
                {
                    transform.localPosition += (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaSeleccionado;
                }
                else
                {
                    transform.localPosition += (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaSeleccionado;
                    mejorar.transform.localPosition += ((mejorar.transform.up * selectionOffset) / 2);
                    hipotecar.transform.localPosition += ((hipotecar.transform.up * selectionOffset) / 2);
                }
            }
            else
            {
                if (negociar.panelNegociacion.activeInHierarchy)
                {
                    transform.localPosition -= (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaInicial;
                }
                else
                {
                    transform.localPosition -= (gameObject.transform.up * selectionOffset);
                    transform.localScale = escalaInicial;
                    mejorar.transform.localPosition -= ((mejorar.transform.up * selectionOffset) / 2);
                    hipotecar.transform.localPosition -= ((hipotecar.transform.up * selectionOffset) / 2);
                }
            }
        }          
    }*/
    
    public void MejorarCarta()
    {
        // FALTA CAMBIAR EL PREFAB
        // Logica para poder deshipotecar una carta, faltaria algun cambio visual
        if (dataCarta.hipoteca)
        {
            Debug.LogWarning("Carta deshipotecada");

            
            int precio = turnos.Jugadores[turnos.ContadorTurnos()].EsTemplanza() ? dataCarta.precioMejora : dataCarta.precioMejora + (dataCarta.precioMejora / 10);
           
            economia.PerderDinero(precio, turnos.ContadorTurnos());
            dataCarta.hipoteca = false;
            return;
        }

        if (!dataCarta.grupoCompleto) return;
        economia.PerderDinero(dataCarta.precioMejora, turnos.ContadorTurnos());
        nivelMejoraActual++;
        dataCarta.nivelCasilla++;
        dataCarta.precioAlquiler = dataCarta.arrayPreciosAlquiler[dataCarta.nivelCasilla];
        CambiarColorTexto();
        ControlarBotones();

       
    }


    public void HipotecarCarta()
    {
        // Si no está la casilla mejorada, hipotecamos la casilla
        // >>>>>>>>>>>>>>>> Hay que añadir alguna logica para poder deshipotecarla <<<<<<<<<<<<<<<<< ESTOOO
        // A parte de saber de alguna manera logica que realmente esta hipotecada
        if(nivelMejoraActual == 0)
        {
            if (dataCarta.hipoteca) return;
            economia.GanarDinero(dataCarta.precioCasilla / 2, turnos.ContadorTurnos());
            dataCarta.hipoteca = true;
            hipotecar.GetComponent<Button>().interactable = false;
            
        }
        // Si está mejorada, bajamos un nivel de mejora       
        else
        {
            economia.GanarDinero(dataCarta.precioMejora / 2, dataCarta.dueno);
            nivelMejoraActual--;
            dataCarta.nivelCasilla--;
            dataCarta.precioAlquiler = dataCarta.arrayPreciosAlquiler[dataCarta.nivelCasilla];
            mejorar.GetComponent <Button>().interactable = true;
            CambiarColorTexto();
            ControlarBotones();
            // FALTA CAMBIAR EL PREFAB

        }
    }

    public void CambiarColorTexto()
    {
        //No me acaba de convencer esta manera de pintar los colores la verdad
        //Pasamos todos a blanco
        for (int i = 0; i < arrayAlquilerNombres.Length; i++)
        {
            arrayAlquilerNombres[i].color = colorBlanco;
            arrayAlquilerTxt[i].color = colorBlanco;         
        }
        alquilerBaseConColorName.color = colorBlanco;
        alquilerBaseConColorTxt.color = colorBlanco;

        //Modificamos el color de los seleccionados si es mayor que 0 el nivel
        if(nivelMejoraActual > 0)
        {
            arrayAlquilerNombres[nivelMejoraActual].color = colorNivelActual;
            arrayAlquilerTxt[nivelMejoraActual].color = colorNivelActual;
        }
        // Si el nivel es 0 miramos si el grupo esta completo o no para sobresaltar uno de los dos
        else
        {
            if (dataCarta.grupoCompleto)
            {
                alquilerBaseConColorName.color = colorNivelActual;
                alquilerBaseConColorTxt.color = colorNivelActual;
            }
            else
            {
                arrayAlquilerNombres[nivelMejoraActual].color = colorNivelActual;
                arrayAlquilerTxt[nivelMejoraActual].color= colorNivelActual;
            }
        }
        
    }
    // Aqui tenemos que checkear que la diferencia entre las casillas no es mas que uno
    //                  para quitar mejora / aumentar mejora
    public void ControlarBotones()
    {
        List<int> IDNivelAlto = new();

        for(int i = 0; i < ruta.infoGrupos[dataCarta.groupID].groupInfo.Count; i++)
        {
            BuildingData dataI = ruta.infoGrupos[dataCarta.groupID].groupInfo[i];

            for (int j = 0; j < ruta.infoGrupos[dataCarta.groupID].groupInfo.Count; j++)
            {
                BuildingData dataJ = ruta.infoGrupos[dataCarta.groupID].groupInfo[j];

                //Calculamos la diferencia entre las dos casillas, si es menos que 1, pasamos a la siguiente iteracion
                if (Mathf.Abs(dataI.nivelCasilla - dataJ.nivelCasilla) < 1)  continue; 

                //Añadimos el ID del edificio que tenga el valor de casilla mas alta y que no este en la lista
                if (dataI.nivelCasilla < dataJ.nivelCasilla)
                {
                    if (IDNivelAlto.Contains(dataJ.IDentificador)) continue;

                    IDNivelAlto.Add(dataJ.IDentificador);
                }
                else
                {
                    if (IDNivelAlto.Contains(dataI.IDentificador)) continue;

                    IDNivelAlto.Add(dataI.IDentificador);
                }
            }

        }
        // Si la lista esta vacia implicaria que todas las casillas tienen el mismo nivel, asique todos los botones 
        // seran interactuables salvo que sean nivel 3
        if(IDNivelAlto.Count == 0)
        {
            foreach(GameObject obj in turnos.JugadorActual().InventarioJugador)
            {
                Carta carta = obj.GetComponent<Carta>();
                if(carta.dataCarta.groupID == dataCarta.groupID)
                {
                    carta.mejorar.GetComponent<Button>().interactable = true;
                    carta.hipotecar.GetComponent<Button>().interactable = true;
                    if(carta.nivelMejoraActual >=3) carta.mejorar.GetComponent<Button>().interactable = false;

                }

            }
            return;
        }

        // Si hay diferencia respecto al mas bajo, lo(s) tendremos en la lista, tenemos que desactivar
        // los botones de mejora de esas cartas

        foreach(GameObject obj in turnos.JugadorActual().InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();
            for(int i = 0; i < IDNivelAlto.Count; i++)
            {
                if (IDNivelAlto[i] == carta.ID)
                {
                    carta.mejorar.GetComponent<Button>().interactable = false;
                    carta.hipotecar.GetComponent <Button>().interactable = true;
                }
            }
        }

        // Aquellos que tengan el ID del mismo grupo pero que NO estan en la lista IDNivelAlto
        // implica que tienen un nivel menos,asique les quitamos la posibilidad de hipotecar

        int IDdelGrupo = dataCarta.groupID;

        foreach (GameObject obj in turnos.JugadorActual().InventarioJugador)
        {
            Carta carta = obj.GetComponent<Carta>();

            //Si la carta tiene un ID del grupo diferente o si la lista contiene su ID, pasamos al siguiente
            if (carta.dataCarta.groupID != IDdelGrupo || IDNivelAlto.Contains(carta.ID)) continue;              
            

            //Desactivamos la opcion de Hipotecar porque su nivel es ya uno menos que el mas alto
            carta.hipotecar.GetComponent<Button>().interactable = false;

        }

    }

  
}
