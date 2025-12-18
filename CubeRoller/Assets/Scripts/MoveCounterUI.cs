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
        // Si no hay texto asignado, buscar en hijos
        if (moveCountText == null)
        {
            moveCountText = GetComponentInChildren<TMP_Text>();
        }
        
        // Mostrar el contador inicial
        UpdateMoveCount(MoveCounter.Instance.GetTotalMoves());
    }
    

    
    public void UpdateMoveCount(int moves)
    {
        if (moveCountText != null)
        {
            moveCountText.text = moves.ToString();
        }
    }
}