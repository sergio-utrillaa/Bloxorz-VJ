using System.Collections;
using UnityEngine;

public class TileRiseAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation; // Guardar rotación inicial
    private Vector3 targetPosition;
    private float animationDuration;
    private float delay;
    private float rotationSpeed = 360f; // Reducido para rotación más suave
    private float spiralRadius = 3.0f; // Radio inicial de la espiral
    private float spiralFrequency = 4.0f; // Número de vueltas completas durante la subida
    
    public void StartRiseAnimation(float animationDelay, float duration, float riseHeight)
    {
        delay = animationDelay;
        animationDuration = duration;
        startPosition = transform.position;
        startRotation = transform.rotation; // Guardar rotación original
        targetPosition = new Vector3(startPosition.x, startPosition.y + riseHeight, startPosition.z);
        
        StartCoroutine(RiseAfterDelay());
    }
    
    IEnumerator RiseAfterDelay()
    {
        // Esperar el delay antes de empezar a subir
        yield return new WaitForSeconds(delay);
        
        float elapsedTime = 0f;
        float currentRotationY = 0f; // Acumular rotación en Y
        
        // Animar la subida con movimiento en espiral
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Curva de aceleración exponencial
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            
            // Calcular altura actual (interpolación entre start y target)
            float currentHeight = Mathf.Lerp(startPosition.y, targetPosition.y, easedT);
            
            // Calcular ángulo de la espiral (múltiples vueltas completas)
            float spiralAngle = t * spiralFrequency * Mathf.PI * 2f;
            
            // El radio empieza grande y se reduce a 0 cuando llega arriba
            float currentRadius = spiralRadius * (1f - easedT);
            
            // Calcular offset X y Z basado en el ángulo y radio actual
            float offsetX = Mathf.Cos(spiralAngle) * currentRadius;
            float offsetZ = Mathf.Sin(spiralAngle) * currentRadius;
            
            // Posición final = posición original + offset de espiral + altura
            Vector3 spiralPosition = new Vector3(
                startPosition.x + offsetX,
                currentHeight,
                startPosition.z + offsetZ
            );
            
            transform.position = spiralPosition;
            
            // SOLO rotar en el eje Y para mantener la apariencia original
            currentRotationY += rotationSpeed * Time.deltaTime;
            transform.rotation = startRotation * Quaternion.Euler(0, currentRotationY, 0);
            
            // Eliminar efecto de escala pulsante para mantener forma original
            // (comentado para que no se deformen)
            
            yield return null;
        }
        
        // Asegurar posición final (sin offset de espiral)
        transform.position = targetPosition;
        transform.rotation = startRotation; // Restaurar rotación original
    }
}