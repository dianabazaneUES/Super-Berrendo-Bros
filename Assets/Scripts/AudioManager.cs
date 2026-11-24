using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instancia;

    [Header("Reproductor de Efectos")]
    [SerializeField] private AudioSource fuenteEfectos;

    [Header("Clips de Efectos de Sonido (SFX)")]
    public AudioClip sfxSalto;
    public AudioClip sfxMoneda;
    public AudioClip sfxGolpeBloque;
    public AudioClip sfxAplastarEnemigo;
    public AudioClip sfxDanio;
    [Tooltip("Sonido que se reproduce al recoger un ItemVidaExtra.")]
    public AudioClip sfxVidaExtra;
    public AudioClip sfxVictoria;

    void Awake()
    {
        instancia = this;
    }

    public void ReproducirEfecto(AudioClip clip)
    {
        if (fuenteEfectos != null && clip != null)
        {
            fuenteEfectos.PlayOneShot(clip);
        }
    }
}