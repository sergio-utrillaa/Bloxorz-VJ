using System.Collections;
using UnityEngine;

public class BridgeRiseAnimation : MonoBehaviour
{
    private Vector3 originalPosition;      // Posición original guardada la primera vez
    private Quaternion originalRotation;   // Rotación original guardada la primera vez
    private Vector3 hiddenPosition;        // Posición oculta (debajo del suelo)
    private Quaternion hiddenRotation;     // Rotación oculta (90 grados rotado)
    private Vector3 pivotOffset;           // Offset del punto de pivote para la rotación
    private float animationDuration = 0.5f;
    private bool isAnimating = false;
    private bool isInitialized = false;    // Flag para saber si ya se inicializó
    
    void Awake()
    {
        // Guardar posición y rotación original al crear el componente
        if (!isInitialized)
        {
            InitializePositions();
        }
    }
    
    void InitializePositions()
    {
        // Guardar posición y rotación original
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Detectar la orientación del puente para determinar el pivote correcto
        // Asumiendo que el puente tiene escala (1, 0.1, 1) y dos tiles
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Determinar si el puente está orientado en X o Z
        bool isOrientedInX = Mathf.Abs(right.x) > Mathf.Abs(right.z);
        
        if (isOrientedInX)
        {
            // Puente orientado en X (horizontal)
            // El pivote está en el borde inferior del primer tile
            pivotOffset = new Vector3(-0.5f, -0.05f, 0f); // Borde izquierdo inferior
            
            // Rotación inicial: 90 grados hacia abajo alrededor del eje Z
            hiddenRotation = originalRotation * Quaternion.Euler(0f, 0f, -90f);
            
            // Calcular posición oculta basada en el pivote
            // Cuando está rotado 90°, el puente apunta hacia abajo
            Vector3 hiddenOffset = originalRotation * new Vector3(0f, -1.0f, 0f);
            hiddenPosition = originalPosition + pivotOffset + hiddenOffset - pivotOffset;
        }
        else
        {
            // Puente orientado en Z (vertical)
            // El pivote está en el borde inferior del primer tile
            pivotOffset = new Vector3(0f, -0.05f, -0.5f); // Borde frontal inferior
            
            // Rotación inicial: 90 grados hacia abajo alrededor del eje X
            hiddenRotation = originalRotation * Quaternion.Euler(90f, 0f, 0f);
            
            // Calcular posición oculta basada en el pivote
            Vector3 hiddenOffset = originalRotation * new Vector3(0f, -1.0f, 0f);
            hiddenPosition = originalPosition + pivotOffset + hiddenOffset - pivotOffset;
        }
        
        isInitialized = true;
        
        Debug.Log($"Puente {gameObject.name} inicializado:");
        Debug.Log($"  Original Position: {originalPosition}");
        Debug.Log($"  Hidden Position: {hiddenPosition}");
        Debug.Log($"  Pivot Offset: {pivotOffset}");
        Debug.Log($"  Oriented in X: {isOrientedInX}");
    }
    
    public void StartRiseAnimation(float duration = 0.5f)
    {
        if (isAnimating) return;
        
        if (!isInitialized)
        {
            InitializePositions();
        }
        
        animationDuration = duration;
        isAnimating = true;
        StartCoroutine(AnimateRise());
    }
    
    IEnumerator AnimateRise()
    {
        float elapsedTime = 0f;
        
        // Asegurar que empieza desde la posición oculta
        transform.position = hiddenPosition;
        transform.rotation = hiddenRotation;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Curva de animación suave (ease out cubic)
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            // Interpolar rotación y posición usando el pivote
            // Calcular punto de pivote en el mundo
            Vector3 currentPivot = Vector3.Lerp(
                hiddenPosition + hiddenRotation * pivotOffset,
                originalPosition + originalRotation * pivotOffset,
                t
            );
            
            // Interpolar rotación
            transform.rotation = Quaternion.Slerp(hiddenRotation, originalRotation, t);
            
            // Calcular posición basada en el pivote actual
            transform.position = currentPivot - transform.rotation * pivotOffset;
            
            yield return null;
        }
        
        // Asegurar posición y rotación final (original)
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        isAnimating = false;
    }
    
    public void StartFallAnimation(float duration = 0.5f)
    {
        if (isAnimating) return;
        
        if (!isInitialized)
        {
            InitializePositions();
        }
        
        animationDuration = duration;
        isAnimating = true;
        StartCoroutine(AnimateFall());
    }
    
    IEnumerator AnimateFall()
    {
        float elapsedTime = 0f;
        
        // Asegurar que empieza desde la posición original
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Curva de animación (ease in)
            t = t * t;
            
            // Interpolar rotación y posición usando el pivote
            Vector3 currentPivot = Vector3.Lerp(
                originalPosition + originalRotation * pivotOffset,
                hiddenPosition + hiddenRotation * pivotOffset,
                t
            );
            
            // Interpolar rotación
            transform.rotation = Quaternion.Slerp(originalRotation, hiddenRotation, t);
            
            // Calcular posición basada en el pivote actual
            transform.position = currentPivot - transform.rotation * pivotOffset;
            
            yield return null;
        }
        
        // Asegurar posición y rotación final (oculta)
        transform.position = hiddenPosition;
        transform.rotation = hiddenRotation;
        
        isAnimating = false;
        
        // Desactivar el puente después de la animación
        gameObject.SetActive(false);
    }
}