using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotonCruz : MonoBehaviour
{
    public bool isPressed = false;
    private bool hasTriggered = false;
    private float lastTriggerTime = 0f;
    private float triggerDelay = 0.2f;
    
    private Vector3 originalPosition;
    private MoveCube playerCube; // Referencia al script MoveCube del jugador

    public GameObject[] puentesControlados; // Array de puentes que este botón controla
    public bool toggleMode = true; // Si true, alterna activación/desactivación
    private bool puentesActivos = false;

    public GameObject efectoDestello; // Prefab del efecto de destello
    public Color colorActivacion = Color.blue; // Color diferente para distinguir de BotonRedondo
    public Color colorDesactivacion = Color.red;
    
    // Sonidos
    public AudioClip buttonPressSound;
    public AudioClip bridgeActivateSound;   // ✨ NUEVO: Sonido al activar puente
    public AudioClip bridgeDeactivateSound; // ✨ NUEVO: Sonido al desactivar puente
    public float soundVolume = 1.0f;
    public float bridgeSoundVolume = 1.0f;  // ✨ NUEVO: Volumen para sonidos de puente
    
    void Start()
    {
        // Esperar un frame para que la animación de tile establezca la posición final
        StartCoroutine(InitializePositionsAfterAnimation());
    }

    IEnumerator InitializePositionsAfterAnimation()
    {
        // Verificar y deshabilitar el componente TileAnimation si existe
        TileAnimation tileAnim = GetComponent<TileAnimation>();
        if (tileAnim != null)
        {
            Debug.Log($"TileAnimation encontrado en botón cruz {name}, esperando animación");
            yield return new WaitForSeconds(1.29f);
            
            // Siempre corregir posición Y a 0
            Vector3 posicionCorrecta = new Vector3(transform.position.x, 0.0f, transform.position.z);
            transform.position = posicionCorrecta;
            Debug.Log($"Posición del botón cruz corregida a: {posicionCorrecta}");
            
            tileAnim.enabled = false;
            Debug.Log($"TileAnimation deshabilitado para botón cruz {name}");
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }
        
        // Asegurar que el botón esté visible y activo
        gameObject.SetActive(true);
        
        // Establecer la posición original
        originalPosition = transform.position;
        Debug.Log($"Botón cruz {name} inicializado en posición: {originalPosition}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && Time.time - lastTriggerTime > triggerDelay)
        {
            // ✨ NUEVO: Verificar que NO sea un cubo pequeño
            MoveCubeSmall smallCube = other.GetComponent<MoveCubeSmall>();
            if (smallCube != null)
            {
                Debug.Log("Cubo pequeño detectado en botón cruz - NO ACTIVAR");
                return; // Los cubos pequeños no pueden activar botones cruz
            }
            
            // Obtener referencia al MoveCube si no la tenemos
            if (playerCube == null)
            {
                playerCube = other.GetComponent<MoveCube>();
            }
            
            // Solo activar si el cubo está en posición vertical
            if (playerCube != null && playerCube.IsVertical())
            {
                hasTriggered = true;
                lastTriggerTime = Time.time;
                PressButton();
                
                if (toggleMode)
                {
                    TogglePuentes();
                }
                else
                {
                    SetPuentesState(true, true);
                }
                
                Debug.Log($"Botón cruz activado - Cubo en posición vertical");
            }
            else
            {
                Debug.Log($"Botón cruz NO activado - Cubo no está en posición vertical");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hasTriggered && Time.time - lastTriggerTime > triggerDelay)
        {
            hasTriggered = false;
            lastTriggerTime = Time.time;
            ReleaseButton();
            
            if (!toggleMode)
            {
                // En modo mantener, desactivar al salir
                SetPuentesState(false, true);
            }
            // En modo toggle, NO hacer nada - el estado se mantiene
        }
    }
    
    void PressButton()
    {
        isPressed = true;
        
        // ✨ NUEVO: Reproducir sonido al presionar (solo cuando está en vertical)
        if (buttonPressSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonPressSound, transform.position, soundVolume);
        }
        
        Debug.Log("Botón cruz presionado!");
    }
    
    void ReleaseButton()
    {
        isPressed = false;
        Debug.Log("Botón cruz liberado!");
    }

    void TogglePuentes()
    {
        puentesActivos = !puentesActivos;
        Debug.Log($"Cambiando estado de puentes (cruz) a: {puentesActivos}");
        SetPuentesState(puentesActivos, true);
    }
    
    void SetPuentesState(bool activo, bool mostrarEfecto)
    {
        if (puentesControlados == null || puentesControlados.Length == 0)
        {
            Debug.LogWarning("No hay puentes controlados asignados al botón cruz");
            return;
        }
        
        // ✨ NUEVO: Reproducir sonido de activación/desactivación de puente
        if (activo && bridgeActivateSound != null)
        {
            AudioSource.PlayClipAtPoint(bridgeActivateSound, transform.position, bridgeSoundVolume);
        }
        else if (!activo && bridgeDeactivateSound != null)
        {
            AudioSource.PlayClipAtPoint(bridgeDeactivateSound, transform.position, bridgeSoundVolume);
        }
        
        foreach (GameObject puente in puentesControlados)
        {
            if (puente != null && puente != this.gameObject && puente.name != this.name)
            {
                // Activar el puente primero (si estaba desactivado)
                if (!puente.activeInHierarchy)
                {
                    puente.SetActive(true);
                }
                
                // Obtener o añadir el componente de animación
                BridgeRiseAnimation riseAnim = puente.GetComponent<BridgeRiseAnimation>();
                if (riseAnim == null)
                {
                    riseAnim = puente.AddComponent<BridgeRiseAnimation>();
                }
                
                // Alternar el estado del puente
                riseAnim.Toggle();
                
                Debug.Log($"Puente {puente.name} alternado por botón cruz");
                
                // Mostrar efecto de destello si está habilitado
                if (mostrarEfecto && efectoDestello != null)
                {
                    MostrarEfectoDestello(puente.transform.position, activo);
                }
            }
        }
        
        Debug.Log($"Puentes alternados por botón cruz!");
    }
    
    void MostrarEfectoDestello(Vector3 posicion, bool activando)
    {
        if (efectoDestello != null)
        {
            GameObject efecto = Instantiate(efectoDestello, posicion, Quaternion.identity);
            
            // Configurar color según si está activando o desactivando
            ParticleSystem ps = efecto.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = activando ? colorActivacion : colorDesactivacion;
            }
            
            // Configurar luz si existe
            Light luz = efecto.GetComponent<Light>();
            if (luz != null)
            {
                luz.color = activando ? colorActivacion : colorDesactivacion;
            }
            
            // Destruir el efecto después de un tiempo
            Destroy(efecto, 2.0f);
        }
    }
}
