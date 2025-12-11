using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsignarPuenteABoton : MonoBehaviour
{
    [System.Serializable]
    public class BotonPuenteMapping
    {
        public Vector3 posicionBoton; // Posición del botón (x, 0, z)
        public GameObject[] puentes; // Puentes que controla ese botón
        public bool esBotonCruz = false; // Si es botón cruz en lugar de redondo
    }
    
    public BotonPuenteMapping[] configuracionBotones;
    public float toleranciaPosicion = 0.5f; // Tolerancia para comparar posiciones
    
    void Start()
    {
        // Esperar a que se creen los botones
        Invoke("AsignarPuentes", 0.5f);
    }
    
    void AsignarPuentes()
    {
        if (configuracionBotones == null || configuracionBotones.Length == 0)
        {
            Debug.LogWarning("No hay configuración de botones definida.");
            return;
        }
        
        // Buscar todos los botones en la escena
        BotonRedondo[] botonesRedondos = FindObjectsOfType<BotonRedondo>();
        BotonCruz[] botonesCruz = FindObjectsOfType<BotonCruz>();
        
        int asignacionesExitosas = 0;
        
        // Asignar puentes según la configuración
        foreach (var config in configuracionBotones)
        {
            if (config.puentes == null || config.puentes.Length == 0)
            {
                Debug.LogWarning($"Configuración sin puentes en posición {config.posicionBoton}");
                continue;
            }
            
            if (config.esBotonCruz)
            {
                // Buscar botón cruz en esa posición
                BotonCruz botonCruz = EncontrarBotonCruzEnPosicion(botonesCruz, config.posicionBoton);
                if (botonCruz != null)
                {
                    botonCruz.puentesControlados = config.puentes;
                    asignacionesExitosas++;
                    Debug.Log($"Puentes asignados a botón cruz en {config.posicionBoton}: {config.puentes.Length} puentes");
                }
                else
                {
                    Debug.LogWarning($"No se encontró botón cruz en posición {config.posicionBoton}");
                }
            }
            else
            {
                // Buscar botón redondo en esa posición
                BotonRedondo botonRedondo = EncontrarBotonRedondoEnPosicion(botonesRedondos, config.posicionBoton);
                if (botonRedondo != null)
                {
                    botonRedondo.puentesControlados = config.puentes;
                    asignacionesExitosas++;
                    Debug.Log($"Puentes asignados a botón redondo en {config.posicionBoton}: {config.puentes.Length} puentes");
                }
                else
                {
                    Debug.LogWarning($"No se encontró botón redondo en posición {config.posicionBoton}");
                }
            }
        }
        
        Debug.Log($"Asignaciones exitosas: {asignacionesExitosas}/{configuracionBotones.Length}");
    }
    
    BotonRedondo EncontrarBotonRedondoEnPosicion(BotonRedondo[] botones, Vector3 posicionBuscada)
    {
        foreach (var boton in botones)
        {
            // Comparar solo X y Z, ignorar Y
            Vector3 posBoton = new Vector3(boton.transform.position.x, 0, boton.transform.position.z);
            Vector3 posBuscada = new Vector3(posicionBuscada.x, 0, posicionBuscada.z);
            
            if (Vector3.Distance(posBoton, posBuscada) < toleranciaPosicion)
            {
                return boton;
            }
        }
        return null;
    }
    
    BotonCruz EncontrarBotonCruzEnPosicion(BotonCruz[] botones, Vector3 posicionBuscada)
    {
        foreach (var boton in botones)
        {
            // Comparar solo X y Z, ignorar Y
            Vector3 posBoton = new Vector3(boton.transform.position.x, 0, boton.transform.position.z);
            Vector3 posBuscada = new Vector3(posicionBuscada.x, 0, posicionBuscada.z);
            
            if (Vector3.Distance(posBoton, posBuscada) < toleranciaPosicion)
            {
                return boton;
            }
        }
        return null;
    }
}