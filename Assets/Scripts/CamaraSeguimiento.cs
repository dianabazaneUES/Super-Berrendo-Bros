using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float limiteIzquierdoMinimo = 0f;
    [SerializeField] private float limiteDerechoMaximo = 100f;
    private float posicionXMaxima;

    void Start()
    {
        posicionXMaxima = transform.position.x;
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Solo avanza hacia la derecha como en Mario clásico
        if (objetivo.position.x > posicionXMaxima)
        {
            posicionXMaxima = objetivo.position.x;
        }

        float nuevaX = Mathf.Clamp(posicionXMaxima, limiteIzquierdoMinimo, limiteDerechoMaximo);
        transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);
    }
}