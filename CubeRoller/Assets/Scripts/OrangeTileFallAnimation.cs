using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrangeTileFallAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animationDuration = 0.8f;
    private float fallDepth = 10.0f;
    
    public void StartFallAnimation(float duration = 0.8f)
    {
        startPosition = transform.position;
        targetPosition = new Vector3(startPosition.x, startPosition.y - fallDepth, startPosition.z);
        animationDuration = duration;
        
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
        
        // NUEVO: Activar animación de caída de todos los tiles
        Debug.Log("Cubo y tile naranja han caído. Activando animación de caída de todos los tiles...");
        
        MapCreation mapCreation = FindObjectOfType<MapCreation>();
        if (mapCreation != null)
        {
            // Llamar al método OnCubeFell que ya tienes implementado
            mapCreation.OnCubeFell();

            // Reiniciar contador antes de llamar a OnCubeFell
            //MoveCounter.Instance.RestartLevel();
        }
        else
        {
            // Si no se encuentra MapCreation, reiniciar directamente
            Debug.LogWarning("MapCreation no encontrado. Reiniciando nivel directamente...");
            MoveCounter.Instance.RestartLevel();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}