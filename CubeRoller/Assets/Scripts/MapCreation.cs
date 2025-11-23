using System;
using UnityEngine;


// MapCreation instances multiple copies of a tile prefab to build a level
// following the contents of a map file


public class MapCreation : MonoBehaviour
{
    public TextAsset map; 		// Text file containing the map
    public GameObject tile; 	// Tile prefab used to instance and build the level
    
    public GameObject cube; // Reference to the existing cube in the scene
    public Vector3 cubePosition;
    public GameObject botonRedondoPrefab;
    
    // Animation parameters
    public float tileAnimationDuration = 0.5f;  // Duration of tile rise animation
    public float tileStartHeight = -10.0f;        // Height where tiles start (below ground)
    public float delayBetweenTiles = 0.05f;      // Delay between each tile animation
    public float cubeSpawnHeight = 10.0f;        // Height from where cube falls
    public float cubeFallDuration = 0.8f;        // Duration of cube fall animation
    
    private float totalAnimationTime = 0f;      // Total time until all tiles finish
    private bool cubeSpawned = false;            // Flag to check if cube has been spawned

    // Start is called once after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;   // 20% de velocidad → cámara lenta
        CreateMap();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Check if tiles animation has finished and spawn cube
        if (!cubeSpawned && totalAnimationTime > 0f)
        {
            totalAnimationTime -= Time.deltaTime;
            if (totalAnimationTime <= 0f)
            {
                SpawnCube();
            }
        }
        
        // Check if X key is pressed to restart map creation
        if (Input.GetKeyDown(KeyCode.X))
        {
            RestartMapCreation();
        }
    }

    // Method to create the map from the text file
    void CreateMap()
    {
        char[] seps = {' ', '\n', '\r'}; 	// Characters that act as separators between numbers
        string [] snums; 					// Substrings read from the map file
        int [] nums; 						// Numbers converted from strings in snums

		// Split the string of the whole map file into substrings separated by spaces
        snums = map.text.Split(seps, StringSplitOptions.RemoveEmptyEntries);
		
		// Convert the substrings in snums to integers
        nums = new int[snums.Length];
        for (int i = 0; i < snums.Length; i++)
        {
            nums[i] = int.Parse(snums[i]);
        }
		
		// Create the level. First get the size in tiles of the map from nums
        int sizeX = nums[0], sizeZ = nums[1];
		
		// Hide cube initially
		if (cube != null)
		{
		    cube.SetActive(false);
		}
		
		cubeSpawned = false;
		float maxDelay = 0f;
		
		// Process the map. For each tileId == 2 create a copy of the tile prefab
        for(int z=0; z<sizeZ; z++)
            for(int x=0; x<sizeX; x++)
            {
                int tileValue = nums[z * sizeX + x + 2];
                
                if (tileValue == 2 || tileValue == 4)
                {
                    int xFlipped = (sizeX - 1) - x;   // <--- volteo horizontal

                    // Instantiate the copy at its corresponding location
                    GameObject obj = Instantiate(tile, new Vector3(xFlipped, -0.05f, z), transform.rotation);

                    // Set the new object parent to be the game object containing this script
                    obj.transform.parent = transform;
                    
                    // Add animation component
                    TileAnimation tileAnim = obj.AddComponent<TileAnimation>();
                    
                    // Wave from bottom-left to top-right
                    float delay = (x + z) * delayBetweenTiles;
                    
                    // Track maximum delay to know when last tile finishes
                    if (delay > maxDelay)
                        maxDelay = delay;
                    
                    tileAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight);

                    // Si es un 4, crear el botón encima del tile
                     if (tileValue == 4 && botonRedondoPrefab != null)
                    {
                        // Crear el botón a la altura final del tile (0.0f)
                        GameObject boton = Instantiate(botonRedondoPrefab, 
                            new Vector3(xFlipped, 0.0f, z),  // Altura final sobre el tile
                            Quaternion.identity);
                        boton.transform.parent = transform;
                        
                        // Animar el botón desde la misma altura inicial que el tile
                        TileAnimation botonAnim = boton.AddComponent<TileAnimation>();
                        botonAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight + 0.05f);  // Mismo delay, empieza 0.2 más arriba que el tile
                    }
                }
            }
      
      // Calculate total animation time (last tile's delay + animation duration)
      totalAnimationTime = maxDelay + tileAnimationDuration;
    }
    
    // Method to spawn and initialize the cube
    void SpawnCube()
    {
        if (cube != null && !cubeSpawned)
        {
            cube.SetActive(true);
            cube.transform.position = cubePosition;
            cube.transform.rotation = Quaternion.identity;
            
            // Reset MoveCube component parameters
            MoveCube moveCube = cube.GetComponent<MoveCube>();
            if (moveCube != null)
            {
                moveCube.ResetCube();
            }
            
            // Add fall animation component
            CubeSpawnAnimation cubeAnim = cube.AddComponent<CubeSpawnAnimation>();
            cubeAnim.StartFallAnimation(cubeFallDuration, cubeSpawnHeight);
            
            cubeSpawned = true;
        }
    }
    
    // Public method to restart the map creation (can be called from UI button)
    public void RestartMapCreation()
    {
        // Destroy all child tiles
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // Recreate the map (this will also reset the cube position)
        CreateMap();
    }

}
