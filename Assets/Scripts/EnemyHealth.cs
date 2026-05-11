using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidas = 1; // Golpes necesarios para eliminarlo
    
    [Header("Efectos Visuales")]
    public GameObject explosionPrefab;

    // Esta función recibe el daño y el lugar exacto del golpe (para la explosión)
    public void RecibirDano(int cantidad, Vector3 posicionImpacto)
    {
        vidas -= cantidad;
        
        if (vidas <= 0)
        {
            // Crear la explosión justo donde nos pegaron
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, posicionImpacto, Quaternion.identity);
                Destroy(explosion, 1f); // Limpiar la explosión de la memoria
            }
            
            // Destruir al enemigo
            Destroy(gameObject);
        }
    }
}
