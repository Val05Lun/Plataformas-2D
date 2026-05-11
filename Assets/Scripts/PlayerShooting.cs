using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Configuración del Ataque")]
    public GameObject proyectilPrefab;
    public float tiempoEntreDisparos = 0.5f;
    
    private float tiempoUltimoDisparo;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Buscar el SpriteRenderer (puede estar en el mismo objeto o en un "hijo" visual)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Detectar si presionamos Z o P
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.P))
        {
            if (Time.time >= tiempoUltimoDisparo + tiempoEntreDisparos)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time;
            }
        }
    }

    void Disparar()
    {
        if (proyectilPrefab != null)
        {
            // Fabricar la cereza
            GameObject proyectil = Instantiate(proyectilPrefab, transform.position, Quaternion.identity);
            
            // Ajustar dirección según hacia dónde mira el jugador
            float direccion = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
            
            Proyectil scriptProyectil = proyectil.GetComponent<Proyectil>();
            if (scriptProyectil != null)
            {
                scriptProyectil.ConfigurarDireccion(direccion);
            }
            else
            {
                Debug.LogError("¡ATENCIÓN! La cereza no tiene el script 'Proyectil' agregado. ¡Añádeselo en Unity!");
            }

            // Heredar capa gráfica para que no se vuelva invisible
            SpriteRenderer proyectilSprite = proyectil.GetComponent<SpriteRenderer>();
            if (proyectilSprite != null && spriteRenderer != null)
            {
                proyectilSprite.sortingLayerName = spriteRenderer.sortingLayerName;
                proyectilSprite.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }
    }
}
