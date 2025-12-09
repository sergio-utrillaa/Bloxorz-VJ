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
    
    [Header("Animación de Entrada")]
    public bool playEntranceAnimation = true;
    public float entranceDelay = 0.1f;
    
    private bool isAnimating = false;
    
    // Clase auxiliar para almacenar información de elementos UI
    private class UIElementData
    {
        public RectTransform rectTransform;
        public Vector2 originalPosition;
        public Vector3 originalScale;
        public CanvasGroup canvasGroup;
    }
    
    // Clase auxiliar para animación de entrada
    private class EntranceElementData
    {
        public RectTransform rectTransform;
        public Vector2 originalPosition;
        public Vector3 originalScale;
        public Vector2 direction;
        public float delay;
        public CanvasGroup canvasGroup;
    }
    
    void Start()
    {
        if (playEntranceAnimation)
        {
            // Ocultar elementos inmediatamente antes del primer frame
            HideElementsImmediately();
            StartCoroutine(WaitForLayoutAndAnimate());
        }
    }
    
    void HideElementsImmediately()
    {
        // Ocultar título
        if (titleContainer != null)
        {
            CanvasGroup titleCanvas = titleContainer.GetComponent<CanvasGroup>();
            if (titleCanvas == null)
            {
                titleCanvas = titleContainer.AddComponent<CanvasGroup>();
            }
            titleCanvas.alpha = 0f;
        }
        
        // Ocultar elementos de la lista
        foreach (GameObject element in elementsToAnimate)
        {
            if (element == null) continue;
            
            CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = element.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }
    }
    
    IEnumerator WaitForLayoutAndAnimate()
    {
        // Esperar un frame para que el layout group calcule las posiciones
        yield return null;
        
        // Ahora ejecutar la animación de entrada
        StartCoroutine(PerformEntranceAnimation());
    }
    
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
    
    IEnumerator PerformEntranceAnimation()
    {
        // Normalizar direcciones
        elementsExitDirection.Normalize();
        alternateDirection.Normalize();
        titleExitDirection.Normalize();
        
        // Preparar elementos para entrada
        List<EntranceElementData> entranceElements = new List<EntranceElementData>();
        
        // Preparar título
        if (titleContainer != null)
        {
            RectTransform titleRect = titleContainer.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                EntranceElementData titleData = new EntranceElementData();
                titleData.rectTransform = titleRect;
                titleData.originalPosition = titleRect.anchoredPosition;
                titleData.originalScale = titleRect.localScale;
                titleData.direction = -titleExitDirection; // Dirección opuesta para entrada
                titleData.delay = 0f;
                
                // Preparar CanvasGroup
                titleData.canvasGroup = titleContainer.GetComponent<CanvasGroup>();
                if (titleData.canvasGroup == null && fadeOut)
                {
                    titleData.canvasGroup = titleContainer.AddComponent<CanvasGroup>();
                }
                
                // Posicionar fuera de pantalla
                titleRect.anchoredPosition = titleData.originalPosition + titleExitDirection * moveDistance;
                if (fadeOut && titleData.canvasGroup != null)
                {
                    titleData.canvasGroup.alpha = 0f;
                }
                if (scaleDown)
                {
                    titleRect.localScale = titleData.originalScale * minScale;
                }
                
                entranceElements.Add(titleData);
            }
        }
        
        // Preparar elementos de la lista
        for (int i = 0; i < elementsToAnimate.Count; i++)
        {
            if (elementsToAnimate[i] == null) continue;
            
            RectTransform rectTransform = elementsToAnimate[i].GetComponent<RectTransform>();
            if (rectTransform == null) continue;
            
            EntranceElementData elementData = new EntranceElementData();
            elementData.rectTransform = rectTransform;
            elementData.originalPosition = rectTransform.anchoredPosition;
            elementData.originalScale = rectTransform.localScale;
            
            // Dirección opuesta para entrada
            Vector2 exitDir = alternateDirections && (i % 2 == 1) ? alternateDirection : elementsExitDirection;
            elementData.direction = -exitDir;
            elementData.delay = entranceDelay + (i * delayBetweenElements);
            
            // Preparar CanvasGroup
            elementData.canvasGroup = elementsToAnimate[i].GetComponent<CanvasGroup>();
            if (elementData.canvasGroup == null && fadeOut)
            {
                elementData.canvasGroup = elementsToAnimate[i].AddComponent<CanvasGroup>();
            }
            
            // Posicionar fuera de pantalla
            rectTransform.anchoredPosition = elementData.originalPosition + exitDir * moveDistance;
            if (fadeOut && elementData.canvasGroup != null)
            {
                elementData.canvasGroup.alpha = 0f;
            }
            if (scaleDown)
            {
                rectTransform.localScale = elementData.originalScale * minScale;
            }
            
            entranceElements.Add(elementData);
        }
        
        // Animar entrada de todos los elementos
        foreach (var element in entranceElements)
        {
            StartCoroutine(AnimateEntrance(element));
        }
        
        yield return null;
    }
    
    IEnumerator AnimateEntrance(EntranceElementData data)
    {
        // Esperar el delay inicial
        yield return new WaitForSeconds(data.delay);
        
        Vector2 startPosition = data.rectTransform.anchoredPosition;
        Vector3 startScale = data.rectTransform.localScale;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Aplicar curva de movimiento
            float curveValue = movementCurve.Evaluate(t);
            data.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, data.originalPosition, curveValue);
            
            // Fade in si está habilitado
            if (fadeOut && data.canvasGroup != null)
            {
                float fadeValue = fadeCurve.Evaluate(1f - t); // Invertido para fade in
                data.canvasGroup.alpha = 1f - fadeValue;
            }
            
            // Scale up si está habilitado
            if (scaleDown)
            {
                float scale = Mathf.Lerp(minScale, 1f, t);
                data.rectTransform.localScale = data.originalScale * scale;
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        data.rectTransform.anchoredPosition = data.originalPosition;
        if (fadeOut && data.canvasGroup != null)
        {
            data.canvasGroup.alpha = 1f;
        }
        if (scaleDown)
        {
            data.rectTransform.localScale = data.originalScale;
        }
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
}
