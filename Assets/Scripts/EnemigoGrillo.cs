using UnityEngine;

public class EnemigoGrillo : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float distanciaPatrulla = 3f;
    [SerializeField] private bool empiezaHaciaLaIzquierda = true;
    [SerializeField] private bool spriteMiraALaIzquierda = true; // Desmarca si tu dibujo original mira a la derecha

    private Vector2 puntoInicial;
    private int direccion;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private bool estaMuerto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        puntoInicial = transform.position;
        direccion = empiezaHaciaLaIzquierda ? -1 : 1;
        ActualizarOrientacion();
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;

        // Mover al grillo horizontalmente manteniendo la gravedad
        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);

        // Cambiar de dirección al llegar a los límites de su patrulla
        if (direccion == -1 && transform.position.x <= puntoInicial.x - distanciaPatrulla)
        {
            direccion = 1;
            ActualizarOrientacion();
        }
        else if (direccion == 1 && transform.position.x >= puntoInicial.x + distanciaPatrulla)
        {
            direccion = -1;
            ActualizarOrientacion();
        }
    }

    void ActualizarOrientacion()
    {
        if (sr == null) return;

        if (spriteMiraALaIzquierda)
            sr.flipX = (direccion == 1);
        else
            sr.flipX = (direccion == -1);
    }

    public void Aplastar()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        //Reproducir sonido de aplastar enemigo
        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxAplastarEnemigo);

        // Detener física y animación antes de desaparecer
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = false;

        if (GameManager.instancia != null)
            GameManager.instancia.SumarPuntos(200);

        Destroy(gameObject, 0.1f);
    }
}