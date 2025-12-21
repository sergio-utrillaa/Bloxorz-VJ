using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MoveCounter : MonoBehaviour
{
    private static MoveCounter instance;
    
    private int currentLevelMoves = 0;  // Movimientos en el nivel actual
    private int totalMoves = 0;         // Movimientos totales acumulados
    private int levelStartMoves = 0;    // Movimientos con los que empezó el nivel actual
    
    // Evento que se dispara cuando cambia el contador
    public static event Action<int> OnMovesChanged;
    
    public static MoveCounter Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("MoveCounter");
                instance = go.AddComponent<MoveCounter>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse a cambios de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si estamos en el menú o créditos, destruir este singleton
        if (scene.name == "Menu" || scene.name == "Credits")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
            Destroy(gameObject);
        }
    }
    
    // Incrementar contador de movimientos
    public void AddMove()
    {
        currentLevelMoves++;
        totalMoves++;
        
        // Notificar a todos los listeners
        OnMovesChanged?.Invoke(totalMoves);
        
        Debug.Log($"Movimiento registrado. Nivel: {currentLevelMoves}, Total: {totalMoves}");
    }
    
    // Obtener movimientos totales
    public int GetTotalMoves()
    {
        return totalMoves;
    }
    
    // Obtener movimientos del nivel actual
    public int GetCurrentLevelMoves()
    {
        return currentLevelMoves;
    }
    
    // Reiniciar nivel (volver a los movimientos con los que empezó)
    public void RestartLevel()
    {
        totalMoves = levelStartMoves;
        currentLevelMoves = 0;
        
        // Notificar a todos los listeners
        OnMovesChanged?.Invoke(totalMoves);
        
        Debug.Log($"Nivel reiniciado. Movimientos totales restaurados a: {totalMoves}");
    }
    
    // Avanzar al siguiente nivel (guardar progreso)
    public void CompleteLevel()
    {
        levelStartMoves = totalMoves;
        currentLevelMoves = 0;
        
        Debug.Log($"Nivel completado. Próximo nivel empezará con {levelStartMoves} movimientos");
    }
    
    // Resetear todo (para empezar desde el nivel 1)
    public void ResetAllProgress()
    {
        currentLevelMoves = 0;
        totalMoves = 0;
        levelStartMoves = 0;
        
        // Notificar a todos los listeners
        OnMovesChanged?.Invoke(totalMoves);
        
        Debug.Log("Progreso completo reseteado");
    }
}