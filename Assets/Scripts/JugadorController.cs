using UnityEngine;

public class JugadorController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 7.5f;
    [SerializeField] private float fuerzaSalto = 14.5f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform detectorSuelo;
    [SerializeField] private float radioDeteccion = 0.2f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Referencias Visuales")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Daño e Invulnerabilidad")]
    [SerializeField] private float tiempoInvulnerable = 1.2f;
    private bool esInvulnerable = false;

    private Rigidbody2D rb;
    private float inputHorizontal;
    private bool enSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Entrada horizontal (A/D o Flechas)
        inputHorizontal = Input.GetAxisRaw("Horizontal");

        // 2. Voltear sprite según dirección
        if (inputHorizontal > 0)
            spriteRenderer.flipX = false;
        else if (inputHorizontal < 0)
            spriteRenderer.flipX = true;

        // 3. Comprobar si toca el suelo
        enSuelo = Physics2D.OverlapCircle(detectorSuelo.position, radioDeteccion, capaSuelo);

        // 4. Salto con Espacio, W o Flecha Arriba
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && enSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);

            // Reproducir sonido de salto
            if (AudioManager.instancia != null)
                AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxSalto);
        }

        // 5. Actualizar parámetros del Animator
        anim.SetFloat("Velocidad", Mathf.Abs(inputHorizontal));
        anim.SetBool("EnSuelo", enSuelo);
    }

    void FixedUpdate()
    {
        // Aplicar velocidad física constante en X
        rb.linearVelocity = new Vector2(inputHorizontal * velocidadMovimiento, rb.linearVelocity.y);
    }

    // 1. Detectar Monedas y Caída al Pozo (Triggers)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Moneda"))
        {
            if (GameManager.instancia != null)
                GameManager.instancia.SumarMoneda();

            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("ZonaMuerte"))
        {
            if (GameManager.instancia != null)
                GameManager.instancia.MuerteInstantanea();
        }
    }

    // 2. Detectar Enemigo (Aplastar por arriba vs Recibir daño lateral)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemigo"))
        {
            // Revisar si el berrendo viene cayendo desde arriba del enemigo
            if (rb.linearVelocity.y < -0.1f && transform.position.y > collision.transform.position.y + 0.3f)
            {
                EnemigoGrillo grillo = collision.gameObject.GetComponent<EnemigoGrillo>();
                if (grillo != null) grillo.Aplastar();

                // Pequeño rebote hacia arriba al aplastarlo
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto * 0.65f);
            }
            else if (!esInvulnerable)
            {
                if (GameManager.instancia != null)
                    GameManager.instancia.PerderVida();

                StartCoroutine(ActivarInvulnerabilidad());
            }
        }
    }

    private System.Collections.IEnumerator ActivarInvulnerabilidad()
    {
        esInvulnerable = true;
        // Parpadeo visual de daño
        for (int i = 0; i < 6; i++)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(tiempoInvulnerable / 6f);
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        esInvulnerable = false;
    }

    // Dibuja el círculo de detección de pies en el editor para ajustarlo visualmente
    private void OnDrawGizmosSelected()
    {
        if (detectorSuelo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectorSuelo.position, radioDeteccion);
        }
    }
}