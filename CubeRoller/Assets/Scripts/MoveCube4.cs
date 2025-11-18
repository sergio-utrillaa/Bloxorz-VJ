using System;
using UnityEngine;


// MoveCube manages cube movement. WASD + Cursor keys rotate the cube in the
// selected direction. If the cube is not grounded (has a tile under it), it falls.
// Some events trigger corresponding sounds.


public class MoveCube : MonoBehaviour
{
    bool bMoving = false; 			// Is the object in the middle of moving?
	bool bFalling = false; 			// Is the object falling?
    
	public float rotSpeed; 			// Rotation speed in degrees per second
    public float fallSpeed; 		// Fall speed in the Y direction

    Vector3 rotPoint, rotAxis; 		// Rotation movement is performed around the line formed by rotPoint and rotAxis
	float rotRemainder; 			// The angle that the cube still has to rotate before the current movement is completed
    float rotDir; 					// Has rotRemainder to be applied in the positive or negative direction?
    LayerMask layerMask; 			// LayerMask to detect raycast hits with ground tiles only

    public AudioClip[] sounds; 		// Sounds to play when the cube rotates
    public AudioClip fallSound; 	// Sound to play when the cube starts falling

    enum CubeState { Vertical, HorizontalX, HorizontalZ }
    CubeState state = CubeState.Vertical;


    // Determine if the cube is grounded by shooting a ray down from the cube location and 
    // looking for hits with ground tiles

    bool isGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f, layerMask))
            return true;

        return false;
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
        if(bFalling)
        {
			// If we have fallen, we just move down
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
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
