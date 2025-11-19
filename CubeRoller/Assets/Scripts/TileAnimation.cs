using UnityEngine;

// Animates a tile rising from below to its final position
public class TileAnimation : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float animationTime = 0f;
    private float animationDuration;
    private bool isAnimating = false;

    public void StartAnimation(float delay, float duration, float startHeight)
    {
        targetPosition = transform.position;
        startPosition = new Vector3(targetPosition.x, startHeight, targetPosition.z);
        transform.position = startPosition;
        
        animationDuration = duration;
        animationTime = -delay; // Negative time acts as delay
        isAnimating = true;
    }

    void Update()
    {
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            
            if (animationTime >= 0f)
            {
                float t = Mathf.Clamp01(animationTime / animationDuration);
                
                // Ease out curve for smoother animation
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                
                if (t >= 1f)
                {
                    isAnimating = false;
                    // Optionally destroy this component after animation
                    Destroy(this);
                }
            }
        }
    }
}
