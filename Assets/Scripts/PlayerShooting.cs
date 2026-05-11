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
        // Buscar el SpriteRenderer para saber hacia dónde miramos y heredar la capa gráfica
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Detectar si el jugador presiona la tecla Z o P
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.P))
        {
            // Comprobar si ya pasó suficiente tiempo para volver a disparar
            if (Time.time >= tiempoUltimoDisparo + tiempoEntreDisparos)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time;
            }
        }
    }

    void Disparar()
    {
        if (proyectilPrefab == null) return;

        // Crear la cereza en la misma posición que el jugador
        GameObject proyectil = Instantiate(proyectilPrefab, transform.position, Quaternion.identity);

        // Ajustar la capa de dibujo para que se vea igual o por encima del jugador
        SpriteRenderer proyectilSprite = proyectil.GetComponentInChildren<SpriteRenderer>();
        if (proyectilSprite != null && spriteRenderer != null)
        {
            proyectilSprite.sortingLayerName = spriteRenderer.sortingLayerName;
            proyectilSprite.sortingOrder = spriteRenderer.sortingOrder + 1; // Un punto por encima
        }

        // Detectar la dirección (depende de si el sprite del jugador está volteado en X)
        float direccion = 1f;
        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            direccion = -1f;
        }

        // Pasarle la dirección al script del proyectil
        Proyectil scriptProyectil = proyectil.GetComponent<Proyectil>();
        if (scriptProyectil != null)
        {
            scriptProyectil.ConfigurarDireccion(direccion);
        }
    }
}
