using UnityEngine;

/// <summary>
/// Item coleccionable que restaura corazones al jugador.
/// Se apoya en GameManager para respetar el máximo de 3 vidas y en
/// AudioManager para reproducir el efecto de sonido configurado.
/// </summary>
[DisallowMultipleComponent]
public class ItemVidaExtra : MonoBehaviour
{
    [Header("Configuración del Item")]
    [Tooltip("Cantidad de corazones que recupera el jugador. GameManager limita el total a 3 vidas.")]
    [SerializeField] private int cantidadVida = 1;

    [Tooltip("Si está activo, el objeto se destruye después de aplicarse la curación.")]
    [SerializeField] private bool destruirAlRecoger = true;

    [Header("Efectos opcionales")]
    [Tooltip("Prefab opcional de partículas o efecto visual que aparece al recoger el item.")]
    [SerializeField] private GameObject efectoRecogida;

    private bool recogido = false;
    private Collider2D colisionador;

    /// <summary>
    /// Guarda la referencia al collider y fuerza su configuración como Trigger
    /// para que el jugador pueda atravesarlo sin bloquearse.
    /// </summary>
    private void Awake()
    {
        colisionador = GetComponent<Collider2D>();

        if (colisionador == null)
        {
            Debug.LogError("ItemVidaExtra necesita un Collider2D en el mismo GameObject.", this);
            enabled = false;
            return;
        }

        colisionador.isTrigger = true;
        Debug.Log("ItemVidaExtra listo en '" + name + "'. Trigger activo.", this);
    }

    /// <summary>
    /// Detecta al jugador, solicita la curación a GameManager y, solo si la
    /// vida fue restaurada, reproduce el sonido, muestra el efecto y consume
    /// el item. Si el jugador ya tiene 3 corazones, el item permanece en la escena.
    /// </summary>
    /// <param name="collision">Collider del objeto que entra en el Trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (recogido || !collision.CompareTag("Player"))
            return;

        Debug.Log("ItemVidaExtra detecto al jugador '" + collision.name + "'.", this);

        if (GameManager.instancia == null)
        {
            Debug.LogWarning("ItemVidaExtra no encuentra GameManager.instancia.", this);
            return;
        }

        if (!GameManager.instancia.RecuperarVida(cantidadVida))
        {
            Debug.Log("ItemVidaExtra no se recoge: el jugador ya tiene 3 vidas.", this);
            return;
        }

        recogido = true;

        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxVidaExtra);

        if (efectoRecogida != null)
            Instantiate(efectoRecogida, transform.position, Quaternion.identity);

        if (destruirAlRecoger)
        {
            Destroy(gameObject);
        }
        else
        {
            colisionador.enabled = false;
        }
    }
}
