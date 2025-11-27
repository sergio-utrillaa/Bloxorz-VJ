using System;
using UnityEngine;

// Movement script for small 1x1x1 cubes (always vertical)
public class MoveCubeSmall : MonoBehaviour
{
    bool bMoving = false;
    bool bFalling = false;
    
    public float rotSpeed = 180f;
    public float fallSpeed = 5f;
    
    Vector3 rotPoint, rotAxis;
    float rotRemainder;
    float rotDir;
    LayerMask layerMask;
    
    public AudioClip[] sounds;
    public AudioClip fallSound;
    
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
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
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
}
