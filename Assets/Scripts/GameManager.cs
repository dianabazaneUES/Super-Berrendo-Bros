using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    public const int VidasMaximas = 3;

    [Header("Estadísticas del Jugador")]
    public int vidas = 3;
    public int monedas = 0;
    public int puntos = 0;
    public float tiempoRestante = 400f;

    [Header("Referencias del HUD")]
    [SerializeField] private TMP_Text txtNombre;
    [SerializeField] private TMP_Text txtMonedas;
    [SerializeField] private TMP_Text txtPuntos;
    [SerializeField] private TMP_Text txtTiempo;
    [SerializeField] private Image[] corazones; // Arrastra Corazon_1, 2 y 3 aquí

    [Header("Pantalla de Victoria")]
    [SerializeField] private GameObject panelVictoria;
    [Header("Configuracion de niveles")]
    [SerializeField] private string nombreSiguienteNivel = "JuegoNivel2";
    [SerializeField] private TMP_Text txtPuntajeFinal;

    [Header("Pantalla de Pausa")]
    [SerializeField] private GameObject panelPausa;
    private bool estaPausado = false;

    private bool juegoTerminado = false;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        // Cargar el nombre guardado desde el Menú Principal
        if (txtNombre != null)
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "BERRENDO");
            txtNombre.text = nombre.ToUpper();
        }

        ActualizarHUD();
    }

    void Update()
    {
        if (juegoTerminado) return;

        //Activar o desactivar pausa con la techa ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado)
                ReanudarJuego();
            else
                PausarJuego();
        }

        if (estaPausado) return;

        // Descontar tiempo segundo a segundo
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (txtTiempo != null)
                txtTiempo.text = "TIEMPO: " + Mathf.CeilToInt(tiempoRestante).ToString("000");
        }
        else
        {
            tiempoRestante = 0;
            IrAGameOver();
        }
    }

    public void SumarMoneda()
    {
        monedas++;
        puntos += 100;
        ActualizarHUD();

        // Reproducir sonido de moneda
        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxMoneda);
    }

    public void SumarPuntos(int cantidad)
    {
        puntos += cantidad;
        ActualizarHUD();
    }

    /// <summary>
    /// Recupera corazones sin superar el máximo de 3 vidas.
    /// Devuelve true solo si se aplicó la curación.
    /// </summary>
    /// <param name="cantidad">Corazones a recuperar. Debe ser mayor que cero.</param>
    public bool RecuperarVida(int cantidad = 1)
    {
        if (juegoTerminado || cantidad <= 0 || vidas >= VidasMaximas)
            return false;

        vidas = Mathf.Min(VidasMaximas, vidas + cantidad);
        ActualizarHUD();
        return true;
    }

    public void PerderVida()
    {
        if (juegoTerminado) return;

        vidas--;
        ActualizarHUD();

        // Reproducir sonido de daño
        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxDanio);

        if (vidas <= 0)
        {
            IrAGameOver();
        }
    }

    public void MuerteInstantanea()
    {
        if (juegoTerminado) return;
        vidas = 0;
        ActualizarHUD();
        IrAGameOver();
    }

    void ActualizarHUD()
    {
        if (txtMonedas != null) txtMonedas.text = "MONEDAS: " + monedas.ToString("00");
        if (txtPuntos != null) txtPuntos.text = "PUNTOS: " + puntos.ToString("0000");

        // Encender o apagar los corazones según las vidas restantes
        for (int i = 0; i < corazones.Length; i++)
        {
            if (corazones[i] != null)
                corazones[i].enabled = (i < vidas);
        }
    }

    void IrAGameOver()
    {
        juegoTerminado = true;
        SceneManager.LoadScene("GameOver");
    }

    public void GanarNivel()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        // Reproducir sonido de victoria
        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxVictoria);

        // Bono por el tiempo que sobró (10 puntos por cada segundo restante)
        int bonoTiempo = Mathf.CeilToInt(tiempoRestante) * 10;
        puntos += bonoTiempo;
        ActualizarHUD();

        if (txtPuntajeFinal != null)
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "BERRENDO").ToUpper();
            txtPuntajeFinal.text = "¡FELICIDADES " + nombre + "!\nPUNTUACIÓN FINAL: " + puntos.ToString("0000");
        }

        if (panelVictoria != null)
            panelVictoria.SetActive(true);
    }

    public void RegresarAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void PausarJuego()
    {
        if (juegoTerminado) return;
        estaPausado = true;
        Time.timeScale = 0f; // Congela físicas, enemigos y animaciones
        if (panelPausa != null) panelPausa.SetActive(true);
    }

    public void ReanudarJuego()
    {
        estaPausado = false;
        Time.timeScale = 1f; // Devuelve el tiempo a la normalidad
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // Muy importante restaurar el tiempo antes de recargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CargarSiguienteNivel()
    {
        Time.timeScale = 1f; //Aseguramos que el timepo corra normal al cambiar el nivel
        if (!string.IsNullOrEmpty(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }

    }
}