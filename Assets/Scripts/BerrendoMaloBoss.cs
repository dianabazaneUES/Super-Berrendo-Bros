using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BerrendoMaloBoss : MonoBehaviour
{
    [Header("Referencias Principales")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Slider barraVidaJefe;

    [Header("Estadísticas del Villano")]
    [SerializeField] private int vidasMaximas = 5;
    private int vidasActuales;
    [SerializeField] private int puntosAlDerrotar = 2500;

    [Header("Persecución y Combate")]
    [SerializeField] private float velocidadNormal = 5.5f;
    [SerializeField] private float velocidadFuria = 7.5f;
    [SerializeField] private float distanciaGolpe = 1.5f;
    [SerializeField] private float tiempoEntreGolpes = 1.1f;
    [SerializeField] private float fuerzaSalto = 14.5f;
    [SerializeField] private float intervaloSalto = 2.4f;

    [Header("Detección de Suelo")]
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private float radioSuelo = 0.4f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private Collider2D miCollider;
    private Collider2D colJugador;

    private int direccion = -1;
    private bool enSuelo;
    private bool estaAtacando = false;
    private bool esInvulnerable = false;
    private bool estaDerrotado = false;
    private float siguienteGolpePermitido = 0f;
    private float siguienteDanioJugador = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        miCollider = GetComponent<Collider2D>();

        BuscarJugador();

        vidasActuales = vidasMaximas;
        if (barraVidaJefe != null)
        {
            barraVidaJefe.maxValue = vidasMaximas;
            barraVidaJefe.value = vidasActuales;
        }

        StartCoroutine(RutinaSaltos());
    }

    private void BuscarJugador()
    {
        if (jugador == null)
        {
            JugadorController ctrl = FindFirstObjectByType<JugadorController>();
            if (ctrl != null)
            {
                jugador = ctrl.transform;
            }
            else
            {
                GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
                if (objJugador != null && objJugador != gameObject)
                    jugador = objJugador.transform;
            }
        }

        if (jugador != null && colJugador == null)
        {
            colJugador = jugador.GetComponentInChildren<Collider2D>();
        }
    }

    private Vector2 ObtenerPosicionRealJugador()
    {
        if (colJugador != null)
            return colJugador.bounds.center;
        return jugador.position;
    }

    void Update()
    {
        if (estaDerrotado) return;

        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null) return;
        }

        enSuelo = Physics2D.OverlapCircle(transform.position, radioSuelo, capaSuelo);
        if (anim != null)
            anim.SetBool("enSuelo", enSuelo);

        Vector2 posJugador = ObtenerPosicionRealJugador();
        float centroJefeY = (miCollider != null) ? miCollider.bounds.center.y : (transform.position.y + 1.0f);
        float diferenciaX = posJugador.x - transform.position.x;
        float distanciaX = Mathf.Abs(diferenciaX);
        float distanciaY = Mathf.Abs(posJugador.y - centroJefeY);

        if (!estaAtacando && distanciaX > 0.1f)
        {
            direccion = (diferenciaX < 0f) ? -1 : 1;
            sr.flipX = (direccion == -1);
        }

        if (!estaAtacando && distanciaX <= distanciaGolpe && distanciaY <= 1.8f && Time.time >= siguienteGolpePermitido)
        {
            StartCoroutine(EjecutarGolpe());
        }
    }

    void FixedUpdate()
    {
        if (estaDerrotado || jugador == null) return;

        if (estaAtacando)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("estaCaminando", false);
            return;
        }

        Vector2 posJugador = ObtenerPosicionRealJugador();
        float distanciaX = Mathf.Abs(posJugador.x - transform.position.x);

        if (distanciaX > distanciaGolpe * 0.75f)
        {
            float vel = (vidasActuales <= 2) ? velocidadFuria : velocidadNormal;
            rb.linearVelocity = new Vector2(direccion * vel, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("estaCaminando", true);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("estaCaminando", false);
        }
    }

    IEnumerator RutinaSaltos()
    {
        while (!estaDerrotado)
        {
            yield return new WaitForSeconds(intervaloSalto);
            if (!estaDerrotado && jugador != null && enSuelo && !estaAtacando)
            {
                Vector2 posJugador = ObtenerPosicionRealJugador();
                float centroJefeY = (miCollider != null) ? miCollider.bounds.center.y : (transform.position.y + 1.0f);
                // Si el jugador está subido en una plataforma, salta más alto para alcanzarlo
                float impulso = (posJugador.y > centroJefeY + 1.4f) ? fuerzaSalto * 1.25f : fuerzaSalto;
                rb.AddForce(Vector2.up * impulso, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator EjecutarGolpe()
    {
        estaAtacando = true;
        siguienteGolpePermitido = Time.time + tiempoEntreGolpes;

        if (anim != null)
            anim.SetTrigger("Golpear");

        yield return new WaitForSeconds(0.22f);

        if (!estaDerrotado && jugador != null)
        {
            Vector2 posJugador = ObtenerPosicionRealJugador();
            float centroJefeY = (miCollider != null) ? miCollider.bounds.center.y : (transform.position.y + 1.0f);
            float distX = Mathf.Abs(posJugador.x - transform.position.x);
            float distY = Mathf.Abs(posJugador.y - centroJefeY);

            if (distX <= distanciaGolpe + 0.7f && distY <= 2.0f)
            {
                AplicarDanioAJugador(jugador.gameObject);
            }
        }

        yield return new WaitForSeconds(0.32f);
        estaAtacando = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluarChoqueConJugador(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        EvaluarChoqueConJugador(collision);
    }

    private void EvaluarChoqueConJugador(Collision2D collision)
    {
        if (estaDerrotado) return;

        bool esJugador = (jugador != null && collision.transform.root == jugador.root) ||
                         collision.gameObject.CompareTag("Player") ||
                         collision.gameObject.GetComponentInParent<JugadorController>() != null;

        if (!esJugador) return;

        Rigidbody2D rbJugador = collision.gameObject.GetComponentInParent<Rigidbody2D>();

        // Comparar la planta de los pies del jugador contra el centro del cuerpo del jefe
        float piesJugadorY = (colJugador != null) ? colJugador.bounds.min.y : collision.collider.bounds.min.y;
        float centroJefeY = (miCollider != null) ? miCollider.bounds.center.y : (transform.position.y + 1.0f);

        bool cayendoSobreCabeza = (rbJugador != null && rbJugador.linearVelocity.y < -0.1f) &&
                                   (piesJugadorY >= centroJefeY) &&
                                   (collision.contactCount > 0 && collision.GetContact(0).normal.y < -0.35f);

        if (cayendoSobreCabeza)
        {
            siguienteDanioJugador = Time.time + 0.65f;
            rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, 12.5f);
            RecibirDanio();
        }
        else
        {
            AplicarDanioAJugador(jugador != null ? jugador.gameObject : collision.gameObject);
        }
    }

    private void AplicarDanioAJugador(GameObject objJugador)
    {
        if (Time.time < siguienteDanioJugador) return;
        siguienteDanioJugador = Time.time + 1.2f;

        JugadorController ctrl = objJugador.GetComponentInParent<JugadorController>();
        if (ctrl != null)
        {
            string[] posiblesNombres = { "RecibirDanio", "TomarDanio", "HacerDanio", "RecibirGolpe" };
            foreach (string nombre in posiblesNombres)
            {
                MethodInfo m = ctrl.GetType().GetMethod(nombre, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null)
                {
                    var parametros = m.GetParameters();
                    if (parametros.Length == 0)
                    {
                        m.Invoke(ctrl, null);
                        return;
                    }
                    if (parametros.Length == 1 && parametros[0].ParameterType == typeof(int))
                    {
                        m.Invoke(ctrl, new object[] { 1 });
                        return;
                    }
                }
            }
        }

        if (GameManager.instancia != null)
            GameManager.instancia.PerderVida();

        if (AudioManager.instancia != null && AudioManager.instancia.sfxDanio != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxDanio);

        Rigidbody2D rbJugador = objJugador.GetComponentInParent<Rigidbody2D>();
        if (rbJugador != null)
        {
            float empujeX = (ObtenerPosicionRealJugador().x < transform.position.x) ? -6.5f : 6.5f;
            rbJugador.linearVelocity = new Vector2(empujeX, 6.5f);
        }

        SpriteRenderer srJugador = objJugador.GetComponentInChildren<SpriteRenderer>();
        if (srJugador != null)
            StartCoroutine(ParpadearJugador(srJugador));
    }

    IEnumerator ParpadearJugador(SpriteRenderer srJugador)
    {
        for (int i = 0; i < 6; i++)
        {
            if (srJugador == null) yield break;
            srJugador.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            if (srJugador == null) yield break;
            srJugador.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RecibirDanio()
    {
        if (esInvulnerable || estaDerrotado) return;

        vidasActuales--;

        if (barraVidaJefe != null)
            barraVidaJefe.value = vidasActuales;

        if (AudioManager.instancia != null)
            AudioManager.instancia.ReproducirEfecto(AudioManager.instancia.sfxAplastarEnemigo);

        if (vidasActuales <= 0)
        {
            DerrotarVillano();
        }
        else
        {
            StartCoroutine(ParpadeoInvulnerable());
        }
    }

    IEnumerator ParpadeoInvulnerable()
    {
        esInvulnerable = true;
        for (int i = 0; i < 6; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = (vidasActuales <= 2) ? new Color(1f, 0.55f, 0.55f) : Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        esInvulnerable = false;
    }

    private void DerrotarVillano()
    {
        estaDerrotado = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = false;

        if (barraVidaJefe != null)
            barraVidaJefe.gameObject.SetActive(false);

        if (GameManager.instancia != null)
        {
            GameManager.instancia.SumarPuntos(puntosAlDerrotar);
            GameManager.instancia.GanarNivel();
        }

        Destroy(gameObject, 0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioSuelo);
    }
}