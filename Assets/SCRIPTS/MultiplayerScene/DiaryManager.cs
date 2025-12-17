using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;



[Serializable]
public class DiaryData
{
    public string name;
    public GameObject objetoPadre; //Usaremos este objeto para desactivar todo en caso de que el jugador este enDerrota

    [Header("Iconos necesarios")]
    public Image playerIconImage;
    public Image playerClassImage;
    public Image classImageDatos;

    [Header("Sliders")]
    public Slider karmaSlider;

    [Header("Text necesarios")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI ingresosPasivosText;
    public TextMeshProUGUI numPropiedadesText;
    public TextMeshProUGUI textClassDescriptionDatos;

    [Header("Transform y GameObject necesarios")]
    public GameObject contenidoDatos;
    public Transform contentScrollViewGeneral;
    public GameObject botonHabilidadDatos;
    
   
    
}

public class DiaryManager : MonoBehaviour
{
    private bool isDiaryUpdated;

    [SerializeField] DiaryData[] playerDiaryDataArray;



    [Header("Variables necesarias para el funcionamiento del Diario")]

    [SerializeField] GameObject prefabCartaSimplificada; //O hacer Script para la carta simplificada, que nos hara falta 
    [SerializeField] GameObject ventanaNegociacionAceptada;
    [SerializeField] GameObject ventanaNegociacionRechazada;
    [SerializeField] Sprite classIconPlaceholderSprite;

    [Header("Diferentes secciones del Diario")]

    [SerializeField] GameObject panelInicialDiario;
    [SerializeField] GameObject windowCreacionNegociacion;
    [SerializeField] GameObject windowNegociacionRecibida;

    [Header("Datos")]

    [SerializeField] GameObject ventanaDatos;
    [SerializeField] Image imagenDatosJugadorElegido;
    [SerializeField] TextMeshProUGUI datosNombreJugadorElegido;
    [SerializeField] List<Button> listaBotonesDatos;

    [Header("Listas")]

    [SerializeField] List<Sprite> fondosCartasSimplificadas;

    [Header("Negociacion/Jugador")]
    [SerializeField] Image iconoJugador;
    [SerializeField] TextMeshProUGUI textoNombreJugador;
    [SerializeField] TMP_InputField dineroOfrecerInputField;
    [SerializeField] Transform contentScrollViewPropiedadesNegociacion;
    [SerializeField] Transform contentScrollViewTarotNegociacion;
    private bool modoNegociacion;
    private List<int> listaPropiedadesOfrecer = new();

    [Header("Negociacion/Oponente")]
    [SerializeField] Image iconoOponenteNegociacion;
    [SerializeField] TextMeshProUGUI textoNombreOponenteNegociacion;
    [SerializeField] TMP_InputField dineroPedirOponenteIF;
    [SerializeField] Transform contentSVOponentePropiedades;
    [SerializeField] Transform contentSVOponenteTarot;
    [SerializeField] List<Button> botonEleccionNegociacion;
    private int numeroAnteriorPulsado = -1;
    private List<int> listaPropiedadesPedir = new();

    [Header("Negociacion Recibida / Negociante")]
    [SerializeField] Image iconoPlayerNegociante;
    [SerializeField] TextMeshProUGUI textNombreNegociante;
    [SerializeField] TextMeshProUGUI textDineroOfrecido;
    [SerializeField] Transform contentPropiedadesNegociante;
    [SerializeField] Transform contentTarotNegociante;

    [Header("Negociacion Recibida / Jugador")]
    [SerializeField] Image iconoJugadorNegociado;
    [SerializeField] TextMeshProUGUI textJugadorNegociado;
    [SerializeField] TextMeshProUGUI textDineroPedido;
    [SerializeField] Transform contentPropiedadesNegociado;
    [SerializeField] Transform contentTarotNegociado;
    private List<int> listaPropiedadesPedida = new();
    private List<int> listaPropiedadesOfrecida = new();
    
    
    static int SortByGroupID(Casilla c1, Casilla c2)
    {
        int carta1 = c1.GetGroupID();
        int carta2 = c2.GetGroupID();

        int result = carta1.CompareTo(carta2);
        if (result == 0)
        {
            result = c1.ID.CompareTo(c2.ID);
        }
        return result;
    }
    
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

        LimpiarListasDiarioGeneral();

       
        foreach(GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
        {            
            RellenarPaginaInicialDiario(data);

            for (int i = 0; i < data.listaPropiedades.Count; i++)
            {
                Casilla casilla = data.listaPropiedades[i];

                GameObject carta = Instantiate(prefabCartaSimplificada, playerDiaryDataArray[data.ID].contentScrollViewGeneral);

                carta.GetComponent<CartaSimplificada>().RellenarDatosCarta(fondosCartasSimplificadas[casilla.GetGroupID()], casilla);
            }

            data.listaPropiedades.Sort(SortByGroupID);

        }

        int valorClaseJugadorActual = (int)GameManagerMultiplayer.Instance.ownerPlayer.GetClass();
        imagenDatosJugadorElegido.sprite = ClassManager.Instance.GetClass(valorClaseJugadorActual).iconoPlayer;
        datosNombreJugadorElegido.text = GameManagerMultiplayer.Instance.ownerPlayer.name;

        isDiaryUpdated = true;
        panelInicialDiario.SetActive(true);
    }
    private void LimpiarListasDiarioGeneral()
    {
        for (int i = 0; i < playerDiaryDataArray.Length; i++)
        {
            Transform transform = playerDiaryDataArray[i].contentScrollViewGeneral;

            for (int y = transform.childCount - 1; y >= 0; y--)
            {
                Destroy(transform.GetChild(y).gameObject);
            }

        }
    }

    private void RellenarPaginaInicialDiario(GameManagerMultiplayer.PlayerData data) 
    {

        DiaryData diaryData = playerDiaryDataArray[data.ID];

        diaryData.objetoPadre.SetActive(true);
        if (data.player.enDerrota.Value)
        {
            diaryData.objetoPadre.SetActive(false);
            return;
        }

        int valorClase = (int)data.player.GetClass();
        
        if(valorClase < 1)
        {
            //Aqui pondremos la logica que quiza necesitemos al comienzo de la partida / antes de que se escojan las clases
            // por si queremos ocultar o hacer algo con ello

            //PODEMOS HACERLO CON UN OPERADOR TERNARIO
        }

        diaryData.nameText.text = data.player.name;
        diaryData.moneyText.text = data.player.dinero.Value.ToString();
        diaryData.ingresosPasivosText.text = "El número de propiedades es " + data.listaPropiedades.Count + " \n" + "Los ingresos pasivos son " + data.player.dineroPasivo.Value.ToString();

        diaryData.textClassDescriptionDatos.text =  valorClase > 1 ? ClassManager.Instance.GetClass(valorClase).textoDescriptivo: "No hay clase seleccionada todavia";
        //diaryData.numPropiedadesText.text = data.listaPropiedades.Count.ToString();  /// Lo comentamos porque no esta asignado

        diaryData.karmaSlider.value = data.player.karma.Value;

        Sprite classSprite = valorClase > 1 ? ClassManager.Instance.GetClass(valorClase).iconoPlayer : classIconPlaceholderSprite;
        diaryData.playerIconImage.sprite = classSprite; //<< Esto sera el icono del player, que escogera prepartida
        diaryData.playerClassImage.sprite = classSprite;
        diaryData.classImageDatos.sprite = classSprite;


        bool activar = data.ID == GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
        diaryData.contenidoDatos.SetActive(activar);
        diaryData.botonHabilidadDatos.SetActive(activar);


        // Habra que comprobar mas adelante si la clase del jugador usa el boton de habilidad o no para quitarla
        // Tambien podemos aprovechar para no modificar mas de una vez algunas cosas pero ya se vera
    }
  

    //--------------------------------------- DATOS ------------------------------------//
    /// <summary>
    /// Este metodo es el que usaremos para abrir la pestaña de datos siempre pues así siempre se abrira con la pestaña del jugador abierta.
    /// </summary>
    public void AbrirVentanaDatos()
    {
        foreach (GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
        {
            listaBotonesDatos[data.ID].gameObject.SetActive(!data.player.enDerrota.Value);
            ClassesSO clase = ClassManager.Instance.GetClass((int)data.player.GetClass());
            bool b = clase.habilidadClickable && data.player.GetPlayerID() == GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
            playerDiaryDataArray[data.ID].botonHabilidadDatos.SetActive(b);
           
        }
        CambiarPlayerDatos(GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID());
        ventanaDatos.SetActive(true);
    }

    public void CambiarPlayerDatos(int IDBoton)
    {
        if (GameManagerMultiplayer.Instance.listaPlayers[IDBoton].player.enDerrota.Value) return;
        for(int i = 0; i < playerDiaryDataArray.Length; i++)
        {
            playerDiaryDataArray[i].contenidoDatos.SetActive(i == IDBoton);
        }

        imagenDatosJugadorElegido.sprite = playerDiaryDataArray[IDBoton].playerIconImage.sprite;
        datosNombreJugadorElegido.text = playerDiaryDataArray[IDBoton].nameText.text;
    }

    //-------------------------------- NEGOCIACION -----------------------------//
    
    public void AbrirVentanaNegociacion()
    {

        int ID = GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
        iconoJugador.sprite = playerDiaryDataArray[ID].playerIconImage.sprite;
        textoNombreJugador.text = playerDiaryDataArray[ID].nameText.text;
        botonEleccionNegociacion[ID].interactable = false;
        modoNegociacion = true;
        foreach (GameManagerMultiplayer.PlayerData data in GameManagerMultiplayer.Instance.listaPlayers)
        {
            botonEleccionNegociacion[data.ID].gameObject.SetActive(!data.player.enDerrota.Value);
            // Habra que modificar los iconos de los jugadores para que sepan cual es cual
        }
        foreach (Casilla cas in GameManagerMultiplayer.Instance.listaPlayers[GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID()].listaPropiedades)
        {
            CartaSimplificada carta = Instantiate(prefabCartaSimplificada, contentScrollViewPropiedadesNegociacion).GetComponent<CartaSimplificada>();
            carta.RellenarDatosCarta(fondosCartasSimplificadas[cas.GetGroupID()], cas);

        }
        //No se si dejarlo en blanco y que se active al elegir uno o hacer logica para que el 
        //siguiente de la lista se ponga, creo que por ahora en blanco

        windowCreacionNegociacion.SetActive(true);
    }

    public void ActualizarNegociado(int ID)
    {
        if (numeroAnteriorPulsado == ID) return;
        numeroAnteriorPulsado = ID;
        iconoOponenteNegociacion.sprite = playerDiaryDataArray[ID].playerIconImage.sprite;
        textoNombreOponenteNegociacion.text = playerDiaryDataArray[ID].nameText.text;

        List<Casilla> casillaList = GameManagerMultiplayer.Instance.listaPlayers[ID].listaPropiedades;

        for(int i = 0; i < casillaList.Count; i++)
        {
            CartaSimplificada carta = Instantiate(prefabCartaSimplificada, contentSVOponentePropiedades).GetComponent<CartaSimplificada>();
            carta.RellenarDatosCarta(fondosCartasSimplificadas[casillaList[i].GetGroupID()], casillaList[i]);

            //FALTARIA HACER LO MISMO CON LOS DE TAROT, PERO TODAVIA NO LOS HEMOS PASADO =)
        }

    }

    public void AddCartaOfertaNegociacion(int posTablero)
    {
        if(TableManager.Instance.GetCasillaArray()[posTablero].GetPropietario() == GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID())
        {
            listaPropiedadesOfrecer.Add(posTablero);
        }
        else
        {
            listaPropiedadesPedir.Add(posTablero);
        }
    }

    public void RemoveCartaOfertaNegociacion(int posTablero)
    {
        if (TableManager.Instance.GetCasillaArray()[posTablero].GetPropietario() == GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID())
        {
            listaPropiedadesOfrecer.Remove(posTablero);
        }
        else
        {
            listaPropiedadesPedir.Remove(posTablero);
        }
    }

    public void MetodoBotonNegociacion()
    {
        //COMPROBAR QUE EL JUGADOR ELEGIDO NO TENGA UNA OFERTA REALIZADA / RECIBIDA PENDIENTE MEDIANTE EL SERVIDOR, SEGURAMENTE SEA MEJOR HACERLO AL INICIAR EL PANEL NEGOCIACION
        // Y DIRECTAMENTE BLOQUEAR LOS BOTONES DE LOS PLAYERS PARA QUE NO PUEDA SELECCIONARLOS
        if (GameManagerMultiplayer.Instance.listaPlayers[numeroAnteriorPulsado].player.enNegociacion.Value) return; //<<Podriamos mandarle un mensaje diciendo que el jugador ya esta negociando

        string numeroDineroOfrecer = dineroOfrecerInputField.text;
        int dineroOfrecer = 0;
        int.TryParse(numeroDineroOfrecer, out dineroOfrecer);

        string numeroDineroPedir = dineroPedirOponenteIF.text;
        int dineroPedir = 0;
        int.TryParse(numeroDineroPedir, out dineroPedir);

        if (dineroOfrecer > GameManagerMultiplayer.Instance.ownerPlayer.dinero.Value)
        {
            //Avisar al jugador que no tiene tanto dinero en una ventanita
            return;
        }

        foreach (int numero in listaPropiedadesOfrecer) 
        {
            if (TableManager.Instance.GetCasillaArray()[numero].nivelAlquiler > 0)
            {
                //Panel preguntando si seguro que quieres intercambiar un edificio mejorado y que se perderian las mejoras y el dinero de ellas
                return;
            }
        }

        int[] arrayOfrecer = listaPropiedadesOfrecer.ToArray();
        int[] arrayPedir = listaPropiedadesPedir.ToArray();

       

        ulong IDPlayer = (ulong)numeroAnteriorPulsado;
        int ownerPlayerID = GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();

        //HABRA QUE AÑADIR LAS CARTAS DE TAROT MAS TARDE
        GameManagerMultiplayer.Instance.MandarOfertaJugadorRpc(arrayOfrecer, arrayPedir, dineroOfrecer, dineroPedir,ownerPlayerID, GameManagerMultiplayer.Instance.RpcTarget.Single(IDPlayer, RpcTargetUse.Temp));
        GameManagerMultiplayer.Instance.ModificarValoresNegociacionRpc(ownerPlayerID, numeroAnteriorPulsado, true);


    }

    public void RellenarDatosOfertaNegociadora(int[] listaOfrecida, int[] listaPedida, int dineroOfrecido, int dineroPedido, int IDJugadorEnviante)
    {
        //Si el jugador NUNCA ha entrado al diario quiza de problemas, actualicemos el nombre, su icono y demas en OnNetworkSpawn() sera lo mejor

        DiaryData oponentInfo = playerDiaryDataArray[IDJugadorEnviante];
        iconoPlayerNegociante.sprite = oponentInfo.playerIconImage.sprite;
        textNombreNegociante.text = oponentInfo.nameText.text;
        textDineroOfrecido.text = dineroOfrecido.ToString();
        for(int i = 0; i < listaOfrecida.Length; i++)
        {
            CartaSimplificada carta = Instantiate(prefabCartaSimplificada, contentPropiedadesNegociante).GetComponent<CartaSimplificada>();
            Casilla cas = TableManager.Instance.GetCasillaArray()[listaOfrecida[i]];
            carta.RellenarDatosCarta(fondosCartasSimplificadas[cas.GetGroupID()], cas);
            listaPropiedadesOfrecida.Add(listaOfrecida[i]);
        }

        //FALTA TAROT

        DiaryData myInfo = playerDiaryDataArray[GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID()];
        iconoJugadorNegociado.sprite = myInfo.playerIconImage.sprite;
        textJugadorNegociado.text = myInfo.nameText.text;
        textDineroPedido.text = dineroPedido.ToString();

        for(int y = 0; y < listaPedida.Length; y++)
        {
            CartaSimplificada carta = Instantiate(prefabCartaSimplificada, contentPropiedadesNegociado).GetComponent<CartaSimplificada>();
            Casilla cas = TableManager.Instance.GetCasillaArray()[listaOfrecida[y]];
            carta.RellenarDatosCarta(fondosCartasSimplificadas[cas.GetGroupID()], cas);
            listaPropiedadesPedida.Add(listaPedida[y]);
        }

        //FALTA TAROT

    }

    public void AceptarOfertaNegociacion(int oponentPlayerID)
    {
        int playerID = GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
        GameManagerMultiplayer.Instance.ModificarValoresNegociacionRpc(oponentPlayerID, playerID, false);


        string dineroOfrecidoText = textDineroOfrecido.text;
        int dineroOfrecido = 0;
        int.TryParse(dineroOfrecidoText, out dineroOfrecido);

        string dineroPedidoText = textDineroPedido.text;
        int dineroPedido = 0;
        int.TryParse(dineroPedidoText, out dineroPedido);
        int[] arrayOfrecido = listaPropiedadesOfrecida.ToArray();
        int[] arrayPedido = listaPropiedadesPedida.ToArray();

        GameManagerMultiplayer.Instance.ActualizarNegociacionRpc(oponentPlayerID, playerID, arrayOfrecido, arrayPedido, dineroOfrecido, dineroPedido);

        windowNegociacionRecibida.SetActive(false);
        listaPropiedadesOfrecida.Clear();
        listaPropiedadesPedida.Clear();

        foreach(int i in arrayOfrecido)
        {
            Casilla cas = TableManager.Instance.GetCasillaArray()[i];
            TableManager.Instance.ComprobarGrupoCompradoModoLocal(cas, playerID);
            
        }

        foreach(int i in arrayPedido)
        {
            Casilla cas = TableManager.Instance.GetCasillaArray()[i];
            TableManager.Instance.ComprobarGrupoCompradoModoLocal(cas, oponentPlayerID);
        }

        GameManagerMultiplayer.Instance.MandarConfirmacionNegociacionRpc(true, playerID, GameManagerMultiplayer.Instance.RpcTarget.Single((ulong)oponentPlayerID, RpcTargetUse.Temp));
        //MANDAR VENTANITA AL JUGADOR DICIENDOLE QUE LA OFERTA HA SIDO ACEPTADA, Y AQUI LUEGO MODIFICAREMOS LAS COSAS DE MANERA LOCAL AHI

    }

    public void RechazarOfertaNegociacion(int oponentPlayerID)
    {
        int playerID = GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID();
        GameManagerMultiplayer.Instance.ModificarValoresNegociacionRpc(oponentPlayerID, playerID, false);

        //MANDAR VENTANITA AL JUGADOR DICIENDOLE QUE LA OFERTA HA SIDO RECHAZADA
        GameManagerMultiplayer.Instance.MandarConfirmacionNegociacionRpc(false, playerID, GameManagerMultiplayer.Instance.RpcTarget.Single((ulong)oponentPlayerID, RpcTargetUse.Temp));

        windowNegociacionRecibida.SetActive(false);
        listaPropiedadesOfrecida.Clear();
        listaPropiedadesPedida.Clear();
    }


    public void ActivarPanelNegociacionRecibida()
    {
        windowNegociacionRecibida.SetActive(true);
    }

    public void ConfirmacionNegociacion(bool estado, int oponenteID)
    {
        if (estado)
        {
            ventanaNegociacionAceptada.SetActive(true);
            foreach (int i in listaPropiedadesOfrecer)
            {
                Casilla cas = TableManager.Instance.GetCasillaArray()[i];
                TableManager.Instance.ComprobarGrupoCompradoModoLocal(cas, oponenteID);

            }

            foreach (int i in listaPropiedadesPedir)
            {
                Casilla cas = TableManager.Instance.GetCasillaArray()[i];
                TableManager.Instance.ComprobarGrupoCompradoModoLocal(cas, GameManagerMultiplayer.Instance.ownerPlayer.GetPlayerID());
            }
        }
        else
        {
            ventanaNegociacionRechazada.SetActive(true);
        }

            LimpiarValoresNegociacionEnviada();

    }


    private void LimpiarValoresNegociacionEnviada()
    {
        listaPropiedadesOfrecer.Clear();
        listaPropiedadesPedir.Clear();
        dineroOfrecerInputField.text = string.Empty;
        dineroPedirOponenteIF.text = string.Empty;
        numeroAnteriorPulsado = -1;

        for (int i = contentScrollViewPropiedadesNegociacion.childCount - 1; i >= 0; i--)
        {
            Destroy(contentScrollViewPropiedadesNegociacion.GetChild(i).gameObject);
        }

        //FALTA TAROT
        for (int y = contentSVOponentePropiedades.childCount - 1; y >= 0; y--)
        {
            Destroy(contentSVOponentePropiedades.GetChild(y).gameObject);
        }

        //FALTA TAROT
    }
    public bool GetModoNegociacion()
    {
        return modoNegociacion;
    }

}