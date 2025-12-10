using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Script para cambiar el color del texto de un botón cuando el ratón está encima con fade
public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Colores")]
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.yellow;
    
    [Header("Configuración de Fade")]
    public float fadeDuration = 0.2f;
    
    [Header("Configuración de Movimiento")]
    public bool enableMovement = true;
    public float moveUpDistance = 10f;
    public float moveDuration = 0.2f;
    
    [Header("Referencias (auto-detectar)")]
    public Text legacyText;
    public TextMeshProUGUI tmpText;
    
    private Color originalColor;
    private Color targetColor;
    private Color currentColor;
    private float fadeTimer = 0f;
    private bool isFading = false;
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private bool positionCaptured = false;
    private SceneTransitionAnimator sceneAnimator;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        sceneAnimator = FindObjectOfType<SceneTransitionAnimator>();
        
        if (legacyText == null)
        {
            legacyText = GetComponentInChildren<Text>();
        }
        
        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (legacyText != null)
        {
            originalColor = legacyText.color;
            normalTextColor = originalColor;
            currentColor = originalColor;
        }
        else if (tmpText != null)
        {
            originalColor = tmpText.color;
            normalTextColor = originalColor;
            currentColor = originalColor;
        }
        
        targetColor = normalTextColor;
        
        // Capturar posición original después de que el layout group haya calculado posiciones
        StartCoroutine(CaptureOriginalPositionAfterLayout());
    }
    
    System.Collections.IEnumerator CaptureOriginalPositionAfterLayout()
    {
        // Esperar un frame para que el layout group calcule las posiciones
        yield return null;
        
        // Esperar a que la animación de entrada termine si existe
        if (sceneAnimator != null && sceneAnimator.playEntranceAnimation)
        {
            while (!sceneAnimator.IsEntranceAnimationComplete)
            {
                yield return null;
            }
        }
        
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            targetPosition = originalPosition;
            positionCaptured = true;
        }
    }
    
    void Update()
    {
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);
            
            // Interpolación suave con ease-out
            t = 1f - Mathf.Pow(1f - t, 2f);
            
            currentColor = Color.Lerp(currentColor, targetColor, t);
            ApplyTextColor(currentColor);
            
            if (t >= 1f)
            {
                isFading = false;
                currentColor = targetColor;
            }
        }
        
        if (isMoving && enableMovement && rectTransform != null)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / moveDuration);
            
            // Interpolación suave con ease-out
            t = 1f - Mathf.Pow(1f - t, 2f);
            
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, t);
            
            if (t >= 1f)
            {
                isMoving = false;
                rectTransform.anchoredPosition = targetPosition;
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(hoverTextColor);
        
        if (enableMovement && rectTransform != null && positionCaptured)
        {
            StartMove(originalPosition + new Vector2(0, moveUpDistance));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(normalTextColor);
        
        if (enableMovement && rectTransform != null && positionCaptured)
        {
            StartMove(originalPosition);
        }
    }
    
    private void StartFade(Color target)
    {
        targetColor = target;
        fadeTimer = 0f;
        isFading = true;
    }
    
    private void StartMove(Vector2 target)
    {
        targetPosition = target;
        moveTimer = 0f;
        isMoving = true;
    }
    
    private void ApplyTextColor(Color color)
    {
        if (legacyText != null)
        {
            legacyText.color = color;
        }
        
        if (tmpText != null)
        {
            tmpText.color = color;
        }
    }
}
