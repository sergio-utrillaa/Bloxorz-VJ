using UnityEngine;

// Handles fade out animation for cubes using scale reduction
public class CubeFadeOut : MonoBehaviour
{
    private bool isFading = false;
    private float fadeTime = 0f;
    private float fadeDuration = 0.5f;
    private Vector3 originalScale;
    private Renderer[] renderers;
    private Material[][] fadeMaterials;
    
    public void StartFadeOut(float duration = 0.5f)
    {
        if (isFading) return;
        
        isFading = true;
        fadeDuration = duration;
        fadeTime = 0f;
        originalScale = transform.localScale;
        
        // Get all renderers and create fade materials
        renderers = GetComponentsInChildren<Renderer>();
        fadeMaterials = new Material[renderers.Length][];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            fadeMaterials[i] = new Material[mats.Length];
            
            for (int j = 0; j < mats.Length; j++)
            {
                // Create a new material instance
                fadeMaterials[i][j] = new Material(mats[j]);
                
                // Try to set to transparent mode for different shader types
                Material mat = fadeMaterials[i][j];
                
                // For URP Lit shader
                if (mat.HasProperty("_Surface"))
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha
                }
                
                // For Built-in Standard shader
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3); // Transparent
                    mat.SetOverrideTag("RenderType", "Transparent");
                }
                
                // Common transparency settings
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            
            renderers[i].materials = fadeMaterials[i];
        }
        
        Debug.Log($"CubeFadeOut iniciado en {gameObject.name}, duración: {duration}s");
    }
    
    void Update()
    {
        if (isFading)
        {
            fadeTime += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTime / fadeDuration);
            
            // Ease out curve for smoother fade
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            
            // Scale down the cube while fading
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.1f, easedT);
            
            // Update alpha of all materials
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && fadeMaterials[i] != null)
                {
                    for (int j = 0; j < fadeMaterials[i].Length; j++)
                    {
                        Material mat = fadeMaterials[i][j];
                        if (mat != null)
                        {
                            Color color = mat.color;
                            color.a = Mathf.Lerp(1f, 0f, easedT);
                            mat.color = color;
                            
                            // Also set _BaseColor for URP
                            if (mat.HasProperty("_BaseColor"))
                            {
                                mat.SetColor("_BaseColor", color);
                            }
                        }
                    }
                }
            }
            
            // Destroy when fade is complete
            if (t >= 1f)
            {
                Debug.Log($"Fade completado, destruyendo {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up created materials
        if (fadeMaterials != null)
        {
            foreach (Material[] mats in fadeMaterials)
            {
                if (mats != null)
                {
                    foreach (Material mat in mats)
                    {
                        if (mat != null)
                        {
                            Destroy(mat);
                        }
                    }
                }
            }
        }
    }
}
