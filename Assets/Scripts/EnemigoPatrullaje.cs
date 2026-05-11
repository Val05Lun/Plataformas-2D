using UnityEngine;

public class EnemigoPatrullaje : MonoBehaviour
{
    [Header("Configuración de Patrullaje")]
    public float velocidad = 2f;
    public float distanciaMovimiento = 3f; // Cuánta distancia recorre antes de voltear
    
    private Vector3 posicionInicial;
    private bool moviendoDerecha = true;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Guardamos la posición en la que el enemigo fue colocado inicialmente en el mapa
        posicionInicial = transform.position;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Calcular hacia dónde debemos movernos
        if (moviendoDerecha)
        {
            // Mover hacia la derecha
            transform.Translate(Vector2.right * velocidad * Time.deltaTime);
            
            // Voltear el dibujo (la mayoría de sprites ven a la izquierda por defecto)
            if (spriteRenderer != null) spriteRenderer.flipX = true; 
            
            // Si superamos la distancia máxima a la derecha, voltear
            if (transform.position.x >= posicionInicial.x + distanciaMovimiento)
            {
                moviendoDerecha = false;
            }
        }
        else
        {
            // Mover hacia la izquierda
            transform.Translate(Vector2.left * velocidad * Time.deltaTime);
            
            // Restaurar el dibujo a su lado original
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            
            // Si superamos la distancia máxima a la izquierda, voltear
            if (transform.position.x <= posicionInicial.x - distanciaMovimiento)
            {
                moviendoDerecha = true;
            }
        }
    }
}
