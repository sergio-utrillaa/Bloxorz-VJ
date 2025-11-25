using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animationDuration = 1.0f; // Duración de la animación de hundimiento
    private float sinkDepth = 1.0f; // Profundidad a la que se hunde el cubo
    
    public void StartGoalAnimation()
    {
        startPosition = transform.position;
        targetPosition = new Vector3(startPosition.x, startPosition.y - sinkDepth, startPosition.z);
        
        StartCoroutine(GoalAnimationSequence());
    }
    
    IEnumerator GoalAnimationSequence()
    {
        float elapsedTime = 0f;
        
        // Pequeña pausa inicial (como en Bloxorz)
        yield return new WaitForSeconds(0.3f);
        
        // El cubo se hunde gradualmente (completamente vertical)
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Usar una curva suave (ease-in-out)
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Movimiento completamente vertical (sin rotación)
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            yield return null;
        }
        
        // Asegurar posición final
        transform.position = targetPosition;
        
        // Pausa antes de cambiar de nivel
        yield return new WaitForSeconds(0.5f);
        
        // Avanzar al siguiente nivel
        MapCreation mapCreation = FindObjectOfType<MapCreation>();
        if (mapCreation != null)
        {
            mapCreation.GoToNextLevel();
        }
        else
        {
            Debug.LogWarning("MapCreation no encontrado. Reiniciando nivel actual...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}