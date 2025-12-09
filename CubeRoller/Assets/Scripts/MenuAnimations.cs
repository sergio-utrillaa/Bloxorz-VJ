using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuAnimations : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public float animationDuration = 0.5f;
    public float moveDistance = 2000f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Dirección y Efectos")]
    public Vector2 exitDirection = new Vector2(-1, 0); // Dirección normalizada de salida
    public bool fadeOut = true; // También hacer fade out
    public float delayBetweenButtons = 0.05f; // Delay escalonado entre botones
    
    [Header("Título")]
    public GameObject titleContainer; // Referencia al TitleContainer
    public Vector2 titleExitDirection = new Vector2(0, 1); // Dirección de salida del título (arriba por defecto)
    
    [Header("Opciones Adicionales")]
    public bool scaleDown = false; // Reducir tamaño durante animación
    public float minScale = 0.5f;
    
    private bool isAnimating = false;
    
    public void PlayButtonPressed()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateAndLoadLevel("Level_1"));
        }
    }
    
    public void LoadLevelWithAnimation(string levelName)
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateAndLoadLevel(levelName));
        }
    }
    
    IEnumerator AnimateAndLoadLevel(string levelName)
    {
        isAnimating = true;
        
        // Encontrar todos los elementos UI animables
        Button[] allButtons = FindObjectsOfType<Button>();
        List<UIElement> uiElements = new List<UIElement>();
        
        // Preparar elementos para animación
        for (int i = 0; i < allButtons.Length; i++)
        {
            UIElement element = new UIElement();
            element.rectTransform = allButtons[i].GetComponent<RectTransform>();
            element.originalPosition = element.rectTransform.anchoredPosition;
            element.originalScale = element.rectTransform.localScale;
            
            // Obtener componentes para fade
            element.canvasGroup = allButtons[i].GetComponent<CanvasGroup>();
            if (element.canvasGroup == null && fadeOut)
            {
                element.canvasGroup = allButtons[i].gameObject.AddComponent<CanvasGroup>();
            }
            
            uiElements.Add(element);
            
            // Deshabilitar interacción inmediatamente
            allButtons[i].interactable = false;
        }
        
        // Normalizar dirección de salida
        exitDirection.Normalize();
        titleExitDirection.Normalize();
        
        // Animar el título si está asignado
        if (titleContainer != null)
        {
            RectTransform titleRect = titleContainer.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                StartCoroutine(AnimateTitle(titleRect));
            }
        }
        
        // Animar cada elemento con delay escalonado
        List<Coroutine> animationCoroutines = new List<Coroutine>();
        
        for (int i = 0; i < uiElements.Count; i++)
        {
            float delay = i * delayBetweenButtons;
            animationCoroutines.Add(StartCoroutine(AnimateElement(uiElements[i], delay)));
        }
        
        // Esperar a que todas las animaciones terminen
        float totalTime = animationDuration + (uiElements.Count * delayBetweenButtons);
        yield return new WaitForSeconds(totalTime + 0.1f);
        
        // Cargar el nivel
        SceneManager.LoadScene(levelName);
    }
    
    IEnumerator AnimateElement(UIElement element, float delay)
    {
        // Esperar el delay inicial
        yield return new WaitForSeconds(delay);
        
        Vector2 targetPosition = element.originalPosition + exitDirection * moveDistance;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Aplicar movimiento
            float moveT = movementCurve.Evaluate(t);
            element.rectTransform.anchoredPosition = Vector2.Lerp(element.originalPosition, targetPosition, moveT);
            
            // Aplicar fade out si está habilitado
            if (fadeOut && element.canvasGroup != null)
            {
                element.canvasGroup.alpha = fadeCurve.Evaluate(t);
            }
            
            // Aplicar escala si está habilitado
            if (scaleDown)
            {
                float scaleT = Mathf.Lerp(1f, minScale, t);
                element.rectTransform.localScale = element.originalScale * scaleT;
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        element.rectTransform.anchoredPosition = targetPosition;
        if (fadeOut && element.canvasGroup != null)
        {
            element.canvasGroup.alpha = 0f;
        }
        if (scaleDown)
        {
            element.rectTransform.localScale = element.originalScale * minScale;
        }
    }
    
    IEnumerator AnimateTitle(RectTransform titleRect)
    {
        Vector2 originalPosition = titleRect.anchoredPosition;
        Vector2 targetPosition = originalPosition + titleExitDirection * moveDistance;
        
        CanvasGroup titleCanvas = titleContainer.GetComponent<CanvasGroup>();
        if (titleCanvas == null && fadeOut)
        {
            titleCanvas = titleContainer.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Aplicar curva de movimiento
            float curveValue = movementCurve.Evaluate(t);
            titleRect.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, curveValue);
            
            // Fade out si está habilitado
            if (fadeOut && titleCanvas != null)
            {
                float fadeValue = fadeCurve.Evaluate(t);
                titleCanvas.alpha = fadeValue;
            }
            
            // Scale down si está habilitado
            if (scaleDown)
            {
                float scale = Mathf.Lerp(1f, minScale, t);
                titleRect.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        titleRect.anchoredPosition = targetPosition;
        if (fadeOut && titleCanvas != null)
        {
            titleCanvas.alpha = 0f;
        }
        if (scaleDown)
        {
            titleRect.localScale = Vector3.one * minScale;
        }
    }
    
    // Clase auxiliar para almacenar información de elementos UI
    private class UIElement
    {
        public RectTransform rectTransform;
        public Vector2 originalPosition;
        public Vector3 originalScale;
        public CanvasGroup canvasGroup;
    }
}