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

    // Start is called once after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;   // 20% de velocidad → cámara lenta
        CreateMap();
    }
    
    // Update is called once per frame
    void Update()
    {
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
		
		// Process the map. For each tileId == 2 create a copy of the tile prefab
        for(int z=0; z<sizeZ; z++)
            for(int x=0; x<sizeX; x++)
            {
                if (nums[z * sizeX + x + 2] == 2)
                {
                    int xFlipped = (sizeX - 1) - x;   // <--- volteo horizontal

                    // Instantiate the copy at its corresponding location
                    //Instantiate(tile, new Vector3(x, 0.0f, z), transform.rotation);
                    GameObject obj = Instantiate(tile, new Vector3(xFlipped, -0.05f, z), transform.rotation);

                    // Set the new object parent to be the game object containing this script
                    obj.transform.parent = transform;

                }
            }
      
      // Move cube to initial position and reset its parameters
      if (cube != null)
      {
          cube.transform.position = cubePosition;
          cube.transform.rotation = Quaternion.identity;
          
          // Reset MoveCube component parameters
          MoveCube moveCube = cube.GetComponent<MoveCube>();
          if (moveCube != null)
          {
              moveCube.ResetCube();
          }
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
