using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalAnimation : MonoBehaviour
{
    private float fallSpeed = 10.0f; // Velocidad de caída
    private float fallAcceleration = 20f; // Aceleración de la caída
    private float currentFallSpeed = 0f;
    private bool isFalling = false;
    
    public void StartGoalAnimation()
    {
        isFalling = true;
        currentFallSpeed = 0f;
        StartCoroutine(GoalAnimationSequence());
    }
    
    IEnumerator GoalAnimationSequence()
    {
        // Pequeña pausa inicial
        yield return new WaitForSeconds(0.1f);
        
        // Caída con aceleración
        while (isFalling)
        {
            currentFallSpeed += fallAcceleration * Time.deltaTime;
            if (currentFallSpeed > fallSpeed)
                currentFallSpeed = fallSpeed;
            
            transform.Translate(Vector3.down * currentFallSpeed * Time.deltaTime, Space.World);
            
            // Terminar cuando el cubo haya caído suficiente
            if (transform.position.y < -10.0f)
            {
                isFalling = false;
            }
            
            yield return null;
        }
        
        // Pausa antes de cambiar de nivel
        yield return new WaitForSeconds(0.5f);
        
        // Avanzar al siguiente nivel
        MapCreation mapCreation = FindObjectOfType<MapCreation>();
        if (mapCreation != null)
        {
            mapCreation.OnLevelComplete();
        }
        else
        {
            Debug.LogWarning("MapCreation no encontrado...");
        }
    }
}