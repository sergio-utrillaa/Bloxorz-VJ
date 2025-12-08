using System;
using UnityEngine;

// Movement script for small 1x1x1 cubes (always vertical)
public class MoveCubeSmall : MonoBehaviour
{
    bool bMoving = false;
    bool bFalling = false;
    
    public float rotSpeed = 240f;
    public float fallSpeed = 10f;             // Maximum fall speed
    public float fallAcceleration = 9.8f;     // Acceleration of falling (like gravity)
    public float fallRotSpeed = 360f;         // Rotation speed while falling
    
    private float currentFallSpeed;           // Current fall speed (starts at 0, accelerates)
    private bool hasFallen = false;           // Flag to prevent multiple fall notifications
    
    Vector3 rotPoint, rotAxis;
    float rotRemainder;
    float rotDir;
    LayerMask layerMask;
    
    // Variables for fall rotation
    Vector3 fallRotAxis;
    float fallRotDir;
    Vector3 fallDirection;  // Direction of movement that caused the fall
    
    public AudioClip[] sounds;
    public AudioClip fallSound;
    
    // Method to check if cube is currently moving or falling
    public bool IsMoving()
    {
        return bMoving || bFalling;
    }
    
    bool isGrounded()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f, layerMask);
    }
    
    void Start()
    {
        layerMask = LayerMask.GetMask("Ground");
    }
    
    void Update()
    {
        if (bFalling)
        {
            // Acelerar la caída hasta alcanzar la velocidad máxima
            currentFallSpeed += fallAcceleration * Time.deltaTime;
            /* if (currentFallSpeed > fallSpeed)
                currentFallSpeed = fallSpeed; */
            
            Vector3 direction = Vector3.down; // Por defecto, cae vertical
            if (fallRotAxis != Vector3.zero)
            {
                // Si rota en Z (HorizontalX cayendo), se mueve en X
                if (Mathf.Abs(fallRotAxis.z) > 0.5f)
                {
                    // fallRotDir negativo = cae hacia la derecha (X positivo)
                    // fallRotDir positivo = cae hacia la izquierda (X negativo)
                    float horizontalComponent = -fallRotDir * 0.4f;
                    direction = new Vector3(horizontalComponent, -1f, 0f).normalized;
                }
                // Si rota en X (HorizontalZ cayendo), se mueve en Z
                else if (Mathf.Abs(fallRotAxis.x) > 0.5f)
                {
                    // fallRotDir positivo = cae hacia atrás (Z positivo)
                    // fallRotDir negativo = cae hacia adelante (Z negativo)
                    float horizontalComponent = fallRotDir * 0.4f;
                    direction = new Vector3(0f, -1f, horizontalComponent).normalized;
                }
            }

            // Caer con velocidad acelerada
            transform.Translate(direction * currentFallSpeed * Time.deltaTime, Space.World);

            // Rotar sobre sí mismo mientras cae
            if (fallRotAxis != Vector3.zero)
            {
                transform.Rotate(fallRotAxis, fallRotSpeed * fallRotDir * Time.deltaTime, Space.World);
            }
            
            // Verificar si el cubo ha caído demasiado bajo
            if (transform.position.y < -10.0f && !hasFallen)
            {
                hasFallen = true;
                // Notificar al MapCreation que el cubo se ha caído
                MapCreation mapCreation = FindObjectOfType<MapCreation>();
                if (mapCreation != null)
                {
                    mapCreation.OnCubeFell();
                }
            }
        }
        else if (bMoving)
        {
            float amount = rotSpeed * Time.deltaTime;
            if (amount > rotRemainder)
            {
                transform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                bMoving = false;
            }
            else
            {
                transform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
        }
        else
        {
            if (!isGrounded())
            {
                bFalling = true;
                currentFallSpeed = fallSpeed;  // Iniciar velocidad de caída en 0
                
                // Configurar rotación de caída según la última dirección de movimiento
                SetupFallRotation();
                
                if (fallSound != null)
                {
                    AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
                }
            }
            
            Vector2 dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            dir.Normalize();
            
            if (Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
                bMoving = true;
                
                // Guardar dirección de movimiento para la caída
                fallDirection = new Vector3(dir.x, 0, dir.y);
                
                if (sounds != null && sounds.Length > 0)
                {
                    int iSound = UnityEngine.Random.Range(0, sounds.Length);
                    AudioSource.PlayClipAtPoint(sounds[iSound], transform.position, 1.0f);
                }
                
                // Los cubos pequeños siempre rotan 90 grados (son 1x1x1)
                if (dir.x > 0.99)
                {
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    rotPoint = transform.position + new Vector3(-0.5f, -0.5f, 0.0f);
                }
                else if (dir.x < -0.99)
                {
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    rotPoint = transform.position + new Vector3(0.5f, -0.5f, 0.0f);
                }
                else if (dir.y > 0.99)
                {
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    rotPoint = transform.position + new Vector3(0.0f, -0.5f, -0.5f);
                }
                else if (dir.y < -0.99)
                {
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    rotPoint = transform.position + new Vector3(0.0f, -0.5f, 0.5f);
                }
            }
        }
    }
    
    // Configure rotation for falling based on last movement direction
    void SetupFallRotation()
    {
        // Si no hay dirección guardada, no rotar al caer
        if (fallDirection == Vector3.zero)
        {
            fallRotAxis = Vector3.zero;
            return;
        }
        
        // Rotar en el eje perpendicular al movimiento
        // Si se movió en X (izquierda/derecha), rotar alrededor del eje Z
        if (Mathf.Abs(fallDirection.x) > 0.5f)
        {
            fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
            fallRotDir = fallDirection.x > 0 ? 1.0f : -1.0f;
        }
        // Si se movió en Z (adelante/atrás), rotar alrededor del eje X
        else if (Mathf.Abs(fallDirection.z) > 0.5f)
        {
            fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
            fallRotDir = fallDirection.z > 0 ? -1.0f : 1.0f;
        }
        else
        {
            fallRotAxis = Vector3.zero;
        }
    }
}
