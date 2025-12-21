using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    private static MenuAudioManager instance;
    
    private AudioSource audioSource;
    
    [Header("Sonidos del Menú")]
    public AudioClip menuPassSound;
    public AudioClip menuSelectSound;
    
    [Header("Volúmenes")]
    [Range(0f, 1f)]
    public float passVolume = 0.5f;
    
    [Range(0f, 1f)]
    public float selectVolume = 0.7f;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            // Crear el AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D
            
            Debug.Log("[MenuAudioManager] Inicializado correctamente");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void PlayPassSound()
    {
        if (instance != null && instance.menuPassSound != null)
        {
            instance.audioSource.PlayOneShot(instance.menuPassSound, instance.passVolume);
            Debug.Log($"[MenuAudioManager] Reproduciendo menuPassSound - Volume: {instance.passVolume}");
        }
        else
        {
            Debug.LogWarning($"[MenuAudioManager] No se puede reproducir menuPassSound. Instance: {instance != null}, Clip: {instance?.menuPassSound != null}");
        }
    }
    
    public static void PlaySelectSound()
    {
        if (instance != null && instance.menuSelectSound != null)
        {
            instance.audioSource.PlayOneShot(instance.menuSelectSound, instance.selectVolume);
            Debug.Log($"[MenuAudioManager] Reproduciendo menuSelectSound - Volume: {instance.selectVolume}");
        }
        else
        {
            Debug.LogWarning($"[MenuAudioManager] No se puede reproducir menuSelectSound. Instance: {instance != null}, Clip: {instance?.menuSelectSound != null}");
        }
    }
}