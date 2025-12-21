using System.Collections;
using UnityEngine;

public class OrangeTileBehavior : MonoBehaviour
{
    private bool isFalling = false;
    public AudioClip deathSound;    // Sonido cuando el cubo cae de la plataforma
    [Range(0f, 10.0f)]
    [Tooltip("Volumen del sonido de muerte")]
    public float deathSoundVolume = 10.0f;
    
    void Start()
    {
        // Asegurarse de que el tile naranja tiene un BoxCollider como trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"BoxCollider añadido a {gameObject.name}");
        }
        
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(1.0f, 2.0f, 1.0f);
        boxCollider.center = new Vector3(0f, 1.0f, 0f);
        
        Debug.Log($"OrangeTileBehavior inicializado en {gameObject.name}. Collider: isTrigger={boxCollider.isTrigger}, size={boxCollider.size}, center={boxCollider.center}");
    }
    
    void Update()
    {
        // Verificación cada frame
        if (!isFalling)
        {
            CheckForVerticalCube();
        }
    }
    
    void CheckForVerticalCube()
    {
        // Usar OverlapBox en la posición del tile
        Vector3 boxCenter = transform.position + new Vector3(0f, 0.5f, 0f);
        Vector3 boxSize = new Vector3(0.8f, 1.5f, 0.8f); // Área de detección más ajustada
        
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2f, Quaternion.identity);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                MoveCube moveCube = col.GetComponent<MoveCube>();
                
                if (moveCube != null)
                {
                    // Verificar si está vertical Y no se está moviendo
                    bool isVertical = moveCube.IsVertical();
                    bool isMoving = moveCube.IsMoving();
                    
                    // Debug detallado
                    Debug.Log($"Cubo detectado sobre tile naranja {gameObject.name}. Vertical: {isVertical}, Moving: {isMoving}");
                    
                    if (isVertical && !isMoving)
                    {
                        // Verificación adicional: el cubo debe estar exactamente sobre este tile
                        Vector3 cubePos = moveCube.transform.position;
                        Vector3 tilePos = transform.position;
                        
                        float distX = Mathf.Abs(cubePos.x - tilePos.x);
                        float distZ = Mathf.Abs(cubePos.z - tilePos.z);
                        
                        Debug.Log($"Distancias - X: {distX}, Z: {distZ}");
                        
                        // Solo activar si está muy cerca (tolerancia de 0.3)
                        if (distX < 0.3f && distZ < 0.3f)
                        {
                            Debug.Log($"¡Cubo VERTICAL y QUIETO detectado DIRECTAMENTE sobre tile naranja {gameObject.name}!");
                            StartFallingWithCube(moveCube);
                            return;
                        }
                        else
                        {
                            Debug.Log($"Cubo vertical pero NO centrado sobre el tile naranja. Distancias: X={distX}, Z={distZ}");
                        }
                    }
                }
            }
        }
    }
    
    void StartFallingWithCube(MoveCube cube)
    {
        if (isFalling) return;
        
        isFalling = true;
        
        Debug.Log($"=== INICIANDO CAÍDA CONJUNTA ===");
        Debug.Log($"Tile naranja: {gameObject.name}");
        Debug.Log($"Cubo: {cube.name}");
        
        // Hacer que el cubo caiga
        cube.StartOrangeTileFall();
        
        // Hacer que el tile naranja también caiga
        StartCoroutine(FallAnimation());

        StartCoroutine(waitDeathSound());
    }

    IEnumerator waitDeathSound()
    {
        yield return new WaitForSeconds(0.7f);

        // Reproducir sonido de muerte ANTES de iniciar las animaciones
        PlayDeathSound();
    }
    
    // ✨ NUEVO: Método para reproducir el sonido de muerte
    void PlayDeathSound()
    {
        if (deathSound != null)
        {
            // Crear un AudioSource temporal con configuración robusta
            GameObject tempAudio = new GameObject("OrangeTileDeathSound");
            tempAudio.transform.position = transform.position;
            
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            audioSource.clip = deathSound;
            audioSource.volume = 1f; // Siempre usar volumen máximo del AudioSource
            audioSource.spatialBlend = 0f; // 2D para que se escuche bien en todas partes
            audioSource.playOnAwake = false;
            audioSource.Play();
            
            Debug.Log($"✅ [OrangeTileBehavior] Reproduciendo sonido de muerte: {deathSound.name}, Volumen: {audioSource.volume}, Clip length: {deathSound.length}s");
            
            // Destruir el GameObject después de que termine el sonido
            Destroy(tempAudio, deathSound.length + 0.5f);
        }
        else
        {
            Debug.LogError("❌ [OrangeTileBehavior] No hay sonido de muerte asignado!");
        }
    }
    
    IEnumerator FallAnimation()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - 10.0f, startPosition.z);
        float duration = 0.8f; // Misma duración que el cubo
        float elapsedTime = 0f;
        
        Debug.Log($"Tile {gameObject.name} comenzando animación de caída desde {startPosition}");
        
        // Sin pausa - caer inmediatamente junto con el cubo
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Aceleración (como gravedad) - misma curva que el cubo
            t = t * t;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        Debug.Log($"Tile {gameObject.name} terminó caída. Destruyendo...");
        
        // Destruir el tile después de caer
        Destroy(gameObject);
    }
    
    // Visualizar el área de detección en el Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 boxCenter = transform.position + new Vector3(0f, 0.5f, 0f);
        Vector3 boxSize = new Vector3(0.8f, 1.5f, 0.8f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}