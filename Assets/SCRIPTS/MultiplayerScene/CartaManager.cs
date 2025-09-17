using System;
using UnityEngine;

public class CartaManager : MonoBehaviour
{
    [SerializeField] CartaCasilla prefabCarta;

    public static CartaManager Instance;
    public event Action<CartaCasilla> OnCartaCreada;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        UIManager.Instance.OnBotonSiClickado += UIManager_OnBotonSiClickado;
    }

    private void UIManager_OnBotonSiClickado()
    {
        CrearCartaCasilla(UIManager.Instance.GetCasillaComprar());
    }

    public void CrearCartaCasilla(Casilla casillaComprada)
    {
        CartaCasilla nuevaCarta = Instantiate(prefabCarta, UIManager.Instance.GetInventarioCasillas());
        nuevaCarta.SetCasillaCarta(casillaComprada);
        OnCartaCreada?.Invoke(nuevaCarta);
        
    }
}
