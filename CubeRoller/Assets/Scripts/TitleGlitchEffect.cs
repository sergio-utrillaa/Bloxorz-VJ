using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleGlitchEffect : MonoBehaviour
{
    [Header("Referencias a los textos")]
    [Tooltip("Texto principal (blanco)")]
    public GameObject mainText;
    
    [Tooltip("Texto desplazado rojo")]
    public GameObject redGlitchText;
    
    [Tooltip("Texto desplazado azul")]
    public GameObject blueGlitchText;
    
    [Header("Configuración del Glitch")]
    [Tooltip("Intervalo mínimo entre glitches (segundos)")]
    public float minGlitchInterval = 2f;
    
    [Tooltip("Intervalo máximo entre glitches (segundos)")]
    public float maxGlitchInterval = 5f;
    
    [Tooltip("Duración mínima del efecto glitch (segundos)")]
    public float minGlitchDuration = 0.05f;
    
    [Tooltip("Duración máxima del efecto glitch (segundos)")]
    public float maxGlitchDuration = 0.3f;
    
    [Header("Configuración de Parpadeos")]
    [Tooltip("Probabilidad de parpadeo durante el glitch (0-1)")]
    [Range(0f, 1f)]
    public float flickerChance = 0.6f;
    
    [Tooltip("Velocidad de parpadeo (parpadeos por segundo)")]
    public float flickerSpeed = 20f;
    
    [Header("Desplazamiento")]
    [Tooltip("Desplazamiento máximo en X durante el glitch")]
    public float maxOffsetX = 10f;
    
    [Tooltip("Desplazamiento máximo en Y durante el glitch")]
    public float maxOffsetY = 5f;
    
    [Header("Opciones")]
    [Tooltip("Activar al inicio")]
    public bool playOnStart = true;
    
    [Tooltip("El glitch se repite continuamente")]
    public bool loop = true;
    
    private bool isGlitching = false;
    private Vector3 mainTextOriginalPos;
    private Vector3 redGlitchOriginalPos;
    private Vector3 blueGlitchOriginalPos;
    
    void Start()
    {
        // Guardar posiciones originales
        if (mainText != null)
            mainTextOriginalPos = mainText.transform.localPosition;
        
        if (redGlitchText != null)
            redGlitchOriginalPos = redGlitchText.transform.localPosition;
        
        if (blueGlitchText != null)
            blueGlitchOriginalPos = blueGlitchText.transform.localPosition;
        // Inicialmente ocultar los textos glitch
        if (redGlitchText != null)
            redGlitchText.SetActive(false);
        
        if (blueGlitchText != null)
            blueGlitchText.SetActive(false);
        
        if (playOnStart)
        {
            StartGlitchEffect();
        }
    }
    
    public void StartGlitchEffect()
    {
        if (!isGlitching)
        {
            StartCoroutine(GlitchLoop());
        }
    }
    
    public void StopGlitchEffect()
    {
        isGlitching = false;
        StopAllCoroutines();
        
        // Restaurar posiciones originales
        if (mainText != null)
            mainText.transform.localPosition = mainTextOriginalPos;
        
        if (redGlitchText != null)
        {
            redGlitchText.SetActive(false);
            redGlitchText.transform.localPosition = redGlitchOriginalPos;
        }
        
        if (blueGlitchText != null)
        {
            blueGlitchText.SetActive(false);
            blueGlitchText.transform.localPosition = blueGlitchOriginalPos;
        }
    }
    
    private IEnumerator GlitchLoop()
    {
        isGlitching = true;
        
        while (loop || isGlitching)
        {
            // Esperar un intervalo aleatorio antes del próximo glitch
            float waitTime = Random.Range(minGlitchInterval, maxGlitchInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Ejecutar el efecto glitch
            float glitchDuration = Random.Range(minGlitchDuration, maxGlitchDuration);
            yield return StartCoroutine(PerformGlitch(glitchDuration));
            
            if (!loop)
                break;
        }
        
        isGlitching = false;
    }
    
    private IEnumerator PerformGlitch(float duration)
    {
        float elapsedTime = 0f;
        bool shouldFlicker = Random.value < flickerChance;
        bool wasVisible = false;
        Vector3 currentOffset = Vector3.zero;
        
        // Activar los textos glitch
        if (redGlitchText != null)
            redGlitchText.SetActive(true);
        
        if (blueGlitchText != null)
            blueGlitchText.SetActive(true);
        
        while (elapsedTime < duration)
        {
            if (shouldFlicker)
            {
                // Parpadeo rápido alternando la visibilidad y posición
                bool isVisible = Mathf.Sin(elapsedTime * flickerSpeed * Mathf.PI * 2) > 0;
                
                // Generar nuevo offset cada vez que cambia de invisible a visible
                if (isVisible && !wasVisible)
                {
                    currentOffset = new Vector3(
                        Random.Range(-maxOffsetX, maxOffsetX),
                        Random.Range(-maxOffsetY, maxOffsetY),
                        0f
                    );
                }
                
                wasVisible = isVisible;
                
                if (redGlitchText != null)
                    redGlitchText.SetActive(isVisible);
                
                if (blueGlitchText != null)
                    blueGlitchText.SetActive(isVisible);
                
                // Alternar entre posición original y desplazada
                if (mainText != null)
                    mainText.transform.localPosition = isVisible ? mainTextOriginalPos + currentOffset : mainTextOriginalPos;
                
                if (redGlitchText != null)
                    redGlitchText.transform.localPosition = isVisible ? redGlitchOriginalPos + currentOffset : redGlitchOriginalPos;
                
                if (blueGlitchText != null)
                    blueGlitchText.transform.localPosition = isVisible ? blueGlitchOriginalPos + currentOffset : blueGlitchOriginalPos;
            }
            else
            {
                // Sin flicker, usar un offset fijo
                if (currentOffset == Vector3.zero)
                {
                    currentOffset = new Vector3(
                        Random.Range(-maxOffsetX, maxOffsetX),
                        Random.Range(-maxOffsetY, maxOffsetY),
                        0f
                    );
                    
                    if (mainText != null)
                        mainText.transform.localPosition = mainTextOriginalPos + currentOffset;
                    
                    if (redGlitchText != null)
                        redGlitchText.transform.localPosition = redGlitchOriginalPos + currentOffset;
                    
                    if (blueGlitchText != null)
                        blueGlitchText.transform.localPosition = blueGlitchOriginalPos + currentOffset;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Restaurar posiciones originales
        if (mainText != null)
            mainText.transform.localPosition = mainTextOriginalPos;
        
        if (redGlitchText != null)
        {
            redGlitchText.SetActive(false);
            redGlitchText.transform.localPosition = redGlitchOriginalPos;
        }
        
        if (blueGlitchText != null)
        {
            blueGlitchText.SetActive(false);
            blueGlitchText.transform.localPosition = blueGlitchOriginalPos;
        }
    }
    
    // Método para activar un glitch instantáneo (útil para eventos)
    public void TriggerSingleGlitch()
    {
        if (!isGlitching)
        {
            StartCoroutine(SingleGlitch());
        }
    }
    
    private IEnumerator SingleGlitch()
    {
        float glitchDuration = Random.Range(minGlitchDuration, maxGlitchDuration);
        yield return StartCoroutine(PerformGlitch(glitchDuration));
    }
}
