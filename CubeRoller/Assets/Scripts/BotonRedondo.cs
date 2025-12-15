using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotonRedondo : MonoBehaviour
{
    public bool isPressed = false;
    private bool hasTriggered = false;
    private float lastTriggerTime = 0f;
    private float triggerDelay = 0.2f;

    private Vector3 originalPosition;

    public GameObject[] puentesControlados; // Array de puentes que este botón controla
    public bool toggleMode = true; // Si true, alterna activación/desactivación; si false, solo activa mientras está presionado
    private bool puentesActivos = false;

    public GameObject efectoDestello;
    public Color colorActivacion = Color.green;
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

    IEnumerator InitializePositionsAfterAnimation() {
        // Verificar y deshabilitar el componente TileAnimation si existe
        TileAnimation tileAnim = GetComponent<TileAnimation>();
        if (tileAnim != null)
        {
            Debug.Log($"TileAnimation encontrado en botón {name}, deshabilitando componente");
            // Esperar hasta que la animación de TileAnimation termine
            yield return new WaitForSeconds(1.2f);

            tileAnim.enabled = false; // Deshabilitar en lugar de destruir

            Vector3 posicionCorrecta = new Vector3(transform.position.x, 0.0f, transform.position.z);
            transform.position = posicionCorrecta;
            Debug.Log($"Posición del botón corregida a: {posicionCorrecta}");
        }
        else {
            // Si no hay TileAnimation, esperar solo un frame
            yield return new WaitForEndOfFrame();
        }
        
        // Asegurar que el botón esté visible y activo
        gameObject.SetActive(true);
        
        // Ahora establecer las posiciones basadas en la posición actual
        originalPosition = transform.position;
        
        Debug.Log($"Botón {name} inicializado en posición: {originalPosition}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && Time.time - lastTriggerTime > triggerDelay)
        {
            hasTriggered = true;
            lastTriggerTime = Time.time; // Guardar el tiempo del trigger
            PressButton();
            
            if (toggleMode)
            {
                // En modo toggle, cambiar estado solo una vez al entrar
                TogglePuentes();
            }
            else
            {
                // En modo mantener, activar mientras esté presionado
                SetPuentesState(true, true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hasTriggered && Time.time - lastTriggerTime > triggerDelay)
        {
            hasTriggered = false;
            lastTriggerTime = Time.time; // Guardar el tiempo del trigger
            ReleaseButton();
            
            if (!toggleMode)
            {
                // En modo mantener, desactivar al salir
                SetPuentesState(false, true);
            }
        }
    }
    
    void PressButton()
    {
        isPressed = true;
        
        // ✨ NUEVO: Reproducir sonido al presionar
        if (buttonPressSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonPressSound, transform.position, soundVolume);
        }
        
        Debug.Log("Botón presionado!");
    }
    
    void ReleaseButton()
    {
        isPressed = false;
        transform.position = originalPosition;
        Debug.Log("Botón liberado!");
    }

    void TogglePuentes()
    {
        puentesActivos = !puentesActivos;
        Debug.Log($"Cambiando estado de puentes a: {puentesActivos}");
        SetPuentesState(puentesActivos, true);
    }
    
    void SetPuentesState(bool activo, bool mostrarEfecto)
    {
        if (puentesControlados == null || puentesControlados.Length == 0)
        {
            Debug.LogWarning("No hay puentes controlados asignados al botón");
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
            if (puente != null && puente != this.gameObject)
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
                
                Debug.Log($"Puente {puente.name} alternado");
                
                // Mostrar efecto de destello si está habilitado
                if (mostrarEfecto && efectoDestello != null)
                {
                    MostrarEfectoDestello(puente.transform.position, activo);
                }
            }
        }
        
        Debug.Log($"Puentes alternados!");
    }
    
    void MostrarEfectoDestello(Vector3 posicion, bool activando)
    {
        // Crear el efecto de destello en la posición del puente
        GameObject efecto = Instantiate(efectoDestello, posicion, Quaternion.identity);
        
        // Cambiar el color del efecto según si se activa o desactiva
        ParticleSystem ps = efecto.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = activando ? colorActivacion : colorDesactivacion;
        }
        
        // Cambiar color de cualquier Light component
        Light luz = efecto.GetComponent<Light>();
        if (luz != null)
        {
            luz.color = activando ? colorActivacion : colorDesactivacion;
        }
        
        // Destruir el efecto después de 2 segundos
        Destroy(efecto, 2.0f);
    }
}
