using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTransitionManager : MonoBehaviour
{
    private static LevelTransitionManager instance;
    public static LevelTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("LevelTransitionManager");
                instance = obj.AddComponent<LevelTransitionManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private bool isFirstTimeInLevel = true;
    private GameObject fadePanel;
    private Image fadeImage;
    private TextMeshProUGUI stageText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse a eventos de carga de escena para limpiar el panel
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si estamos en el menú o créditos, destruir este singleton y limpiar todo
        if (scene.name == "Menu" || scene.name == "Credits")
        {
            CleanupFadePanel();
            
            // Destruir el singleton ya que no se necesita en el menú
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
            Destroy(gameObject);
        }
    }

    // Método para mostrar la pantalla de Stage al entrar a un nivel por primera vez
    public IEnumerator ShowStageScreen(string sceneName, System.Action onComplete)
    {
        isFirstTimeInLevel = true;

        // PAUSAR EL JUEGO mientras se muestra la pantalla de Stage
        Time.timeScale = 0f;

        // Crear panel negro con texto
        CreateFadePanel();
        fadeImage.color = Color.black;

        // Crear texto de Stage
        CreateStageText(sceneName);

        // Mostrar texto durante 2 segundos (usar WaitForSecondsRealtime porque el tiempo está pausado)
        yield return new WaitForSecondsRealtime(2.0f);

        // Fade out del texto (solo el texto, no el panel negro)
        float fadeOutDuration = 0.5f;
        float elapsedTime = 0f;
        Color textColor = stageText.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Usar unscaledDeltaTime porque el tiempo está pausado
            float alpha = 1f - (elapsedTime / fadeOutDuration);
            stageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }

        // Destruir texto
        if (stageText != null)
        {
            Destroy(stageText.gameObject);
        }

        // Llamar al callback ANTES del fade (para iniciar la creación del mapa)
        onComplete?.Invoke();
        
        // Pequeña pausa para que se cree el mapa (usar WaitForSecondsRealtime)
        yield return new WaitForSecondsRealtime(0.1f);

        // Fade in desde negro (revelar la escena)
        yield return StartCoroutine(FadeFromBlack(1.0f));

        // REANUDAR EL JUEGO después del fade in
        Time.timeScale = 1.0f;

        isFirstTimeInLevel = false;
    }

    // Método para fade in al reiniciar nivel
    public IEnumerator FadeInOnRestart(System.Action onComplete)
    {
        // PAUSAR EL JUEGO durante el fade in
        Time.timeScale = 0f;

        CreateFadePanel();
        fadeImage.color = Color.black;

        // Llamar al callback ANTES del fade (para iniciar la creación del mapa)
        onComplete?.Invoke();
        
        // Pequeña pausa para que se cree el mapa (usar WaitForSecondsRealtime)
        yield return new WaitForSecondsRealtime(0.1f);

        // Fade in desde negro
        yield return StartCoroutine(FadeFromBlack(0.8f));

        // REANUDAR EL JUEGO después del fade in
        Time.timeScale = 1.0f;
    }

    // Fade desde negro (pantalla negra -> escena visible)
    IEnumerator FadeFromBlack(float duration)
    {
        if (fadePanel == null) CreateFadePanel();

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Usar unscaledDeltaTime porque el tiempo está pausado
            float alpha = 1f - (elapsedTime / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        
        // IMPORTANTE: Destruir panel después del fade
        CleanupFadePanel();
    }

    // Crear panel de fade
    void CreateFadePanel()
    {
        if (fadePanel != null) return;

        fadePanel = new GameObject("FadePanel_Transition");
        DontDestroyOnLoad(fadePanel);

        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // Asegurar que esté sobre todo
        
        // Añadir CanvasGroup para controlar la interacción
        CanvasGroup canvasGroup = fadePanel.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true; // Bloquea interacción durante el fade

        fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        fadeImage.raycastTarget = true; // Asegurar que bloquee clics
    }

    // Crear texto de Stage
    void CreateStageText(string sceneName)
    {
        // Extraer número de nivel
        string stageNumber = "01";
        if (sceneName.StartsWith("Level_"))
        {
            string numberPart = sceneName.Substring(6);
            if (int.TryParse(numberPart, out int levelNum))
            {
                stageNumber = levelNum.ToString("D2"); // Formato 01, 02, etc.
            }
        }

        // Crear GameObject para el texto
        GameObject textObj = new GameObject("StageText");
        textObj.transform.SetParent(fadePanel.transform, false);

        stageText = textObj.AddComponent<TextMeshProUGUI>();
        stageText.text = $"STAGE {stageNumber}";
        stageText.fontSize = 72;
        stageText.fontStyle = FontStyles.Bold;
        stageText.alignment = TextAlignmentOptions.Center;
        
        // CARGAR LA FUENTE "Electronic Highway Sign SDF
        TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
        if (customFont != null)
        {
            stageText.font = customFont;
        }
        else
        {
            Debug.LogWarning("No se encontró la fuente 'Electronic Highway Sign SDF'. Usando fuente por defecto.");
        }
        
        stageText.color = Color.white;
        
        // Aumentar el grosor de las letras (Face Dilate)
        stageText.fontSharedMaterial = new Material(stageText.fontSharedMaterial);
        stageText.fontSharedMaterial.SetFloat("_FaceDilate", 0.1f); // Más gruesas
        
        // Añadir outline grueso para efecto más redondeado
        stageText.outlineWidth = 0.25f; // Outline grueso
        stageText.outlineColor = new Color32(255, 255, 255, 255); // Outline blanco
        
        // Suavizar bordes para apariencia más redondeada
        stageText.fontSharedMaterial.SetFloat("_OutlineSoftness", 0.15f);
        
        // Espaciado entre caracteres para mejor legibilidad
        stageText.characterSpacing = 10;
        stageText.wordSpacing = 20;
        
        // Alternativa: Usar gradiente para dar sensación de volumen
        stageText.enableVertexGradient = true;
        VertexGradient gradient = new VertexGradient();
        gradient.topLeft = new Color(1f, 1f, 1f, 1f);      // Blanco arriba
        gradient.topRight = new Color(1f, 1f, 1f, 1f);
        gradient.bottomLeft = new Color(0.85f, 0.85f, 0.85f, 1f); // Gris claro abajo
        gradient.bottomRight = new Color(0.85f, 0.85f, 0.85f, 1f);
        stageText.colorGradient = gradient;

        // Posicionar en el centro
        RectTransform rectTransform = stageText.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public bool IsFirstTimeInLevel()
    {
        return isFirstTimeInLevel;
    }

    public void SetFirstTimeInLevel(bool value)
    {
        isFirstTimeInLevel = value;
    }

    // Limpiar panel si existe
    public void CleanupFadePanel()
    {
        if (fadePanel != null)
        {
            Destroy(fadePanel);
            fadePanel = null;
        }
    }
}