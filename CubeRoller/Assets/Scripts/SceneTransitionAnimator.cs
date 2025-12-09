using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionAnimator : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public float animationDuration = 0.5f;
    public float moveDistance = 2000f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Título")]
    public GameObject titleContainer;
    public Vector2 titleExitDirection = new Vector2(0, 1);
    
    [Header("Elementos a Animar")]
    [Tooltip("Lista de GameObjects que se animarán (pueden ser botones, textos, containers, etc.)")]
    public List<GameObject> elementsToAnimate = new List<GameObject>();
    
    [Header("Dirección de Salida")]
    public Vector2 elementsExitDirection = new Vector2(-1, 0);
    public bool alternateDirections = false;
    public Vector2 alternateDirection = new Vector2(1, 0);
    
    [Header("Efectos")]
    public bool fadeOut = true;
    public bool scaleDown = false;
    public float minScale = 0.5f;
    public float delayBetweenElements = 0.05f;
    
    private bool isAnimating = false;
    
    public void AnimateAndLoadScene(string sceneName)
    {
        if (!isAnimating)
        {
            StartCoroutine(PerformAnimationAndLoadScene(sceneName));
        }
    }
    
    IEnumerator PerformAnimationAndLoadScene(string sceneName)
    {
        isAnimating = true;
        
        // Normalizar direcciones
        elementsExitDirection.Normalize();
        alternateDirection.Normalize();
        titleExitDirection.Normalize();
        
        // Animar el título si está asignado
        if (titleContainer != null)
        {
            RectTransform titleRect = titleContainer.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                StartCoroutine(AnimateElement(titleRect, titleExitDirection, 0f));
            }
        }
        
        // Preparar y animar elementos
        List<UIElementData> uiElements = new List<UIElementData>();
        
        for (int i = 0; i < elementsToAnimate.Count; i++)
        {
            if (elementsToAnimate[i] == null) continue;
            
            RectTransform rectTransform = elementsToAnimate[i].GetComponent<RectTransform>();
            if (rectTransform == null) continue;
            
            UIElementData elementData = new UIElementData();
            elementData.rectTransform = rectTransform;
            elementData.originalPosition = rectTransform.anchoredPosition;
            elementData.originalScale = rectTransform.localScale;
            
            // Preparar CanvasGroup para fade
            elementData.canvasGroup = elementsToAnimate[i].GetComponent<CanvasGroup>();
            if (elementData.canvasGroup == null && fadeOut)
            {
                elementData.canvasGroup = elementsToAnimate[i].AddComponent<CanvasGroup>();
            }
            
            // Deshabilitar botones si los hay
            Button button = elementsToAnimate[i].GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
            
            uiElements.Add(elementData);
            
            // Determinar dirección para este elemento
            Vector2 direction = alternateDirections && (i % 2 == 1) ? alternateDirection : elementsExitDirection;
            
            float delay = i * delayBetweenElements;
            StartCoroutine(AnimateElement(elementData.rectTransform, direction, delay, elementData.canvasGroup));
        }
        
        // Esperar a que todas las animaciones terminen
        float totalTime = animationDuration + (elementsToAnimate.Count * delayBetweenElements);
        yield return new WaitForSeconds(totalTime + 0.1f);
        
        // Cargar la escena
        SceneManager.LoadScene(sceneName);
    }
    
    IEnumerator AnimateElement(RectTransform rectTransform, Vector2 direction, float delay, CanvasGroup canvasGroup = null)
    {
        // Esperar el delay inicial
        yield return new WaitForSeconds(delay);
        
        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector3 originalScale = rectTransform.localScale;
        Vector2 targetPosition = originalPosition + direction * moveDistance;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Aplicar curva de movimiento
            float curveValue = movementCurve.Evaluate(t);
            rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, curveValue);
            
            // Fade out si está habilitado
            if (fadeOut && canvasGroup != null)
            {
                float fadeValue = fadeCurve.Evaluate(t);
                canvasGroup.alpha = fadeValue;
            }
            
            // Scale down si está habilitado
            if (scaleDown)
            {
                float scale = Mathf.Lerp(1f, minScale, t);
                rectTransform.localScale = originalScale * scale;
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        rectTransform.anchoredPosition = targetPosition;
        if (fadeOut && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (scaleDown)
        {
            rectTransform.localScale = originalScale * minScale;
        }
    }
    
    // Clase auxiliar para almacenar información de elementos UI
    private class UIElementData
    {
        public RectTransform rectTransform;
        public Vector2 originalPosition;
        public Vector3 originalScale;
        public CanvasGroup canvasGroup;
    }
}
