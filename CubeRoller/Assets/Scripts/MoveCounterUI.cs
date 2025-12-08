using UnityEngine;
using UnityEngine.UI;

public class MoveCounterUI : MonoBehaviour
{
    public Text moveCountText;
    
    void Start()
    {
        // Crear el UI si no existe
        if (moveCountText == null)
        {
            CreateUI();
        }
        
        // Mostrar el contador inicial
        UpdateMoveCount(MoveCounter.Instance.GetTotalMoves());
    }
    
    void CreateUI()
    {
        // Buscar o crear Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MoveCounterCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Crear panel contenedor
        GameObject panel = new GameObject("MoveCounterPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);  // Esquina superior derecha
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-20, -20);  // 20 píxeles de margen
        panelRect.sizeDelta = new Vector2(200, 50);
        
        // Crear texto "Moves:"
        GameObject labelObj = new GameObject("MovesLabel");
        labelObj.transform.SetParent(panel.transform, false);
        
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Moves:";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 24;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleRight;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        // Añadir sombra al texto
        Shadow labelShadow = labelObj.AddComponent<Shadow>();
        labelShadow.effectColor = Color.black;
        labelShadow.effectDistance = new Vector2(2, -2);
        
        // Crear contador numérico
        GameObject counterObj = new GameObject("MovesCounter");
        counterObj.transform.SetParent(panel.transform, false);
        
        moveCountText = counterObj.AddComponent<Text>();
        moveCountText.text = "0";
        moveCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        moveCountText.fontSize = 28;
        moveCountText.fontStyle = FontStyle.Bold;
        moveCountText.color = Color.yellow;
        moveCountText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform counterRect = counterObj.GetComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(0.5f, 0);
        counterRect.anchorMax = new Vector2(1, 1);
        counterRect.offsetMin = new Vector2(10, 0);  // Pequeño margen a la izquierda
        counterRect.offsetMax = Vector2.zero;
        
        // Añadir sombra al contador
        Shadow counterShadow = counterObj.AddComponent<Shadow>();
        counterShadow.effectColor = Color.black;
        counterShadow.effectDistance = new Vector2(2, -2);
        
        Debug.Log("UI de contador de movimientos creada");
    }
    
    public void UpdateMoveCount(int moves)
    {
        if (moveCountText != null)
        {
            moveCountText.text = moves.ToString();
        }
    }
}