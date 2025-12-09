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
    
    [Header("Referencias (auto-detectar)")]
    public Text legacyText;
    public TextMeshProUGUI tmpText;
    
    private Color originalColor;
    private Color targetColor;
    private Color currentColor;
    private float fadeTimer = 0f;
    private bool isFading = false;
    
    void Start()
    {
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
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(hoverTextColor);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(normalTextColor);
    }
    
    private void StartFade(Color target)
    {
        targetColor = target;
        fadeTimer = 0f;
        isFading = true;
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
