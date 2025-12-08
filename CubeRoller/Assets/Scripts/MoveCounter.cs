using UnityEngine;

public class MoveCounter : MonoBehaviour
{
    private static MoveCounter instance;
    
    private int currentLevelMoves = 0;  // Movimientos en el nivel actual
    private int totalMoves = 0;         // Movimientos totales acumulados
    private int levelStartMoves = 0;    // Movimientos con los que empezó el nivel actual
    
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Incrementar contador de movimientos
    public void AddMove()
    {
        currentLevelMoves++;
        totalMoves++;
        
        // Actualizar UI
        MoveCounterUI ui = FindObjectOfType<MoveCounterUI>();
        if (ui != null)
        {
            ui.UpdateMoveCount(totalMoves);
        }
        
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
        
        // Actualizar UI
        MoveCounterUI ui = FindObjectOfType<MoveCounterUI>();
        if (ui != null)
        {
            ui.UpdateMoveCount(totalMoves);
        }
        
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
        
        // Actualizar UI
        MoveCounterUI ui = FindObjectOfType<MoveCounterUI>();
        if (ui != null)
        {
            ui.UpdateMoveCount(totalMoves);
        }
        
        Debug.Log("Progreso completo reseteado");
    }
}