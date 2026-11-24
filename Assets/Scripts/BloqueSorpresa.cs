using System.Collections;
using UnityEngine;

public class BloqueSorpresa : MonoBehaviour
{
    [Header("Sprites del Bloque")]
    [SerializeField] private Sprite spriteVacio; // El bloque café/usado sin signo '?'

    [Header("Ajustes de Golpe")]
    [SerializeField] private float alturaRebote = 0.25f;
    [SerializeField] private float velocidadRebote = 8f;
    [SerializeField] private GameObject efectoMonedaVisual; // Opcional: prefab visual de moneda

    private bool yaUsado = false;
    private Vector3 posicionOriginal;
    private SpriteRenderer sr;

    void Start()
    {
        posicionOriginal = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si ya se usó o no lo golpeó el jugador, ignorar
        if (yaUsado || !collision.gameObject.CompareTag("Player")) return;

        // Revisar la dirección del contacto: si la normal apunta hacia arriba (> 0.5),
        // significa que el jugador golpeó la cara INFERIOR del bloque con la cabeza
        foreach (ContactPoint2D punto in collision.contacts)
        {
            if (punto.normal.y > 0.5f)
            {
                ActivarBloque();
                break;
            }
        }
    }

    void ActivarBloque()
    {
        yaUsado = true;

        // Reproducir sonido de golpe de bloque
        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxGolpeBloque);

        // Cambiar al sprite de bloque vacío
        if (sr != null && spriteVacio != null)
            sr.sprite = spriteVacio;

        // Sumar moneda y puntos al HUD
        if (GameManager.instancia != null)
            GameManager.instancia.SumarMoneda();

        // Si asignaste un prefab visual de moneda saliendo, lo muestra brevemente
        if (efectoMonedaVisual != null)
        {
            GameObject moneda = Instantiate(efectoMonedaVisual, transform.position + Vector3.up * 0.8f, Quaternion.identity);
            Destroy(moneda, 0.4f);
        }

        // Hacer el pequeño brinco del bloque al ser golpeado
        StartCoroutine(AnimarGolpe());
    }

    IEnumerator AnimarGolpe()
    {
        Vector3 destinoArriba = posicionOriginal + Vector3.up * alturaRebote;

        // Subir
        while (Vector3.Distance(transform.position, destinoArriba) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destinoArriba, velocidadRebote * Time.deltaTime);
            yield return null;
        }

        // Regresar a su sitio
        while (Vector3.Distance(transform.position, posicionOriginal) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionOriginal, velocidadRebote * Time.deltaTime);
            yield return null;
        }

        transform.position = posicionOriginal;
    }
}