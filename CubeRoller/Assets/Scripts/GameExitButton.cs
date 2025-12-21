using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameExitButton : MonoBehaviour
{
    // Método para llamar desde un botón de UI
    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        
        #if UNITY_EDITOR
            // En el editor, detener el modo de juego
            EditorApplication.isPlaying = false;
        #else
            // En el build, cerrar la aplicación
            Application.Quit();
        #endif
    }
}
