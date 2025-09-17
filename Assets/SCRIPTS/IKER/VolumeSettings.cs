using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] private AudioMixer AudioMixer;
    public Slider Musica_SLIDER;
    public Slider Efectos_SLIDER;


    // Start is called before the first frame update
    void Start()
    {
         if (PlayerPrefs.HasKey("Musica")) { Cargar_Valor_Musica(); }
        else { VOL_MUSICA(); }

        if (PlayerPrefs.HasKey("Efectos")) { Cargar_Valor_Efectos(); }
        else { VOL_EFECTOS(); }
    }

    private void Awake()
    {
                DontDestroyOnLoad(gameObject);

    }

    public void VOL_MUSICA()
    {
        float volume = Musica_SLIDER.value;
        AudioMixer.SetFloat("Musica", Mathf.Log10(volume) * 20); //ajuste Slider
        PlayerPrefs.SetFloat("Musica", volume); 
    }
    public void VOL_EFECTOS()
    {
        float volume = Efectos_SLIDER.value;
        AudioMixer.SetFloat("Efectos", Mathf.Log10(volume) * 20); //ajuste Slider
        PlayerPrefs.SetFloat("Efectos", volume);
    }

     public void Cargar_Valor_Musica() 
    {
        Musica_SLIDER.value = PlayerPrefs.GetFloat("Musica");
        VOL_MUSICA();


    }
    public void Cargar_Valor_Efectos()
    {
       Efectos_SLIDER.value = PlayerPrefs.GetFloat("Efectos");
        VOL_EFECTOS();


    }


}
