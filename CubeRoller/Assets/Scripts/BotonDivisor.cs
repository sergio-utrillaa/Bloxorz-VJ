using System.Collections;
using UnityEngine;

// Button that splits the main cube into two small cubes when pressed vertically
public class BotonDivisor : MonoBehaviour
{
    public bool isPressed = false;
    private bool hasTriggered = false;
    private float lastTriggerTime = 0f;
    private float triggerDelay = 0.2f;
    
    private Vector3 originalPosition;
    
    public Vector3 smallCube1SpawnOffset = new Vector3(-2, 0, 0);  // Offset relativo para el primer cubo
    public Vector3 smallCube2SpawnOffset = new Vector3(2, 0, 0);   // Offset relativo para el segundo cubo
    
    public GameObject efectoDestello;
    public Color colorActivacion = Color.yellow;
    
    void Start()
    {
        StartCoroutine(InitializePositionsAfterAnimation());
    }
    
    IEnumerator InitializePositionsAfterAnimation()
    {
        TileAnimation tileAnim = GetComponent<TileAnimation>();
        if (tileAnim != null)
        {
            yield return new WaitForSeconds(1.2f);
            tileAnim.enabled = false;
            Vector3 posicionCorrecta = new Vector3(transform.position.x, 0.0f, transform.position.z);
            transform.position = posicionCorrecta;
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }
        
        gameObject.SetActive(true);
        originalPosition = transform.position;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Solo marcar que el cubo está sobre el botón, no activar todavía
            Debug.Log("Cubo sobre el botón divisor");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && Time.time - lastTriggerTime > triggerDelay)
        {
            // Verificar continuamente si el cubo está en posición vertical Y no está en movimiento
            MoveCube moveCube = other.GetComponent<MoveCube>();
            if (moveCube != null && moveCube.IsVertical() && !moveCube.IsMoving())
            {
                hasTriggered = true;
                lastTriggerTime = Time.time;
                PressButton();
                ActivateSplit(other.transform.position);
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
        }
    }
    
    void PressButton()
    {
        isPressed = true;
        // Bajar el botón un poco
        transform.position = originalPosition + Vector3.down * 0.1f;
        Debug.Log("Botón divisor presionado!");
    }
    
    void ReleaseButton()
    {
        isPressed = false;
        transform.position = originalPosition;
        Debug.Log("Botón divisor liberado!");
    }
    
    void ActivateSplit(Vector3 cubePosition)
    {
        CubeManager manager = CubeManager.Instance;
        if (manager != null && !manager.IsSplit())
        {
            // Calcular posiciones absolutas para los cubos pequeños
            Vector3 pos1 = cubePosition + smallCube1SpawnOffset;
            Vector3 pos2 = cubePosition + smallCube2SpawnOffset;
            
            // Ajustar altura para que estén sobre el suelo
            pos1.y = 0.5f;
            pos2.y = 0.5f;
            
            manager.SplitCube(pos1, pos2);
            
            // Mostrar efecto visual
            if (efectoDestello != null)
            {
                MostrarEfectoDestello(cubePosition);
            }
            
            Debug.Log("¡Cubo dividido!");
        }
        else
        {
            Debug.Log("El cubo ya está dividido o no hay CubeManager");
        }
    }
    
    void MostrarEfectoDestello(Vector3 posicion)
    {
        GameObject efecto = Instantiate(efectoDestello, posicion, Quaternion.identity);
        
        ParticleSystem ps = efecto.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = colorActivacion;
        }
        
        Light luz = efecto.GetComponent<Light>();
        if (luz != null)
        {
            luz.color = colorActivacion;
        }
        
        Destroy(efecto, 2.0f);
    }
}
