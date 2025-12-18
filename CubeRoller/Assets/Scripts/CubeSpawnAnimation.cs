using UnityEngine;

// Animates the cube falling from above to its target position
public class CubeSpawnAnimation : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float animationTime = 0f;
    private float animationDuration;
    private bool isAnimating = false;
    private MoveCube moveCubeComponent;
    
    // Audio de spawn
    public AudioClip spawnSound;
    
    [Range(0f, 3f)]
    [Tooltip("Volumen del sonido de spawn")]
    public float spawnSoundVolume = 1.5f;

    private bool hasPlayedSound = false; // Para controlar la reproducción del sonido

    public void StartFallAnimation(float duration, float startHeight)
    {
        targetPosition = transform.position;
        startPosition = new Vector3(targetPosition.x, startHeight, targetPosition.z);
        transform.position = startPosition;
        
        animationDuration = duration;
        animationTime = 0f;
        isAnimating = true;
        
        // Disable MoveCube component during spawn animation
        moveCubeComponent = GetComponent<MoveCube>();
        if (moveCubeComponent != null)
        {
            moveCubeComponent.enabled = false;
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            float t = Mathf.Clamp01(animationTime / animationDuration);
            
            // Ease in curve for falling effect (accelerates)
            t = t * t;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            if (t >= 1f)
            {
                isAnimating = false;
                
                // Reproducir sonido cuando toca el suelo
                if (spawnSound != null && !hasPlayedSound)
                {
                    AudioSource.PlayClipAtPoint(spawnSound, transform.position, spawnSoundVolume);
                    Debug.Log($"Reproduciendo sonido de spawn al tocar el suelo en {transform.position}");
                    hasPlayedSound = true;
                }
                
                // Re-enable MoveCube component after animation
                if (moveCubeComponent != null)
                {
                    moveCubeComponent.enabled = true;
                }
                
                // Destroy this component after animation
                Destroy(this);
            }
        }
    }
}
