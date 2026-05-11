using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 10f;
    public float tiempoDeVida = 2.5f; // Se destruye tras 2.5s para no llenar la memoria
    
    private float direccionX = 1f;
    private Rigidbody2D rb;
    private bool haChocado = false; // Evita hacer daño doble en un mismo choque

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Evitar que le afecte la gravedad al proyectil
            rb.gravityScale = 0f;
        }
        
        Destroy(gameObject, tiempoDeVida); // Autodestrucción de seguridad
    }

    public void ConfigurarDireccion(float dirX)
    {
        direccionX = dirX;
    }

    void Update()
    {
        // Mover el proyectil hacia la dirección que le dio el jugador
        transform.Translate(new Vector3(direccionX, 0, 0) * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D colision)
    {
        if (haChocado) return;

        string nombreChoque = colision.name.ToLower();
        string nombrePadre = colision.transform.root.name.ToLower();

        // 1. Ignorar colisiones con el propio jugador para que no explote al nacer
        if (nombreChoque.Contains("player") || nombreChoque.Contains("imagen")) return;

        // 2. Comprobar si chocó con un enemigo (buscando en el nombre en vez de usar Tags)
        bool esEnemigo = nombreChoque.Contains("eagle") || nombreChoque.Contains("aguila") || 
                         nombreChoque.Contains("frog") || nombreChoque.Contains("opossum") || 
                         nombreChoque.Contains("enemy") ||
                         nombrePadre.Contains("eagle") || nombrePadre.Contains("aguila") || 
                         nombrePadre.Contains("frog") || nombrePadre.Contains("opossum");

        if (esEnemigo)
        {
            haChocado = true;
            Debug.Log("¡Cereza impactó al enemigo: " + colision.name + "!");
            
            // Buscar si el enemigo tiene el sistema de salud (en él o en su padre)
            EnemyHealth salud = colision.GetComponent<EnemyHealth>();
            if (salud == null && colision.transform.parent != null)
            {
                salud = colision.transform.parent.GetComponent<EnemyHealth>();
            }

            if (salud != null)
            {
                // Tiene el script de vida, le restamos 1 vida y le decimos dónde impactó
                salud.RecibirDano(1, transform.position);
            }
            else
            {
                // Por si se te olvidó ponerle el script al enemigo, lo destruye de un golpe (como antes)
                if (colision.transform.parent != null)
                {
                    string nombrePadreDirecto = colision.transform.parent.name.ToLower();
                    if (nombrePadreDirecto.Contains("eagle") || nombrePadreDirecto.Contains("aguila") || 
                        nombrePadreDirecto.Contains("frog") || nombrePadreDirecto.Contains("opossum"))
                    {
                        Destroy(colision.transform.parent.gameObject);
                    }
                    else
                    {
                        Destroy(colision.gameObject);
                    }
                }
                else
                {
                    Destroy(colision.gameObject);
                }
            }
        }

        // Destruir la cereza siempre que choque con algo válido (piso, pared, enemigo)
        Destroy(gameObject);
    }
}
