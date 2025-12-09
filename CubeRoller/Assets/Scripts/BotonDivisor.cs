using System.Collections;
using UnityEngine;

// Button that splits the main cube into two small cubes when pressed vertically
public class BotonDivisor : MonoBehaviour
{
    public bool isPressed = false;
    
    // Posiciones absolutas donde aparecerán los cubos pequeños (configuradas desde MapCreation)
    public Vector3 smallCube1Position;
    public Vector3 smallCube2Position;
    
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
        if (other.CompareTag("Player"))
        {
            // Verificar continuamente si el cubo está en posición vertical Y no está en movimiento
            MoveCube moveCube = other.GetComponent<MoveCube>();
            if (moveCube != null && moveCube.IsVertical() && !moveCube.IsMoving())
            {
                PressButton();
                ActivateSplit(other.transform.position);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ReleaseButton();
        }
    }
    
    void PressButton()
    {
        isPressed = true;
        Debug.Log("Botón divisor presionado!");
    }
    
    void ReleaseButton()
    {
        isPressed = false;
        Debug.Log("Botón divisor liberado!");
    }
    
    void ActivateSplit(Vector3 cubePosition)
    {
        CubeManager manager = CubeManager.Instance;
        if (manager != null && !manager.IsSplit())
        {
            // Usar las posiciones configuradas desde MapCreation
            Vector3 pos1 = smallCube1Position;
            Vector3 pos2 = smallCube2Position;
            
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
