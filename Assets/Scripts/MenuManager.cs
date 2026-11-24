using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Barra de Nombre")]
    [SerializeField] private TMP_InputField inputNombre;

    [Header("Paneles Emergentes")]
    [SerializeField] private GameObject panelReglas;
    [SerializeField] private GameObject panelCreditos;

    void Start()
    {
        // Asegura que los paneles empiecen cerrados
        if (panelReglas != null) panelReglas.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);

        // Si ya había un nombre guardado antes, lo vuelve a poner
        if (inputNombre != null && PlayerPrefs.HasKey("NombreJugador"))
        {
            inputNombre.text = PlayerPrefs.GetString("NombreJugador");
        }
    }

    public void IniciarJuego()
    {
        // Guarda el nombre escrito (si lo dejan vacío, pone "Berrendo" por defecto)
        string nombre = (inputNombre != null && !string.IsNullOrWhiteSpace(inputNombre.text))
            ? inputNombre.text.Trim()
            : "Berrendo";

        PlayerPrefs.SetString("NombreJugador", nombre);
        PlayerPrefs.Save();

        SceneManager.LoadScene("JuegoPrincipal");
    }

    public void AbrirReglas()
    {
        panelReglas.SetActive(true);
    }

    public void CerrarReglas()
    {
        panelReglas.SetActive(false);
    }

    public void AbrirCreditos()
    {
        panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        panelCreditos.SetActive(false);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando Super Berrendo Bros...");

        // Cierra la aplicación cuando ya está exportada (.exe / compilada)
        Application.Quit();

        // Detiene el modo Play si estás probando dentro del editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}