using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LevelSwitcher : MonoBehaviour
{
    private static LevelSwitcher instance;
    
    void Awake()
    {
        // Singleton pattern - solo una instancia
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse al evento de carga de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse del evento
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    // Llamado cuando se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si estamos en el menú o créditos, destruir este singleton
        if (scene.name == "Menu" || scene.name == "Credits" || scene.name == "End")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
            Destroy(gameObject);
            return;
        }
        
        // Forzar actualización de la iluminación
        DynamicGI.UpdateEnvironment();
        
        // Resetear configuración de render
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Exposure", 1f);
        }
    }
    
    void Update()
    {
        // Detectar teclas numéricas (0-9) en el teclado principal
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            LoadLevel(1); // Tecla 0 -> Level_1
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadLevel(2); // Tecla 1 -> Level_2
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadLevel(3); // Tecla 2 -> Level_3
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadLevel(4); // Tecla 3 -> Level_4
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            LoadLevel(5); // Tecla 4 -> Level_5
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            LoadLevel(6); // Tecla 5 -> Level_6
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            LoadLevel(7); // Tecla 6 -> Level_7
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            LoadLevel(8); // Tecla 7 -> Level_8
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            LoadLevel(9); // Tecla 8 -> Level_9
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            LoadLevel(10); // Tecla 9 -> Level_10
        }
        
        // También soportar el teclado numérico
        else if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            LoadLevel(1);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            LoadLevel(2);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            LoadLevel(3);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            LoadLevel(4);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            LoadLevel(5);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            LoadLevel(6);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            LoadLevel(7);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            LoadLevel(8);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            LoadLevel(9);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            LoadLevel(10);
        }
    }
    
    void LoadLevel(int levelNumber)
    {
        string levelName = $"Level_{levelNumber}";
        
        // Verificar si el nivel existe en Build Settings
        if (LevelExists(levelName))
        {
            Debug.Log($"Cargando {levelName}...");
            
            // Opcional: Resetear el contador de movimientos al cambiar de nivel manualmente
            // MoveCounter.Instance.ResetAllProgress();
            
            // Cargar la escena de forma única (descargar escena anterior completamente)
            SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning($"El nivel {levelName} no existe o no está añadido en Build Settings.");
        }
    }
    
    // Método público para llamar desde botones de UI
    public void StartLevel1()
    {
        LoadLevel(1);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
    
    bool LevelExists(string levelName)
    {
        // Verificar si la escena existe en Build Settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneName == levelName)
            {
                return true;
            }
        }
        
        return false;
    }
}