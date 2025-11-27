using System;
using UnityEngine;


// MoveCube manages cube movement. WASD + Cursor keys rotate the cube in the
// selected direction. If the cube is not grounded (has a tile under it), it falls.
// Some events trigger corresponding sounds.


public class MoveCube : MonoBehaviour
{
    bool bMoving = false; 			// Is the object in the middle of moving?
	bool bFalling = false; 			// Is the object falling?
    bool bEdgeRotation = false;     // Is the cube doing initial edge rotation before free fall?
    
	public float rotSpeed; 			// Rotation speed in degrees per second
    public float fallSpeed; 		// Maximum fall speed in the Y direction
    public float fallAcceleration = 9.8f; // Acceleration of falling (like gravity)
    public float fallRotSpeed = 360.0f; // Rotation speed while falling
    public float edgeRotationAngle = 90.0f; // Degrees to rotate on edge before free fall
    
    private float currentFallSpeed; // Current fall speed (starts at 0, accelerates to fallSpeed)
    private bool hasFallen = false;

    private bool isOnGoalTile = false;
    private bool isGoalAnimationActive = false;

    Vector3 rotPoint, rotAxis; 		// Rotation movement is performed around the line formed by rotPoint and rotAxis
	float rotRemainder; 			// The angle that the cube still has to rotate before the current movement is completed
    float rotDir; 					// Has rotRemainder to be applied in the positive or negative direction?
    LayerMask layerMask; 			// LayerMask to detect raycast hits with ground tiles only
    
    Vector2 lastMoveDirection; 		// Last movement direction (to determine fall rotation)
    
    // Variables para la rotación durante la caída
    Vector3 fallPivotOffset;        // Offset relativo desde el centro del cubo al punto de pivote
    Vector3 fallRotAxis;            // Eje de rotación durante la caída
    float fallRotDir;               // Dirección de rotación durante la caída
    float edgeRotationRemaining;    // Ángulo restante de rotación en el borde

    Vector3 pivotPoint;

    public AudioClip[] sounds; 		// Sounds to play when the cube rotates
    public AudioClip fallSound; 	// Sound to play when the cube starts falling

    enum CubeState { Vertical, HorizontalX, HorizontalZ }
    CubeState state = CubeState.Vertical;
    CubeState stateBeforeMove = CubeState.Vertical; // Estado antes del último movimiento

    // Public method to reset all cube parameters
    public void ResetCube()
    {
        bMoving = false;
        bFalling = false;
        bEdgeRotation = false;
        state = CubeState.Vertical;
        stateBeforeMove = CubeState.Vertical;
        rotRemainder = 0f;
        edgeRotationRemaining = 0f;
        currentFallSpeed = fallSpeed;
        lastMoveDirection = Vector2.zero;
    }

    // Method to check if cube is in vertical position
    public bool IsVertical()
    {
        return state == CubeState.Vertical;
    }
    
    // Method to check if cube is currently moving or falling
    public bool IsMoving()
    {
        return bMoving || bFalling || bEdgeRotation;
    }
    
    // Method to set the cube state after merging (HorizontalX or HorizontalZ)
    public void SetHorizontalState(bool isXAxis)
    {
        bMoving = false;
        bFalling = false;
        bEdgeRotation = false;
        
        if (isXAxis)
        {
            state = CubeState.HorizontalX;
            stateBeforeMove = CubeState.HorizontalX;
        }
        else
        {
            state = CubeState.HorizontalZ;
            stateBeforeMove = CubeState.HorizontalZ;
        }
        
        rotRemainder = 0f;
        edgeRotationRemaining = 0f;
        currentFallSpeed = fallSpeed;
        lastMoveDirection = Vector2.zero;
    }


    // Determine if the cube is grounded by shooting a ray down from the cube location and 
    // looking for hits with ground tiles

    bool isGrounded()
    {
        RaycastHit hit;
        
        // Si el cubo está vertical, solo verificamos el centro
        if (state == CubeState.Vertical)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f, layerMask))
                return true;
        }
        // Si el cubo está horizontal en X, verificamos ambas mitades (izquierda y derecha)
        else if (state == CubeState.HorizontalX)
        {
            Vector3 leftHalf = transform.position + new Vector3(-0.5f, 0.0f, 0.0f);
            Vector3 rightHalf = transform.position + new Vector3(0.5f, 0.0f, 0.0f);
            
            bool leftGrounded = Physics.Raycast(leftHalf, Vector3.down, out hit, 2.0f, layerMask);
            bool rightGrounded = Physics.Raycast(rightHalf, Vector3.down, out hit, 2.0f, layerMask);
            
            // Solo está soportado si AMBAS mitades tienen suelo
            if (leftGrounded && rightGrounded)
                return true;
        }
        // Si el cubo está horizontal en Z, verificamos ambas mitades (adelante y atrás)
        else if (state == CubeState.HorizontalZ)
        {
            Vector3 frontHalf = transform.position + new Vector3(0.0f, 0.0f, -0.5f);
            Vector3 backHalf = transform.position + new Vector3(0.0f, 0.0f, 0.5f);
            
            bool frontGrounded = Physics.Raycast(frontHalf, Vector3.down, out hit, 2.0f, layerMask);
            bool backGrounded = Physics.Raycast(backHalf, Vector3.down, out hit, 2.0f, layerMask);
            
            // Solo está soportado si AMBAS mitades tienen suelo
            if (frontGrounded && backGrounded)
                return true;
        }

        return false;
    }
    
    // Configura la rotación para cuando el cubo cae
    void SetupFallRotation()
    {
        RaycastHit hit;
        
        Debug.Log("before: " + stateBeforeMove);
        Debug.Log("after: " + state);

        if (state == CubeState.HorizontalX)
        {
            Vector3 leftHalf = transform.position + new Vector3(-0.5f, 0.0f, 0.0f);
            Vector3 rightHalf = transform.position + new Vector3(0.5f, 0.0f, 0.0f);
            
            bool leftGrounded = Physics.Raycast(leftHalf, Vector3.down, out hit, 2.0f, layerMask);
            bool rightGrounded = Physics.Raycast(rightHalf, Vector3.down, out hit, 2.0f, layerMask);
            
            // Si el lado izquierdo tiene soporte pero el derecho no, rota hacia la derecha
            if (leftGrounded && !rightGrounded)
            {
                fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                fallPivotOffset = new Vector3(0.0f, -0.5f, 0.0f); // El pivote está en el borde izquierdo inferior
                fallRotDir = -1.0f;
                bEdgeRotation = true;
                edgeRotationRemaining = edgeRotationAngle;

                pivotPoint = transform.position + fallPivotOffset;
            }
            // Si el lado derecho tiene soporte pero el izquierdo no, rota hacia la izquierda
            else if (rightGrounded && !leftGrounded)
            {
                fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                fallPivotOffset = new Vector3(0.0f, -0.5f, 0.0f); // El pivote está en el borde derecho inferior
                fallRotDir = 1.0f;
                bEdgeRotation = true;
                edgeRotationRemaining = edgeRotationAngle;
                pivotPoint = transform.position + fallPivotOffset;
            }
            // Si ninguno tiene soporte, usar última dirección de movimiento
            else if (lastMoveDirection != Vector2.zero)
            {
                if (stateBeforeMove == CubeState.HorizontalX)
                {
                    fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    fallRotDir = -Mathf.Sign(lastMoveDirection.y);
                }
                else if (stateBeforeMove == CubeState.Vertical)
                {
                    fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    fallRotDir = Mathf.Sign(lastMoveDirection.x);
                }
            }
            else
            {
                fallRotAxis = Vector3.zero;
                bEdgeRotation = false;
            }
        }
        else if (state == CubeState.HorizontalZ)
        {
            Vector3 frontHalf = transform.position + new Vector3(0.0f, 0.0f, -0.5f);
            Vector3 backHalf = transform.position + new Vector3(0.0f, 0.0f, 0.5f);
            
            bool frontGrounded = Physics.Raycast(frontHalf, Vector3.down, out hit, 2.0f, layerMask);
            bool backGrounded = Physics.Raycast(backHalf, Vector3.down, out hit, 2.0f, layerMask);
            
            // Si el lado frontal tiene soporte pero el trasero no, rota hacia atrás
            if (frontGrounded && !backGrounded)
            {
                fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                fallPivotOffset = new Vector3(0.0f, -0.5f, -0.0f); // El pivote está en el borde frontal inferior
                fallRotDir = 1.0f;
                bEdgeRotation = true;
                edgeRotationRemaining = edgeRotationAngle;

                pivotPoint = transform.position + fallPivotOffset;
            }
            // Si el lado trasero tiene soporte pero el frontal no, rota hacia adelante
            else if (backGrounded && !frontGrounded)
            {
                fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                fallPivotOffset = new Vector3(0.0f, -0.5f, 0.0f); // El pivote está en el borde trasero inferior
                fallRotDir = -1.0f;
                bEdgeRotation = true;
                edgeRotationRemaining = edgeRotationAngle;
                pivotPoint = transform.position + fallPivotOffset;
            }
            // Si ninguno tiene soporte, usar última dirección de movimiento
            else if (lastMoveDirection != Vector2.zero)
            {
                if (stateBeforeMove == CubeState.HorizontalZ)
                {
                    fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    fallRotDir = Mathf.Sign(lastMoveDirection.x);
                }
                else if (stateBeforeMove == CubeState.Vertical)
                {
                    fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    fallRotDir = -Mathf.Sign(lastMoveDirection.y);
                }
            }
            else
            {
                fallRotAxis = Vector3.zero;
                bEdgeRotation = false;
            }
        }
        else // Vertical
        {
            // Cuando está vertical y cae, usar última dirección de movimiento
            if (lastMoveDirection != Vector2.zero)
            {
                // Si el último movimiento fue en X
                if (Mathf.Abs(lastMoveDirection.x) > 0.5f)
                {
                    Debug.Log("kebabish");
                    fallRotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    fallRotDir = Mathf.Sign(lastMoveDirection.x);
                }
                // Si el último movimiento fue en Y (Z en mundo)
                else
                {
                    Debug.Log("bishmilla");
                    fallRotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    fallRotDir = -Mathf.Sign(lastMoveDirection.y);
                }
            }
            else
            {
                fallRotAxis = Vector3.zero;
                bEdgeRotation = false;
            }
        }
        Debug.Log(lastMoveDirection);
    }

    bool IsOnGoalTile()
    {
        // Solo verificar si está en posición vertical
        if (state != CubeState.Vertical)
            return false;
        
        RaycastHit hit;
        // Raycast desde el centro del cubo hacia abajo
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f, layerMask))
        {
            return hit.collider.CompareTag("Goal");
        }
        
        return false;
    }
    
    // Método para iniciar la animación de meta
    public void StartGoalAnimation()
    {
        if (!isGoalAnimationActive)
        {
            isGoalAnimationActive = true;
            bMoving = false; // Bloquear movimiento
            
            // Añadir componente de animación de meta
            GoalAnimation goalAnim = gameObject.AddComponent<GoalAnimation>();
            goalAnim.StartGoalAnimation();
            
            Debug.Log("¡Cubo ha llegado a la meta! Iniciando animación...");
        }
    }

    // Start is called once after the MonoBehaviour is created
    void Start()
    {
		// Create the layer mask for ground tiles. Done once in the Start method to avoid doing it every Update call.
        layerMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGoalAnimationActive && !bMoving && !bFalling && IsOnGoalTile())
        {
            StartGoalAnimation();
            return; // Salir del Update inmediatamente
        }

        // Si ya está en animación de meta, no procesar nada más
        if (isGoalAnimationActive)
        {
            return;
        }

        if(bFalling)
        {
            /*if (false){
                float rotAmount = fallRotSpeed * Time.deltaTime;
                transform.RotateAround(pivotPoint, fallRotAxis, rotAmount * fallRotDir);

                transform.Translate(Vector3.left * 1.0f * Time.deltaTime, Space.World);
            }
            // Fase 1: Rotación inicial sobre el borde del tile
            else*/ if (bEdgeRotation)
            {
                float rotAmount = fallRotSpeed * Time.deltaTime;
                
                if (rotAmount >= edgeRotationRemaining)
                {
                    // Completar la rotación del borde y pasar a caída libre
                    //Vector3 pivotPoint = transform.position + fallPivotOffset;
                    transform.RotateAround(pivotPoint, fallRotAxis, edgeRotationRemaining * fallRotDir);
                    bEdgeRotation = false;

                    /* currentFallSpeed += fallAcceleration * Time.deltaTime;
                    if (currentFallSpeed > fallSpeed) currentFallSpeed = fallSpeed;
                    
                    // Caer verticalmente con la velocidad actual
                    transform.Translate(Vector3.down * currentFallSpeed * Time.deltaTime, Space.World); */
                }
                else
                {
                    // Continuar rotando sobre el borde
                    //Vector3 pivotPoint = transform.position + fallPivotOffset;
                    transform.RotateAround(pivotPoint, fallRotAxis, rotAmount * fallRotDir);
                    edgeRotationRemaining -= rotAmount;

                    float pivotFallStep = fallSpeed * Time.deltaTime * 0.5f; // tweak factor for smoothness
                    pivotPoint += Vector3.down * pivotFallStep;

                    /* Debug.Log(pivotPoint);
                    Debug.Log("tr");
                    Debug.Log(transform.position); */

                    /* currentFallSpeed += fallAcceleration * Time.deltaTime;
                    if (currentFallSpeed > fallSpeed) currentFallSpeed = fallSpeed;
                    
                    // Caer verticalmente con la velocidad actual
                    transform.Translate(Vector3.down * currentFallSpeed * Time.deltaTime, Space.World); */
                }
            }
            // Fase 2: Caída libre con rotación sobre sí mismo
            else
            {
                // Si hay una rotación configurada, rotar sobre sí mismo mientras cae
                if (fallRotAxis != Vector3.zero)
                {
                    transform.Rotate(fallRotAxis, fallRotSpeed * fallRotDir * Time.deltaTime, Space.World);
                }
                
                // Acelerar la caída hasta alcanzar fallSpeed
                currentFallSpeed += fallAcceleration * Time.deltaTime;
                /* if (currentFallSpeed > fallSpeed)
                    currentFallSpeed = fallSpeed; */
                
                // Calcular dirección de caída según el eje y dirección de rotación
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
                
                // Caer en la dirección calculada con la velocidad actual
                transform.Translate(direction * currentFallSpeed * Time.deltaTime, Space.World);
            }

            // Verificar si el cubo ha caído demasiado bajo (al final de toda la lógica de caída)
            if (transform.position.y < -5.0f && !hasFallen)
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
			// If we are moving, we rotate around the line formed by rotPoint and rotAxis an angle depending on deltaTime
			// If this angle is larger than the remainder, we stop the movement
            float amount = rotSpeed * Time.deltaTime;
            if(amount > rotRemainder)
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
			// If we are not falling, nor moving, we check first if we should fall, then if we have to move
            if (!isGrounded())
            {
                bFalling = true;
                currentFallSpeed = fallSpeed; // Start falling from zero speed
                
                // Configurar la rotación de caída según el lado sin soporte
                SetupFallRotation();
				
				// Play sound associated to falling
                AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            }

            // Read the move action for input
            Vector2 dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            dir.Normalize();
            if(Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
				// If the absolute value of one of the axis is larger than 0.99, the player wants to move in a non diagonal direction
                bMoving = true;
                stateBeforeMove = state; // Guardar el estado antes del movimiento
                lastMoveDirection = dir; // Guardar la dirección del movimiento
				
				// We play a random movemnt sound
                int iSound = UnityEngine.Random.Range(0, sounds.Length);
                AudioSource.PlayClipAtPoint(sounds[iSound], transform.position, 1.0f);
				
				// Set rotDir, rotRemainder, rotPoint, and rotAxis according to the movement the player wants to make
                if (dir.x > 0.99)
                {
                    
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);

                    if (state == CubeState.Vertical)
                        rotPoint = transform.position + new Vector3(-0.5f, -1.0f, 0.0f);
                    else if (state == CubeState.HorizontalX)
                        rotPoint = transform.position + new Vector3(-1.0f, -0.5f, 0.0f);
                    else // HorizontalZ
                        rotPoint = transform.position + new Vector3(-0.5f, -0.5f, 0.0f);

                    // ACTUALIZAR ESTADO
                    if (state == CubeState.Vertical) state = CubeState.HorizontalX;
                    else if (state == CubeState.HorizontalX) state = CubeState.Vertical;
                    // HorizontalZ se mantiene HorizontalZ

                }
                else if (dir.x < -0.99)
                {
                    
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);

                    if (state == CubeState.Vertical)
                        rotPoint = transform.position + new Vector3(0.5f, -1.0f, 0.0f);
                    else if (state == CubeState.HorizontalX)
                        rotPoint = transform.position + new Vector3(1.0f, -0.5f, 0.0f);
                    else // HorizontalZ
                        rotPoint = transform.position + new Vector3(0.5f, -0.5f, 0.0f);

                    // Cambiar estado
                    if (state == CubeState.Vertical)
                        state = CubeState.HorizontalX;
                    else if (state == CubeState.HorizontalX)
                        state = CubeState.Vertical;
                }
                else if (dir.y > 0.99)
                {
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    //rotPoint = transform.position + new Vector3(0.0f, -0.5f, -0.5f);
                    if (state == CubeState.Vertical)
                        rotPoint = transform.position + new Vector3(0.0f, -1.0f, -0.5f);
                    else if (state == CubeState.HorizontalZ)
                        rotPoint = transform.position + new Vector3(0.0f, -0.5f, -1.0f);
                    else // HorizontalX
                        rotPoint = transform.position + new Vector3(0.0f, -0.5f, -0.5f);

                    // Cambiar estado
                    if (state == CubeState.Vertical)
                        state = CubeState.HorizontalZ;
                    else if (state == CubeState.HorizontalZ)
                        state = CubeState.Vertical;
                }
                else if (dir.y < -0.99)
                {
                    
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    //rotPoint = transform.position + new Vector3(0.0f, -0.5f, 0.5f);
                    if (state == CubeState.Vertical)
                        rotPoint = transform.position + new Vector3(0.0f, -1.0f, 0.5f);
                    else if (state == CubeState.HorizontalZ)
                        rotPoint = transform.position + new Vector3(0.0f, -0.5f, 1.0f);
                    else // HorizontalX
                        rotPoint = transform.position + new Vector3(0.0f, -0.5f, 0.5f);

                    // Cambiar estado
                    if (state == CubeState.Vertical)
                        state = CubeState.HorizontalZ;
                    else if (state == CubeState.HorizontalZ)
                        state = CubeState.Vertical;
                }
            }
        }
    }

}
