using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidas = 2; // Golpes necesarios para eliminarlo
    
    [Header("Efectos Visuales")]
    public GameObject explosionPrefab;

    // Esta función recibe el daño y el lugar exacto del golpe (para la explosión)
    public void RecibirDano(int cantidad, Vector3 posicionImpacto)
    {
        vidas -= cantidad;
        
        if (vidas <= 0)
        {
            Morir(posicionImpacto);
        }
    }

    void Morir(Vector3 posicionMuerte)
    {
        // 1. Mostrar la explosión exactamente donde pegó la cereza y destruirla rápido
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, posicionMuerte, Quaternion.identity);
            Destroy(exp, 0.5f); // Destruir la explosión después de 0.5s para no llenar memoria
        }
        
        // 2. Destruir al enemigo por completo
        Destroy(gameObject);
    }
}
