using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"GoalTrigger detectó colisión con: {other.name}, tag: {other.tag}");
        
        // Detectar cuando el cubo entra en el agujero
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            // Obtener el componente MoveCube del cubo
            MoveCube moveCube = other.GetComponent<MoveCube>();
            
            if (moveCube != null)
            {
                Debug.Log($"Cubo detectado. Estado vertical: {moveCube.IsVertical()}");
                
                // Solo activar si el cubo está en posición vertical
                if (moveCube.IsVertical())
                {
                    Debug.Log("¡Cubo ha caído en el agujero de meta!");
                    moveCube.StartGoalAnimation();
                }
                else
                {
                    Debug.Log("Cubo no está vertical, no se activa meta");
                    hasTriggered = false; // Permitir intentos posteriores
                }
            }
            else
            {
                Debug.LogWarning("No se encontró componente MoveCube en el objeto detectado");
                hasTriggered = false;
            }
        }
    }
    
    // Opcional: resetear si el cubo sale del trigger sin estar vertical
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hasTriggered)
        {
            MoveCube moveCube = other.GetComponent<MoveCube>();
            if (moveCube != null && !moveCube.IsVertical())
            {
                hasTriggered = false; // Permitir nuevo intento
                Debug.Log("Cubo salió del trigger sin estar vertical, reseteando");
            }
        }
    }
}