using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("MusicManager");
                instance = obj.AddComponent<MusicManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    [Header("Música por Ambiente")]
    [Tooltip("Música para el menú principal")]
    public AudioClip menuMusic;
    
    [Tooltip("Música para niveles de bosque (Level_1, Level_2)")]
    public AudioClip bosqueMusic;
    
    [Tooltip("Música para niveles de playa (Level_3, Level_4)")]
    public AudioClip playaMusic;
    
    [Tooltip("Música para niveles de desierto (Level_5, Level_6)")]
    public AudioClip desiertoMusic;
    
    [Tooltip("Música para niveles de nieve (Level_7, Level_8)")]
    public AudioClip nieveMusic;
    
    [Tooltip("Música para niveles de fuego (Level_9, Level_10)")]
    public AudioClip fuegoMusic;
    
    [Tooltip("Música para la pantalla final (End)")]
    public AudioClip endMusic;
    
    [Header("Configuración de Audio")]
    [Range(0f, 1f)]
    [Tooltip("Volumen de la música")]
    public float musicVolume = 0.5f;
    
    [Tooltip("Duración del fade entre canciones (segundos)")]
    public float fadeDuration = 1.0f;

    private AudioSource audioSource;
    private string currentSceneName;
    private bool isFading = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Crear AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;  // Activar loop para que se repita automáticamente
            audioSource.volume = musicVolume;
            audioSource.playOnAwake = false;
            
            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusicForCurrentScene();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Método llamado cuando se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    // Método para reproducir música según el nivel actual
    public void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Evitar cambiar música si ya está sonando la correcta
        if (sceneName == currentSceneName && audioSource.isPlaying)
            return;
        
        currentSceneName = sceneName;
        
        AudioClip newClip = GetMusicForScene(sceneName);
        
        // Cambiar música con fade si es diferente
        if (newClip != null && newClip != audioSource.clip)
        {
            if (isFading)
            {
                StopAllCoroutines();
            }
            StartCoroutine(FadeToNewMusic(newClip));
        }
        else if (newClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = newClip;
            audioSource.loop = true; // Asegurar que loop está activado
            audioSource.Play();
        }
    }

    // Determinar qué música reproducir según la escena
    AudioClip GetMusicForScene(string sceneName)
    {
        // Menú principal
        if (sceneName == "MainMenu" || sceneName == "Menu")
        {
            Debug.Log("Reproduciendo música de menú");
            return menuMusic;
        }
        
        // Niveles de bosque (1-2)
        if (sceneName == "Level_1" || sceneName == "Level_2")
        {
            Debug.Log("Reproduciendo música de bosque");
            return bosqueMusic;
        }
        
        // Niveles de playa (3-4)
        if (sceneName == "Level_3" || sceneName == "Level_4")
        {
            Debug.Log("Reproduciendo música de playa");
            return playaMusic;
        }
        
        // Niveles de desierto (5-6)
        if (sceneName == "Level_5" || sceneName == "Level_6")
        {
            Debug.Log("Reproduciendo música de desierto");
            return desiertoMusic;
        }
        
        // Niveles de nieve (7-8)
        if (sceneName == "Level_7" || sceneName == "Level_8")
        {
            Debug.Log("Reproduciendo música de nieve");
            return nieveMusic;
        }
        
        // Niveles de fuego (9-10)
        if (sceneName == "Level_9" || sceneName == "Level_10")
        {
            Debug.Log("Reproduciendo música de fuego");
            return fuegoMusic;
        }
        
        // Pantalla final
        if (sceneName == "End")
        {
            Debug.Log("Reproduciendo música de la pantalla final");
            return endMusic;
        }
        
        // Si hay más niveles, usar música por defecto o repetir ciclo
        if (sceneName.StartsWith("Level_"))
        {
            Debug.LogWarning($"No hay música específica para {sceneName}, sin música");
            return null;
        }
        
        return null;
    }

    // Corrutina para hacer fade entre canciones
    System.Collections.IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        isFading = true;
        
        // Fade out
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration / 2)
            {
                elapsedTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / (fadeDuration / 2));
                yield return null;
            }
            
            audioSource.Stop();
        }
        
        // Cambiar clip y asegurar que loop está activado
        audioSource.clip = newClip;
        audioSource.loop = true; // Asegurar que loop está activado antes de reproducir
        audioSource.Play();
        
        // Fade in
        float targetVolume = musicVolume;
        float elapsedTime2 = 0f;
        
        while (elapsedTime2 < fadeDuration / 2)
        {
            elapsedTime2 += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime2 / (fadeDuration / 2));
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        isFading = false;
    }

    // Método para cambiar el volumen
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isFading)
        {
            audioSource.volume = musicVolume;
        }
    }

    // Método para pausar la música
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    // Método para reanudar la música
    public void ResumeMusic()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.UnPause();
        }
    }

    // Método para detener la música
    public void StopMusic()
    {
        audioSource.Stop();
    }
    
    // Método para verificar el estado del loop
    public bool IsLooping()
    {
        return audioSource != null && audioSource.loop;
    }
    
    // Método para forzar el loop si por alguna razón se desactiva
    public void EnsureLooping()
    {
        if (audioSource != null)
        {
            audioSource.loop = true;
            Debug.Log($"Loop forzado a true. Música actual: {(audioSource.clip != null ? audioSource.clip.name : "ninguna")}");
        }
    }
}