using UnityEngine;
using UnityEngine.SceneManagement;

public class ZonaDeMuerte : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D colision)
    {
        // Detectar si el objeto que acaba de caer es el jugador o algo que contenga "player" en su nombre
        if (colision.name.ToLower().Contains("player") || colision.CompareTag("Player"))
        {
            Debug.Log("El jugador cayó al vacío. Reiniciando nivel...");
            
            // Obtener el nombre de la escena actual en la que estamos
            string nombreEscenaActual = SceneManager.GetActiveScene().name;
            
            // Cargar (reiniciar) esa misma escena
            SceneManager.LoadScene(nombreEscenaActual);
        }
    }
}
