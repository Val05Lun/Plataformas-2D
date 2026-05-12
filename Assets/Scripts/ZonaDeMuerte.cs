using UnityEngine;
using UnityEngine.SceneManagement;

public class ZonaDeMuerte : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D colision)
    {
        Debug.Log("La Zona de Muerte acaba de tocar a: " + colision.name + " (Tag: " + colision.tag + ")");
        
        // Detectar si el objeto que acaba de caer es el jugador o algo que contenga "player" 
        string nombreColision = colision.name.ToLower();
        string tagColision = colision.tag.ToLower();

        if (nombreColision.Contains("player") || tagColision.Contains("player"))
        {
            Debug.Log("Jugador cayó a la Zona de Muerte. Reiniciando nivel...");
            
            // Reiniciar el nivel actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
