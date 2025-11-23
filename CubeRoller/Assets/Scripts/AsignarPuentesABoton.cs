using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsignarPuenteABoton : MonoBehaviour
{
    public GameObject[] puentesManuales; // Para BotonRedondo (Puente1)
    public GameObject[] puentesManualesCruz; // Para BotonCruz (Puente2)
    
    void Start()
    {
        // Esperar a que se creen los botones
        Invoke("AsignarPuentes", 0.5f);
    }
    
    void AsignarPuentes()
    {
        // Asignar puentes al botón redondo
        BotonRedondo botonRedondo = FindObjectOfType<BotonRedondo>();
        if (botonRedondo != null && puentesManuales.Length > 0)
        {
            botonRedondo.puentesControlados = puentesManuales;
            Debug.Log($"Puentes asignados manualmente al botón redondo: {puentesManuales.Length}");
        }
        
        // Asignar puentes al botón cruz
        BotonCruz botonCruz = FindObjectOfType<BotonCruz>();
        if (botonCruz != null && puentesManualesCruz.Length > 0)
        {
            botonCruz.puentesControlados = puentesManualesCruz;
            Debug.Log($"Puentes asignados manualmente al botón cruz: {puentesManualesCruz.Length}");
        }
        
        // Mensaje de advertencia si no se encontraron botones o puentes
        if (botonRedondo == null && botonCruz == null)
        {
            Debug.LogWarning("No se encontraron botones para asignar puentes.");
        }
    }
}
