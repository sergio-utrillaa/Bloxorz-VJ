using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrangeTileFallAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animationDuration = 0.8f;
    private float fallDepth = 10.0f;
    
    public void StartFallAnimation()
    {
        startPosition = transform.position;
        targetPosition = new Vector3(startPosition.x, startPosition.y - fallDepth, startPosition.z);
        
        StartCoroutine(FallSequence());
    }
    
    IEnumerator FallSequence()
    {
        float elapsedTime = 0f;
        
        // El cubo cae verticalmente (sin rotación)
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Aceleración (simular gravedad)
            t = t * t;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        // Asegurar posición final
        transform.position = targetPosition;
        
        // Pequeña pausa
        yield return new WaitForSeconds(0.3f);
        
        // Reiniciar nivel
        Debug.Log("Reiniciando nivel por caída en tile naranja...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}