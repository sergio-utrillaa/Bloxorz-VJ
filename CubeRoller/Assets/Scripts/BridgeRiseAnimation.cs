using System.Collections;
using UnityEngine;

public class BridgeRiseAnimation : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Duración de la animación en segundos")]
    public float animationDuration = 1f;
    
    [Tooltip("Ángulo máximo de apertura del puente (grados)")]
    public float openAngle = 180f;
    
    [Header("Referencias")]
    [Tooltip("Primera parte del puente (se detecta automáticamente si no se asigna)")]
    public Transform leftPart;
    
    [Tooltip("Segunda parte del puente (se detecta automáticamente si no se asigna)")]
    public Transform rightPart;
    
    public Vector3 leftpivotoffset = new Vector3(-0.5f, 0.05f, 0f);
    public Vector3 rightpivotoffset = new Vector3(0.5f, 0.05f, 0f);
    
    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Vector3 leftPivotPoint;
    private Vector3 rightPivotPoint;
    private Vector3 rotationAxis;
    private bool isAnimating = false;
    private bool isInitialized = false;
    private bool isOrientedInX = true;
    private bool isCurrentlyOpen = false;
    
    public void InitializeBridge()
    {
        // Buscar las dos partes del puente automáticamente si no están asignadas
        if (leftPart == null || rightPart == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            int childCount = 0;
            
            foreach (Transform child in children)
            {
                if (child != transform) // Ignorar el padre
                {
                    if (leftPart == null)
                        leftPart = child;
                    else if (rightPart == null)
                        rightPart = child;
                    
                    childCount++;
                    if (childCount >= 2) break;
                }
            }
        }
        
        if (leftPart == null || rightPart == null)
        {
            Debug.LogError($"Bridge {gameObject.name} no tiene 2 partes (tiles). Asegúrate de que el prefab tenga 2 hijos.");
            return;
        }
        
        // Guardar rotaciones cerradas (posición actual)
        leftClosedRotation = leftPart.localRotation;
        rightClosedRotation = rightPart.localRotation;
        
        // Determinar orientación del puente y calcular puntos de pivote
        Vector3 bridgeForward = transform.forward;
        isOrientedInX = Mathf.Abs(bridgeForward.x) > Mathf.Abs(bridgeForward.z);
        Debug.Log($"[Bridge] {gameObject.name} orientado en: {(isOrientedInX ? "X" : "Z")} (forward: {bridgeForward})");
        
        leftPivotPoint = leftPart.position + leftPart.TransformDirection(leftpivotoffset);
        rightPivotPoint = rightPart.position + rightPart.TransformDirection(rightpivotoffset);
        rotationAxis = leftPart.forward;
        
        isInitialized = true;
        isCurrentlyOpen = true;
        SetOpen(true);
    }
    
    public void StartRiseAnimation(float duration = -1f)
    {
        if (isAnimating) return;
        if (!isInitialized) InitializeBridge();
        if (duration > 0) animationDuration = duration;
        StartCoroutine(AnimateToOpen());
    }
    
    IEnumerator AnimateToOpen()
    {
        Debug.Log($"[Bridge] {gameObject.name} - AnimateToOpen INICIADO (Orientación: {(isOrientedInX ? "X" : "Z")})");
        isAnimating = true;
        float elapsedTime = 0f;
        
        // Guardar posiciones y rotaciones iniciales
        Vector3 leftStartPos = leftPart.position;
        Vector3 rightStartPos = rightPart.position;
        Quaternion leftStartRot = leftPart.rotation;
        Quaternion rightStartRot = rightPart.rotation;
        
        // Calcular la rotación necesaria para cerrar desde la posición actual
        float currentLeftAngle = Vector3.SignedAngle(
            leftClosedRotation * Vector3.up,
            leftStartRot * Vector3.up,
            rotationAxis
        );
        float currentRightAngle = Vector3.SignedAngle(
            rightClosedRotation * Vector3.up,
            rightStartRot * Vector3.up,
            rotationAxis
        );
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Curva suave (ease out)
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            // Calcular el ángulo actual de rotación
            float leftAngle = Mathf.Lerp(0f, -currentLeftAngle, t);
            float rightAngle = Mathf.Lerp(0f, -currentRightAngle, t);
            
            // Restaurar a posición inicial y aplicar rotación alrededor del pivote
            leftPart.position = leftStartPos;
            leftPart.rotation = leftStartRot;
            leftPart.RotateAround(leftPivotPoint, rotationAxis, -leftAngle);
            
            rightPart.position = rightStartPos;
            rightPart.rotation = rightStartRot;
            rightPart.RotateAround(rightPivotPoint, rotationAxis, rightAngle);
            
            yield return null;
        }
        
        leftPart.localRotation = leftClosedRotation;
        rightPart.localRotation = rightClosedRotation;
        isCurrentlyOpen = true;
        isAnimating = false;
        Debug.Log($"[Bridge] {gameObject.name} - AnimateToOpen COMPLETADO");
    }
    
    public void StartFallAnimation(float duration = -1f)
    {
        if (isAnimating) return;
        if (!isInitialized) InitializeBridge();
        if (duration > 0) animationDuration = duration;
        StartCoroutine(AnimateToClose());
    }
    
    IEnumerator AnimateToClose()
    {
        Debug.Log($"[Bridge] {gameObject.name} - AnimateToClose INICIADO (Orientación: {(isOrientedInX ? "X" : "Z")})");
        isAnimating = true;
        float elapsedTime = 0f;
        
        // Guardar posiciones y rotaciones iniciales (cerradas)
        Vector3 leftStartPos = leftPart.position;
        Vector3 rightStartPos = rightPart.position;
        Quaternion leftStartRot = leftPart.rotation;
        Quaternion rightStartRot = rightPart.rotation;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Curva de aceleración (ease in)
            t = t * t;
            
            // Calcular el ángulo actual de rotación (180 grados)
            float currentAngle = Mathf.Lerp(0f, openAngle, t);
            
            // Aplicar rotación alrededor del pivote
            leftPart.position = leftStartPos;
            leftPart.rotation = leftStartRot;
            
            rightPart.position = rightStartPos;
            rightPart.rotation = rightStartRot;
            
            leftPart.RotateAround(leftPivotPoint, rotationAxis, -currentAngle);
            rightPart.RotateAround(rightPivotPoint, rotationAxis, currentAngle);
            
            yield return null;
        }
        
        // Asegurar rotación final abierta (180 grados)
        leftPart.position = leftStartPos;
        leftPart.rotation = leftStartRot;
        
        rightPart.position = rightStartPos;
        rightPart.rotation = rightStartRot;
        
        if (isOrientedInX)
        {
            leftPart.RotateAround(leftPivotPoint, rotationAxis, -openAngle);
            rightPart.RotateAround(rightPivotPoint, rotationAxis, -openAngle);
        }
        else
        {
            leftPart.RotateAround(leftPivotPoint, rotationAxis, -openAngle);
            rightPart.RotateAround(rightPivotPoint, rotationAxis, openAngle);
        }
        
        isCurrentlyOpen = false;
        isAnimating = false;
        Debug.Log($"[Bridge] {gameObject.name} - AnimateToClose COMPLETADO");
    }
    
    public void Toggle()
    {
        if (isAnimating) return;
        if (!isInitialized) InitializeBridge();
        
        if (isCurrentlyOpen)
            StartFallAnimation(0.5f); // Cerrar
        else
            StartRiseAnimation(0.5f); // Abrir
    }
    
    public void SetOpen(bool open)
    {
        Debug.Log($"BridgeRiseAnimation SetOpen called with {open}");
        if (!isInitialized) InitializeBridge();
        isCurrentlyOpen = open;
    }
}