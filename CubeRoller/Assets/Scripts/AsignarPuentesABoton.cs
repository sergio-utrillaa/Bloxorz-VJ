using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsignarPuenteABoton : MonoBehaviour
{
    public GameObject[] puentesManuales; // Arrastra aquí el Puente1 desde el Inspector
    
    void Start()
    {
        // Esperar a que se cree el botón
        Invoke("AsignarPuentes", 0.5f);
    }
    
    void AsignarPuentes()
    {
        // Buscar el botón creado dinámicamente
        BotonRedondo boton = FindObjectOfType<BotonRedondo>();
        if (boton != null && puentesManuales.Length > 0)
        {
            boton.puentesControlados = puentesManuales;
            Debug.Log($"Puentes asignados manualmente al botón: {puentesManuales.Length}");
        }
        else
        {
            Debug.LogWarning("No se pudo asignar los puentes. Botón o puentes no encontrados.");
        }
    }
}
