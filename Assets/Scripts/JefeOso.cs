using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class JefeOso : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    public float velocidadMoviemento = 3f;
    public float rangoDeVisión = 15f; // Distancia a la que el oso "ve" al jugador
    
    [Header("Configuración de Ataque")]
    public float tiempoEntreAtaques = 2f; // Segundos que tarda en volver a pegar
    private float temporizadorAtaque = 0f;

    [Header("Sistema de Vidas (Diamantes)")]
    [Tooltip("Arrastra aquí el objeto 'Vidas' del Nivel 5 que contiene los diamantes")]
    public Transform contenedorDeVidas;

    [Header("Eventos Extras (Opcional)")]
    public UnityEvent onAtacarJugador;

    private Transform jugador;
    private Rigidbody2D rb;
    private bool mirandoDerecha = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Buscamos al jugador por su componente único para que no falle sin importar cómo se llame
        PlayerShooting ps = Object.FindObjectOfType<PlayerShooting>();
        if (ps != null)
        {
            jugador = ps.transform;
        }
        else
        {
            Debug.LogWarning("Jefe Oso: ¡No pude encontrar al jugador! Asegúrate de que el zorrito tenga el script 'PlayerShooting'.");
        }

        // Si no asignaste las vidas manualmente, intentaremos encontrar la carpeta del Nivel 5 automáticamente
        if (contenedorDeVidas == null)
        {
            GameObject nivel5 = GameObject.Find("Nivel5");
            if (nivel5 != null)
            {
                Transform vidasObj = nivel5.transform.Find("Vidas");
                if (vidasObj != null) contenedorDeVidas = vidasObj;
            }
        }
    }

    void Update()
    {
        // 1. Manejar el tiempo de recarga (Cooldown) del ataque
        if (temporizadorAtaque > 0)
        {
            temporizadorAtaque -= Time.deltaTime;
        }

        // 2. Si no hay jugador, no hacemos nada
        if (jugador == null) return;

        // 3. Comportamiento de Persecución
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciaAlJugador <= rangoDeVisión)
        {
            PerseguirJugador();
        }
        else
        {
            // Si el jugador está muy lejos, se queda quieto
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void PerseguirJugador()
    {
        // Calcular dirección hacia el jugador en el eje X
        float direccion = jugador.position.x - transform.position.x;
        
        // Moverse usando físicas
        rb.velocity = new Vector2(Mathf.Sign(direccion) * velocidadMoviemento, rb.velocity.y);

        // Voltear el sprite si es necesario
        if (direccion > 0 && !mirandoDerecha)
        {
            Voltear();
        }
        else if (direccion < 0 && mirandoDerecha)
        {
            Voltear();
        }
    }

    private void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    // 4. Lógica de hacer daño al chocar físicamente con el jugador
    private void OnCollisionStay2D(Collision2D collision)
    {
        IntentarAtacar(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        IntentarAtacar(collider.gameObject);
    }

    private void IntentarAtacar(GameObject objColision)
    {
        string nombreColision = objColision.name.ToLower();
        if (nombreColision.Contains("player") || nombreColision.Contains("jugador"))
        {
            // Si tocamos al jugador y nuestro ataque ya recargó
            if (temporizadorAtaque <= 0f)
            {
                Atacar(objColision);
            }
        }
    }

    private void Atacar(GameObject objetivo)
    {
        Debug.Log("¡El Oso ha golpeado al jugador!");
        
        // Lógica de los Diamantes/Vidas
        if (contenedorDeVidas != null)
        {
            int diamantesRestantes = contenedorDeVidas.childCount;
            if (diamantesRestantes > 0)
            {
                // Destruimos el último diamante de la lista
                Destroy(contenedorDeVidas.GetChild(diamantesRestantes - 1).gameObject);
                Debug.Log("¡Se perdió un diamante! Quedan: " + (diamantesRestantes - 1));

                // Si ese era el último diamante, reiniciamos el nivel
                if (diamantesRestantes == 1)
                {
                    Debug.Log("¡Cero vidas! Reiniciando el nivel...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                // Si ya no había diamantes por alguna razón, igual reiniciamos
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            Debug.LogWarning("Jefe Oso: El contenedor de vidas no está asignado ni encontrado.");
        }

        // Disparamos el evento extra por si acaso
        onAtacarJugador.Invoke();

        // Reiniciamos el temporizador para que no lo mate en 1 segundo
        temporizadorAtaque = tiempoEntreAtaques;
    }
}
