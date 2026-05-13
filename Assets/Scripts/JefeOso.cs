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
    public float fuerzaEmpujeAlJugador = 7f; // Fuerza con la que avienta al jugador al golpearlo
    private float temporizadorAtaque = 0f;

    [Header("Ajustes de Terreno (Pegar al Piso)")]
    public bool detectarSuelo = true; // Si es true, el oso se mantendrá pegado al nivel del suelo
    public float fuerzaGravedad = 9.8f;
    [Tooltip("Ajusta este valor en números negativos (ej. -0.5 o -1.2) si el oso se ve flotando muy arriba del piso")]
    public float ajusteAlturaSuelo = -0.5f;

    [Header("Sistema de Salud del Jefe")]
    public int vidasJefe = 15; // Golpes de cereza necesarios para derrotarlo
    public GameObject explosionPrefab; // Opcional, para cuando muere
    private int vidasIniciales;
    private bool enFase2 = false;
    private float tiempoEfectoDano = 0f;
    private Color colorOriginal = Color.white;

    [Header("Sistema de Vidas (Diamantes)")]
    [Tooltip("Arrastra aquí el objeto 'Vidas' del Nivel 5 que contiene los diamantes")]
    public Transform contenedorDeVidas;

    [Header("Eventos Extras (Opcional)")]
    public UnityEvent onAtacarJugador;

    private Transform jugador;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D _collider;
    private bool mirandoDerecha = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color;
        }

        // Forzar a modo Kinematic por código para ignorar la casilla "Static" en el Editor
        // y evitar bloqueos por fricción estática o batching.
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true; // Garantiza que detecte todas las colisiones y triggers
        }

        vidasIniciales = vidasJefe;

        BuscarJugador();

        // Buscar contenedor de vidas automáticamente si no está asignado
        BuscarContenedorVidas();
    }

    private void BuscarJugador()
    {
        if (jugador != null) return;

        // 1. Buscar por etiqueta estándar de Unity
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
            return;
        }

        // 2. Buscar por componente PlayerShooting
        PlayerShooting ps = Object.FindObjectOfType<PlayerShooting>();
        if (ps != null)
        {
            jugador = ps.transform;
            return;
        }

        // 3. Buscar por nombres comunes
        string[] nombresComunes = new string[] { "player", "jugador", "fox", "zorrito", "Player" };
        foreach (string nombre in nombresComunes)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null)
            {
                jugador = obj.transform;
                return;
            }
        }
    }

    private void BuscarContenedorVidas()
    {
        if (contenedorDeVidas != null) return;

        // Intentar encontrar el objeto Vidas en la escena activa
        GameObject vidasObj = GameObject.Find("Vidas");
        if (vidasObj != null)
        {
            contenedorDeVidas = vidasObj.transform;
        }
        else
        {
            // Respaldo buscando dentro de Nivel5
            GameObject nivel5 = GameObject.Find("Nivel5");
            if (nivel5 != null)
            {
                Transform v = nivel5.transform.Find("Vidas");
                if (v != null) contenedorDeVidas = v;
            }
        }
    }

    void Update()
    {
        // 1. Manejar el temporizador de ataque
        if (temporizadorAtaque > 0)
        {
            temporizadorAtaque -= Time.deltaTime;
        }

        // 2. Manejar el destello visual de daño
        if (tiempoEfectoDano > 0)
        {
            tiempoEfectoDano -= Time.deltaTime;
            if (tiempoEfectoDano <= 0 && spriteRenderer != null)
            {
                // Regresar al color base (o al color de Fase 2 si está enfurecido)
                spriteRenderer.color = enFase2 ? new Color(1f, 0.6f, 0.6f) : colorOriginal;
            }
        }

        // 3. Buscar al jugador continuamente si no se ha encontrado
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null) return; // Si sigue sin estar disponible, salimos
        }

        // 4. Comportamiento de Persecución y Adaptación al Suelo combinados
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        float pasoX = 0f;

        if (distanciaAlJugador <= rangoDeVisión)
        {
            float direccion = jugador.position.x - transform.position.x;
            
            // Moverse solo si no está perfectamente alineado (evita temblor en el mismo sitio)
            if (Mathf.Abs(direccion) > 0.15f)
            {
                pasoX = Mathf.Sign(direccion) * velocidadMoviemento * Time.deltaTime;

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
        }

        // Calcular altura en Y de forma robusta con Raycast
        float targetY = transform.position.y;
        if (detectarSuelo && _collider != null)
        {
            Vector2 origenRayo = _collider.bounds.center;
            RaycastHit2D hit = LanzaRayoSeguro(origenRayo, Vector2.down, _collider.bounds.size.y + 5f);
            
            if (hit.collider != null)
            {
                float diferenciaY = hit.point.y - _collider.bounds.min.y;
                if (Mathf.Abs(diferenciaY) <= 3f + Mathf.Abs(ajusteAlturaSuelo))
                {
                    targetY = transform.position.y + diferenciaY + ajusteAlturaSuelo;
                }
                else if (diferenciaY < -3f)
                {
                    targetY -= fuerzaGravedad * Time.deltaTime;
                }
            }
            else
            {
                targetY -= fuerzaGravedad * Time.deltaTime;
            }
        }

        // Aplicar la posición final combinada en una sola asignación
        // Esto evita conflictos entre el Lerp en Y y la traslación en X en objetos marcados como Static
        float nuevaPosY = Mathf.Lerp(transform.position.y, targetY, 15f * Time.deltaTime);
        transform.position = new Vector3(transform.position.x + pasoX, nuevaPosY, transform.position.z);
    }

    private RaycastHit2D LanzaRayoSeguro(Vector2 origen, Vector2 direccion, float distancia)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, direccion, distancia);

        foreach (var hit in hits)
        {
            if (_collider != null && hit.collider == _collider) continue; // Ignorarnos a nosotros mismos sin apagar el collider
            if (hit.collider.isTrigger) continue; // Ignora siempre los triggers
            
            string nombreHit = hit.collider.gameObject.name.ToLower();
            if (nombreHit.Contains("player") || nombreHit.Contains("proyectil") || nombreHit.Contains("cherry") || nombreHit.Contains("enemy")) continue;
            
            return hit; // Encontramos el piso sólido
        }
        return new RaycastHit2D();
    }

    private void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    // Método para recibir daño de las cerezas
    public void RecibirDanoJefe(int cantidad, Vector3 posicionImpacto)
    {
        vidasJefe -= cantidad;
        Debug.Log("¡Jefe Oso recibió daño! Vidas restantes: " + vidasJefe);

        // Efecto visual de daño (parpadeo rojo)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            tiempoEfectoDano = 0.1f;
        }

        // Verificar si entra en Fase 2 (Enfurecido) al bajar del 50% de vida
        if (!enFase2 && vidasJefe <= vidasIniciales / 2)
        {
            ActivarFase2();
        }

        // Si su vida llega a cero, muere
        if (vidasJefe <= 0)
        {
            Morir(posicionImpacto);
        }
    }

    private void ActivarFase2()
    {
        enFase2 = true;
        velocidadMoviemento *= 1.35f; // Se mueve un 35% más rápido
        tiempoEntreAtaques *= 0.75f; // Ataca más seguido
        Debug.Log("¡EL JEFE OSO HA ENTRADO EN FASE 2: ENFURECIDO!");

        if (spriteRenderer != null)
        {
            // Tono rojizo permanente para denotar furia
            spriteRenderer.color = new Color(1f, 0.6f, 0.6f);
        }
    }

    private void Morir(Vector3 posicionImpacto)
    {
        Debug.Log("¡EL JEFE OSO HA SIDO DERROTADO!");

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, posicionImpacto, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }
        else
        {
            // Intentar buscar el prefab de explosión de otros enemigos si no se asignó
            EnemyHealth saludCualquiera = Object.FindObjectOfType<EnemyHealth>();
            if (saludCualquiera != null && saludCualquiera.explosionPrefab != null)
            {
                GameObject exp = Instantiate(saludCualquiera.explosionPrefab, transform.position, Quaternion.identity);
                Destroy(exp, 1.5f);
            }
        }

        Destroy(gameObject);
    }

    // Colisiones con el jugador
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarAtacar(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IntentarAtacar(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IntentarAtacar(collider.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        IntentarAtacar(collider.gameObject);
    }

    private void IntentarAtacar(GameObject objColision)
    {
        string nombreColision = objColision.name.ToLower();
        if (nombreColision.Contains("player") || nombreColision.Contains("jugador") || objColision.CompareTag("Player"))
        {
            if (temporizadorAtaque <= 0f)
            {
                Atacar(objColision);
            }
        }
    }

    private void Atacar(GameObject objetivo)
    {
        Debug.Log("¡El Jefe Oso ha golpeado al jugador!");
        
        // Aplicar empuje (Knockback) al chocar contra el jefe final
        Rigidbody2D rbJugador = objetivo.GetComponent<Rigidbody2D>();
        if (rbJugador != null)
        {
            float direccionEmpuje = objetivo.transform.position.x > transform.position.x ? 1f : -1f;
            rbJugador.velocity = new Vector2(direccionEmpuje * fuerzaEmpujeAlJugador, fuerzaEmpujeAlJugador * 0.8f);
        }

        BuscarContenedorVidas();

        // Lógica de restar vidas/diamantes
        if (contenedorDeVidas != null)
        {
            int diamantesRestantes = contenedorDeVidas.childCount;
            if (diamantesRestantes > 0)
            {
                Destroy(contenedorDeVidas.GetChild(diamantesRestantes - 1).gameObject);
                Debug.Log("¡Se perdió un diamante contra el Jefe Oso! Quedan: " + (diamantesRestantes - 1));

                if (diamantesRestantes == 1)
                {
                    Debug.Log("¡Cero vidas! Reiniciando el nivel...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            Debug.LogWarning("Jefe Oso: El contenedor de vidas no está asignado ni encontrado.");
        }

        if (onAtacarJugador != null)
        {
            onAtacarJugador.Invoke();
        }

        temporizadorAtaque = tiempoEntreAtaques;
    }
}
