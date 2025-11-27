using UnityEngine;

// Manages the state of cubes: main cube, split cubes, and switching between them
public class CubeManager : MonoBehaviour
{
    public static CubeManager Instance { get; private set; }
    
    public GameObject mainCubePrefab;           // Prefab del cubo principal (1x2x1)
    public GameObject smallCubePrefab;          // Prefab del cubo pequeño (1x1x1)
    
    private GameObject mainCube;                // Cubo principal activo
    private GameObject smallCube1;              // Primer cubo pequeño
    private GameObject smallCube2;              // Segundo cubo pequeño
    
    private GameObject activeControlledCube;    // Cubo que está siendo controlado actualmente
    private bool isSplit = false;               // ¿Está el cubo dividido?
    
    public float mergeCheckDistance = 1.5f;     // Distancia para verificar unión
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find the main cube in the scene
        mainCube = GameObject.FindGameObjectWithTag("Player");
        if (mainCube != null)
        {
            activeControlledCube = mainCube;
        }
    }
    
    void Update()
    {
        // Switch between cubes with Space key
        if (isSplit && Input.GetKeyDown(KeyCode.Space))
        {
            SwitchControlledCube();
        }
        
        // Check for merge when split
        if (isSplit)
        {
            CheckForMerge();
        }
    }
    
    public void SplitCube(Vector3 position1, Vector3 position2)
    {
        if (isSplit || mainCube == null) return;
        
        // Desactivar el cubo principal
        mainCube.SetActive(false);
        
        // Crear dos cubos pequeños
        smallCube1 = Instantiate(smallCubePrefab, position1, Quaternion.identity);
        smallCube2 = Instantiate(smallCubePrefab, position2, Quaternion.identity);
        
        smallCube1.tag = "Player";
        smallCube2.tag = "Player";
        
        // Controlar el primer cubo por defecto
        activeControlledCube = smallCube1;
        SetCubeControl(smallCube1, true);
        SetCubeControl(smallCube2, false);
        
        isSplit = true;
        
        Debug.Log("Cubo dividido en dos cubos pequeños");
    }
    
    private void SwitchControlledCube()
    {
        if (smallCube1 == null || smallCube2 == null) return;
        
        // Cambiar control entre los dos cubos
        if (activeControlledCube == smallCube1)
        {
            SetCubeControl(smallCube1, false);
            SetCubeControl(smallCube2, true);
            activeControlledCube = smallCube2;
            Debug.Log("Controlando cubo 2");
        }
        else
        {
            SetCubeControl(smallCube2, false);
            SetCubeControl(smallCube1, true);
            activeControlledCube = smallCube1;
            Debug.Log("Controlando cubo 1");
        }
    }
    
    private void SetCubeControl(GameObject cube, bool enabled)
    {
        if (cube == null) return;
        
        MoveCubeSmall moveScript = cube.GetComponent<MoveCubeSmall>();
        if (moveScript != null)
        {
            moveScript.enabled = enabled;
        }
        
        // Cambiar visual para indicar cuál está activo (opcional)
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Hacer el cubo activo más brillante
            if (enabled)
            {
                renderer.material.color = Color.white;
            }
            else
            {
                renderer.material.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }
    
    private void CheckForMerge()
    {
        if (smallCube1 == null || smallCube2 == null) return;
        
        float distance = Vector3.Distance(smallCube1.transform.position, smallCube2.transform.position);
        
        // Si están adyacentes (aproximadamente 1 unidad de distancia)
        if (distance <= mergeCheckDistance)
        {
            // Verificar si están en posiciones adyacentes válidas
            Vector3 pos1 = smallCube1.transform.position;
            Vector3 pos2 = smallCube2.transform.position;
            
            Vector3 diff = pos2 - pos1;
            
            // Redondear para verificar si están exactamente adyacentes
            int dx = Mathf.RoundToInt(diff.x);
            int dy = Mathf.RoundToInt(diff.y);
            int dz = Mathf.RoundToInt(diff.z);
            
            // Deben estar en el mismo nivel (y) y adyacentes en x o z
            if (dy == 0 && ((Mathf.Abs(dx) == 1 && dz == 0) || (Mathf.Abs(dz) == 1 && dx == 0)))
            {
                MergeCubes(pos1, pos2, dx, dz);
            }
        }
    }
    
    private void MergeCubes(Vector3 pos1, Vector3 pos2, int dx, int dz)
    {
        // Calcular posición y rotación del cubo fusionado
        Vector3 mergePosition = (pos1 + pos2) / 2f;
        Quaternion mergeRotation = Quaternion.identity;
        
        // Si están alineados en X, el cubo debe estar horizontal en X
        if (dx != 0)
        {
            mergeRotation = Quaternion.Euler(0, 0, 90);
        }
        // Si están alineados en Z, el cubo debe estar horizontal en Z
        else if (dz != 0)
        {
            mergeRotation = Quaternion.Euler(90, 0, 0);
        }
        
        // Destruir los cubos pequeños
        Destroy(smallCube1);
        Destroy(smallCube2);
        smallCube1 = null;
        smallCube2 = null;
        
        // Reactivar el cubo principal
        mainCube.transform.position = mergePosition;
        mainCube.transform.rotation = mergeRotation;
        mainCube.SetActive(true);
        
        // Reiniciar el estado del cubo principal
        MoveCube moveScript = mainCube.GetComponent<MoveCube>();
        if (moveScript != null)
        {
            moveScript.ResetCube();
        }
        
        activeControlledCube = mainCube;
        isSplit = false;
        
        Debug.Log("Cubos unidos de nuevo!");
    }
    
    public bool IsSplit()
    {
        return isSplit;
    }
}
