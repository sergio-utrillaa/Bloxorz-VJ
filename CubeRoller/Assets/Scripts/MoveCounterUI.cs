using UnityEngine;
using TMPro;

public class MoveCounterUI : MonoBehaviour
{
    public TMP_Text moveCountText;
    
    void OnEnable()
    {
        // Suscribirse al evento de cambio de movimientos
        MoveCounter.OnMovesChanged += UpdateMoveCount;
        
        // Mostrar el contador inicial
        UpdateMoveCount(MoveCounter.Instance.GetTotalMoves());
    }
    
    void OnDisable()
    {
        // Desuscribirse del evento
        MoveCounter.OnMovesChanged -= UpdateMoveCount;
    }
    
    void Start()
    {
        // Solo actualizar si el texto ya está asignado manualmente
        if (moveCountText != null)
        {
            // Mostrar el contador inicial
            UpdateMoveCount(MoveCounter.Instance.GetTotalMoves());
        }
        else
        {
            Debug.LogWarning("MoveCounterUI: No se ha asignado un TMP_Text.");
        }
    }
    

    
    public void UpdateMoveCount(int moves)
    {
        if (moveCountText != null)
        {
            moveCountText.text = moves.ToString();
        }
    }
}