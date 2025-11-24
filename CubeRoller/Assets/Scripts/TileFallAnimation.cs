using System.Collections;
using UnityEngine;

public class TileFallAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animationDuration;
    private float delay;
    
    public void StartFallAnimation(float animationDelay, float duration, float targetHeight)
    {
        delay = animationDelay;
        animationDuration = duration;
        startPosition = transform.position;
        targetPosition = new Vector3(startPosition.x, targetHeight, startPosition.z);
        
        StartCoroutine(FallAfterDelay());
    }
    
    IEnumerator FallAfterDelay()
    {
        // Esperar el delay antes de empezar a caer
        yield return new WaitForSeconds(delay);
        
        float elapsedTime = 0f;
        
        // Animar la caída
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Usar una curva de aceleración para que parezca más natural
            t = t * t; // Aceleración cuadrática
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        // Asegurar posición final
        transform.position = targetPosition;
        
        // Opcional: destruir el componente después de la animación
        Destroy(this);
    }
}