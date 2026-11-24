using System.Collections;
using UnityEngine;

public class MetaNivel : MonoBehaviour
{
    [Header("Opcional: Punto de entrada del salón")]
    [SerializeField] private Transform puertaSalon; // Arrastra un objeto vacío puesto en la puerta del salón
    [SerializeField] private float velocidadCaminarMeta = 3f;

    private bool metaAlcanzada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (metaAlcanzada) return;

        if (collision.CompareTag("Player"))
        {
            metaAlcanzada = true;
            StartCoroutine(SecuenciaVictoria(collision.gameObject));
        }
    }

    IEnumerator SecuenciaVictoria(GameObject jugador)
    {
        // Desactivar control manual del jugador
        JugadorController control = jugador.GetComponent<JugadorController>();
        if (control != null) control.enabled = false;

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator anim = jugador.GetComponentInChildren<Animator>();
        SpriteRenderer sr = jugador.GetComponentInChildren<SpriteRenderer>();

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Si asignaste la puerta del salón, el berrendo camina solo hasta ella
        if (puertaSalon != null)
        {
            if (sr != null) sr.flipX = false;
            if (anim != null) anim.SetFloat("Velocidad", 1f);

            while (Mathf.Abs(jugador.transform.position.x - puertaSalon.position.x) > 0.15f)
            {
                Vector3 destino = new Vector3(puertaSalon.position.x, jugador.transform.position.y, jugador.transform.position.z);
                jugador.transform.position = Vector3.MoveTowards(jugador.transform.position, destino, velocidadCaminarMeta * Time.deltaTime);
                yield return null;
            }

            // Ocultar al berrendo como si hubiera entrado al salón
            if (sr != null) sr.enabled = false;
        }

        if (anim != null) anim.SetFloat("Velocidad", 0f);

        // Mostrar el panel de victoria
        if (GameManager.instancia != null)
            GameManager.instancia.GanarNivel();
    }
}