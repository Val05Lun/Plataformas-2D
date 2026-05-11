using UnityEngine;

public enum ModoPatrullaje 
{ 
    PorDistancia,       // Camina una distancia fija
    PorPlataforma,      // Aprovecha todo el piso sin caerse
    PorTecho            // Aprovecha todo el techo sin caerse (ideal para murciélagos)
}

public class EnemigoPatrullaje : MonoBehaviour
{
    [Header("Comportamiento de Patrullaje")]
    [Tooltip("Elige cómo el enemigo decide su ruta")]
    public ModoPatrullaje modoPatrullaje = ModoPatrullaje.PorPlataforma;

    [Tooltip("Solo se usa si el modo es 'PorDistancia'")]
    public float distanciaMovimiento = 3f; 
    public float velocidad = 2f;
    
    [Header("Ajustes de Detección")]
    [Tooltip("Desactiva esto si el enemigo se traba con paredes invisibles (¡Cuidado, atravesará todo!)")]
    public bool detectarParedes = true;
    [Tooltip("Aumenta esto (ej. 1.0 o 2.0) si el enemigo se da la vuelta sin razón en pisos/techos irregulares")]
    public float distanciaRayoBorde = 0.5f;
    [Tooltip("Escribe los Tags (etiquetas) de los objetos que el enemigo debe ignorar y atravesar (ej. Picos, Arbol)")]
    public string[] tagsIgnorados;

    [Header("Ajustes de Terreno")]
    public bool detectarSuelo = true; // Si es true, el enemigo se pegará al suelo
    public float fuerzaGravedad = 9.8f; // Fuerza con la que cae si no hay suelo cerca
    [Tooltip("Ajusta este valor (ej. 0.3 o 0.5) si el enemigo se ve hundido en el piso")]
    public float ajusteAlturaSuelo = 0f;

    private Vector3 posicionInicial;
    private bool moviendoDerecha = true;
    private SpriteRenderer spriteRenderer;
    private Collider2D _collider;

    void Start()
    {
        posicionInicial = transform.position;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Si está activada la detección, evitamos que flote y lo adaptamos al terreno
        // No aplicamos gravedad si el modo es PorTecho, para que no caigan los murciélagos
        if (detectarSuelo && modoPatrullaje != ModoPatrullaje.PorTecho)
        {
            AdaptarAlSuelo();
        }

        // Revisar si hay una pared enfrente o si llegamos al final de la plataforma/techo
        if (HayParedEnfrente() || HayBordeEnfrente())
        {
            // Cambiar de dirección inmediatamente
            moviendoDerecha = !moviendoDerecha;
            // Si usamos distancia, actualizamos la posición inicial para que no se trabe
            if (modoPatrullaje == ModoPatrullaje.PorDistancia) 
            {
                posicionInicial = transform.position;
            }
        }

        // Mover al enemigo
        if (moviendoDerecha)
        {
            transform.Translate(Vector2.right * velocidad * Time.deltaTime);
            if (spriteRenderer != null) spriteRenderer.flipX = true; 
            
            if (modoPatrullaje == ModoPatrullaje.PorDistancia && transform.position.x >= posicionInicial.x + distanciaMovimiento)
                moviendoDerecha = false;
        }
        else
        {
            transform.Translate(Vector2.left * velocidad * Time.deltaTime);
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            
            if (modoPatrullaje == ModoPatrullaje.PorDistancia && transform.position.x <= posicionInicial.x - distanciaMovimiento)
                moviendoDerecha = true;
        }
    }

    // Función de ayuda para lanzar un raycast que ignore triggers y objetos especificados
    private RaycastHit2D LanzaRayoSeguro(Vector2 origen, Vector2 direccion, float distancia)
    {
        bool estadoAnterior = _collider.enabled;
        _collider.enabled = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, direccion, distancia);
        
        _collider.enabled = estadoAnterior;

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue; // Ignora siempre los triggers
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Proyectil") || hit.collider.CompareTag("Enemy")) continue;
            
            // Verificar si tiene un tag que queremos ignorar
            bool ignorarEsteObjeto = false;
            if (tagsIgnorados != null)
            {
                foreach (string tagIgnorado in tagsIgnorados)
                {
                    if (!string.IsNullOrEmpty(tagIgnorado) && hit.collider.CompareTag(tagIgnorado))
                    {
                        ignorarEsteObjeto = true;
                        break;
                    }
                }
            }

            if (ignorarEsteObjeto) continue; // Si es un tag ignorado, pasamos al siguiente

            return hit; // Encontramos algo sólido y válido
        }
        return new RaycastHit2D(); // Devuelve vacío si no encuentra nada sólido
    }

    // Método que detecta si hay un obstáculo en la dirección de movimiento
    private bool HayParedEnfrente()
    {
        if (_collider == null) return false;
        
        // Si el usuario desactivó la detección de paredes, ignoramos cualquier obstáculo frontal
        if (!detectarParedes) return false;

        Vector2 direccion = moviendoDerecha ? Vector2.right : Vector2.left;
        float distanciaRayo = _collider.bounds.extents.x + 0.15f;
        
        // Lanzamos dos rayos: uno desde el centro y otro desde los pies, por si la pared es muy baja o el enemigo muy alto
        Vector2 origenCentro = _collider.bounds.center;
        Vector2 origenPies = new Vector2(_collider.bounds.center.x, _collider.bounds.min.y + 0.2f);

        RaycastHit2D hitCentro = LanzaRayoSeguro(origenCentro, direccion, distanciaRayo);
        RaycastHit2D hitPies = LanzaRayoSeguro(origenPies, direccion, distanciaRayo);

        // Si cualquiera de los dos rayos choca con algo sólido, hay pared
        if (hitCentro.collider != null || hitPies.collider != null)
        {
            return true;
        }
        return false;
    }

    // Método que detecta si el suelo (o el techo) se acabó enfrente de nosotros
    private bool HayBordeEnfrente()
    {
        if (_collider == null) return false;

        if (modoPatrullaje == ModoPatrullaje.PorDistancia) return false;

        float xFrontal = moviendoDerecha ? _collider.bounds.max.x : _collider.bounds.min.x;
        xFrontal += (moviendoDerecha ? 0.1f : -0.1f); // Nos asomamos un poquito

        if (modoPatrullaje == ModoPatrullaje.PorPlataforma)
        {
            Vector2 origenRayo = new Vector2(xFrontal, _collider.bounds.center.y);
            // Usamos la distancia personalizable para detectar pisos irregulares
            float distanciaRayo = _collider.bounds.extents.y + distanciaRayoBorde; 

            RaycastHit2D hit = LanzaRayoSeguro(origenRayo, Vector2.down, distanciaRayo);

            if (hit.collider == null)
            {
                return true; // No hay suelo válido, hay precipicio
            }
        }
        else if (modoPatrullaje == ModoPatrullaje.PorTecho)
        {
            Vector2 origenRayo = new Vector2(xFrontal, _collider.bounds.center.y);
            // Usamos la distancia personalizable para detectar techos alejados
            float distanciaRayo = _collider.bounds.extents.y + distanciaRayoBorde;

            RaycastHit2D hit = LanzaRayoSeguro(origenRayo, Vector2.up, distanciaRayo);

            if (hit.collider == null)
            {
                return true; // Se acabó el techo sólido
            }
        }

        return false;
    }

    // Método para pegar al enemigo al suelo o aplicarle gravedad si está en el aire
    private void AdaptarAlSuelo()
    {
        if (_collider == null) return;

        Vector2 origenRayo = new Vector2(_collider.bounds.center.x, _collider.bounds.max.y);
        float caidaFrame = fuerzaGravedad * Time.deltaTime;
        float distanciaRayo = _collider.bounds.size.y + 1f; 

        RaycastHit2D hit = LanzaRayoSeguro(origenRayo, Vector2.down, distanciaRayo);

        if (hit.collider != null)
        {
            float diferenciaY = hit.point.y - _collider.bounds.min.y;
            
            if (Mathf.Abs(diferenciaY) <= 0.8f + Mathf.Abs(ajusteAlturaSuelo)) 
            {
                // Usamos Lerp para mover al enemigo suavemente y evitar que se trabe/tiemble con otras colisiones
                float targetY = transform.position.y + diferenciaY + ajusteAlturaSuelo;
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), 15f * Time.deltaTime);
            }
            else if (diferenciaY < -0.8f) 
            {
                transform.Translate(Vector3.down * caidaFrame, Space.World);
            }
        }
        else
        {
            transform.Translate(Vector3.down * caidaFrame, Space.World);
        }
    }

    // Respaldo de colisiones físicas
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarColisionFisica(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcesarColisionFisica(collision);
    }

    private void ProcesarColisionFisica(Collision2D collision)
    {
        if (_collider == null) return;

        // 1. Desactivar colisión física con objetos de la lista de ignorados
        if (tagsIgnorados != null && tagsIgnorados.Length > 0)
        {
            foreach (string tag in tagsIgnorados)
            {
                if (!string.IsNullOrEmpty(tag) && collision.collider.CompareTag(tag))
                {
                    Physics2D.IgnoreCollision(_collider, collision.collider, true);
                    return; // Si es ignorado, no lo procesamos como pared
                }
            }
        }

        // 2. Si es una pared real y falló el Raycast, nos damos vuelta físicamente
        if (detectarParedes && !collision.collider.CompareTag("Player") && !collision.collider.CompareTag("Proyectil") && !collision.collider.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contacto in collision.contacts)
            {
                // Si la colisión fue de lado (eje X)
                if (Mathf.Abs(contacto.normal.x) > 0.5f)
                {
                    // Si la pared nos empuja en sentido contrario a donde vamos
                    float direccionX = moviendoDerecha ? 1f : -1f;
                    if (Mathf.Sign(contacto.normal.x) != Mathf.Sign(direccionX))
                    {
                        moviendoDerecha = !moviendoDerecha;
                        if (modoPatrullaje == ModoPatrullaje.PorDistancia) 
                        {
                            posicionInicial = transform.position;
                        }
                        break;
                    }
                }
            }
        }
    }
}
