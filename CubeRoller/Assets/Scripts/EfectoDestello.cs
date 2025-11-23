using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EfectoDestello : MonoBehaviour
{
    public float duracion = 1.5f;
    public AnimationCurve curvaIntensidad = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private Light luz;
    private ParticleSystem particulas;
    private float tiempoInicio;
    private float intensidadInicial;
    
    void Start()
    {
        tiempoInicio = Time.time;
        
        luz = GetComponent<Light>();
        if (luz != null)
        {
            intensidadInicial = luz.intensity;
        }
        
        particulas = GetComponent<ParticleSystem>();
        if (particulas != null)
        {
            particulas.Play();
        }
        
        // Autodestruirse después de la duración
        Destroy(gameObject, duracion);
    }
    
    void Update()
    {
        float tiempoTranscurrido = Time.time - tiempoInicio;
        float progreso = Mathf.Clamp01(tiempoTranscurrido / duracion);
        
        // Animar la intensidad de la luz
        if (luz != null)
        {
            luz.intensity = intensidadInicial * curvaIntensidad.Evaluate(progreso);
        }
    }
}
