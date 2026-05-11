using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 10f;
    public float tiempoDeVida = 2.5f; // Se destruye tras 2.5s para no llenar la memoria
    
    private float direccionX;
    private bool haChocado = false;

    void Start()
    {
        // Programar su destrucción automática
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        if (!haChocado)
        {
            // Moverse hacia adelante
            transform.Translate(Vector2.right * direccionX * velocidad * Time.deltaTime);
        }
    }

    public void ConfigurarDireccion(float direccion)
    {
        direccionX = direccion;
        
        // Voltear la imagen de la cereza si disparamos a la izquierda
        if (direccionX < 0)
        {
            Vector3 escalaLocal = transform.localScale;
            escalaLocal.x *= -1;
            transform.localScale = escalaLocal;
        }
    }

    void OnTriggerEnter2D(Collider2D colision)
    {
        // Si ya chocó en este frame, ignorar cualquier otra colisión simultánea
        if (haChocado) return;

        string nombreColision = colision.name.ToLower();
        string tagColision = colision.tag.ToLower();

        // Ignorar al jugador para que no explote en nuestra propia cara
        if (nombreColision.Contains("player") || tagColision.Contains("player"))
        {
            return;
        }

        // Detectar si choca con algún enemigo
        bool esEnemigo = nombreColision.Contains("enemy") || tagColision.Contains("enemy") ||
                         nombreColision.Contains("eagle") || nombreColision.Contains("aguila") ||
                         nombreColision.Contains("frog") || nombreColision.Contains("rana") ||
                         nombreColision.Contains("opossum") || nombreColision.Contains("zarigueya") ||
                         nombreColision.Contains("bettle") || nombreColision.Contains("escarabajo") ||
                         nombreColision.Contains("dino") || nombreColision.Contains("dog") || nombreColision.Contains("perro") ||
                         nombreColision.Contains("slime") || nombreColision.Contains("bat") || nombreColision.Contains("murcielago");

        if (esEnemigo)
        {
            haChocado = true;
            Debug.Log("¡Cereza impactó al enemigo: " + colision.name + "!");
            
            // Buscar si el enemigo tiene el sistema de salud (en él o en su padre)
            EnemyHealth saludEnemigo = colision.GetComponent<EnemyHealth>();
            if (saludEnemigo == null && colision.transform.parent != null)
            {
                saludEnemigo = colision.transform.parent.GetComponent<EnemyHealth>();
            }

            if (saludEnemigo != null)
            {
                // Pasamos la posición exacta del impacto
                saludEnemigo.RecibirDano(1, transform.position);
            }
            else
            {
                // Si no tiene sistema de salud, lo destruimos al instante (por si acaso)
                if (colision.transform.parent != null)
                {
                    string nombrePadre = colision.transform.parent.name.ToLower();
                    if (nombrePadre.Contains("eagle") || nombrePadre.Contains("aguila") || nombrePadre.Contains("enemy") || 
                        nombrePadre.Contains("frog") || nombrePadre.Contains("rana") || nombrePadre.Contains("opossum") || nombrePadre.Contains("zarigueya") ||
                        nombrePadre.Contains("bettle") || nombrePadre.Contains("escarabajo") || nombrePadre.Contains("dino") || 
                        nombrePadre.Contains("dog") || nombrePadre.Contains("perro") || nombrePadre.Contains("slime") || 
                        nombrePadre.Contains("bat") || nombrePadre.Contains("murcielago"))
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

            // Destruir la cereza
            Destroy(gameObject);
        }
        else if (colision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                 nombreColision.Contains("ground") || tagColision.Contains("ground") || 
                 nombreColision.Contains("piso") || tagColision.Contains("piso") ||
                 nombreColision.Contains("tilemap"))
        {
            // Si choca contra el piso/pared, la cereza se destruye
            haChocado = true;
            Destroy(gameObject);
        }
    }
}
