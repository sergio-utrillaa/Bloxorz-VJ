using System;
using UnityEngine;
using UnityEngine.SceneManagement; // Para reiniciar la escena
using System.Collections;
using System.Collections.Generic;

// MapCreation instances multiple copies of a tile prefab to build a level
// following the contents of a map file


public class MapCreation : MonoBehaviour
{
    public TextAsset map; 		// Text file containing the map
    public GameObject tile; 	// Tile prefab used to instance and build the level
    
    public GameObject cube; // Reference to the existing cube in the scene
    public Vector3 cubePosition;
    public GameObject botonRedondoPrefab;
    public GameObject botonCruzPrefab;
    public GameObject botonDivisorPrefab;
    public GameObject tileNaranjaPrefab;
    
    // Animation parameters
    public float tileAnimationDuration = 0.5f;  // Duration of tile rise animation
    public float tileStartHeight = -10.0f;        // Height where tiles start (below ground)
    public float delayBetweenTiles = 0.05f;      // Delay between each tile animation
    public float cubeSpawnHeight = 10.0f;        // Height from where cube falls
    public float cubeFallDuration = 0.8f;        // Duration of cube fall animation
    
    private float totalAnimationTime = 0f;      // Total time until all tiles finish
    private bool cubeSpawned = false;            // Flag to check if cube has been spawned

    // Nuevas variables para la animación de caída
    public float tileFallAnimationDuration = 0.3f;  // Duración de la animación de caída
    public float tileFallStartHeight = -10.0f;      // Altura final donde terminan los tiles
    public float delayBetweenFallTiles = 0.03f;     // Delay entre cada tile que cae
    public float fadeOutDurationOnFall = 0.8f;      // Duración del fade cuando el cubo cae
    
    private bool isFalling = false;                 // Flag para evitar múltiples animaciones de caída

    // Nuevas variables para la animación de victoria
    public float tileRiseAnimationDuration = 1.0f;  // Duración de la animación de subida
    public float tileRiseHeight = 15.0f;            // Altura a la que suben los tiles
    public float delayBetweenRiseTiles = 0.02f;     // Delay entre cada tile que sube
    public float fadeOutDuration = 1.0f;            // Duración del fade a negro
    
    private bool isVictoryAnimation = false;        // Flag para evitar múltiples animaciones
    private bool hasStartedAnimation = false; // Flag para evitar múltiples inicios
    private bool hasCreatedUI = false; // Nuevo flag para el UI

    private List<GameObject> allTiles = new List<GameObject>(); // Lista de todos los tiles creados
    private List<GameObject> allBridges = new List<GameObject>();

    // Audio de spawn del cubo
    [Header("Audio de Spawn")]
    public AudioClip cubeSpawnSound;
    
    [Range(0f, 10.0f)]
    public float spawnSoundVolume = 10.0f;
    
    // Audio de nivel completado
    [Header("Audio de Victoria")]
    public AudioClip levelCompletedSound;
    
    [Range(0f, 1.0f)]
    [Tooltip("Volumen del sonido de victoria")]
    public float levelCompletedSoundVolume = 0.5f;
    
    // ✨ NUEVO: Audio de muerte para tiles naranjas
    [Header("Audio de Tiles Naranjas")]
    public AudioClip orangeTileDeathSound;
    
    [Range(0f, 10.0f)]
    [Tooltip("Volumen del sonido de muerte en tiles naranjas")]
    public float orangeTileDeathVolume = 5.0f;

    // Start is called once after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;
        
        // Reproducir música del nivel
        MusicManager.Instance.PlayMusicForCurrentScene();
    
        // Determinar si es la primera vez en el nivel
        bool isFirstTime = LevelTransitionManager.Instance.IsFirstTimeInLevel();
        
        if (isFirstTime)
        {
            // Primera vez: mostrar pantalla de Stage
            StartCoroutine(LevelTransitionManager.Instance.ShowStageScreen(
                SceneManager.GetActiveScene().name,
                OnStageScreenComplete
            ));
        }
        else
        {
            // Reinicio: fade in directo
            StartCoroutine(LevelTransitionManager.Instance.FadeInOnRestart(OnFadeInComplete));
        }
    }
    
    // Callback cuando termina la pantalla de Stage
    void OnStageScreenComplete()
    {
        if (!hasStartedAnimation)
        {
            hasStartedAnimation = true;
            
            CreateMap();
            FindAllBridges();
        }
    }
    
    // Callback cuando termina el fade in en reinicio
    void OnFadeInComplete()
    {
        if (!hasStartedAnimation)
        {
            hasStartedAnimation = true;
            
            CreateMap();
            FindAllBridges();
        }
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
    
    // Nuevo método para crear el UI del contador
    void CreateMoveCounterUI()
    {
        if (!hasCreatedUI && FindObjectOfType<MoveCounterUI>() == null)
        {
            hasCreatedUI = true;
            GameObject uiObj = new GameObject("MoveCounterUI");
            uiObj.AddComponent<MoveCounterUI>();
            Debug.Log("UI del contador de movimientos creado después de la animación");
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
		
		// First pass: find spawn positions for split cubes
		Vector3 cube1SpawnPos = Vector3.zero;
		Vector3 cube2SpawnPos = Vector3.zero;
		bool foundCube1Pos = false;
		bool foundCube2Pos = false;
		
		for(int z=0; z<sizeZ; z++)
		{
		    for(int x=0; x<sizeX; x++)
		    {
		        int tileValue = nums[z * sizeX + x + 2];
		        int xFlipped = (sizeX - 1) - x;
		        
		        if (tileValue == 8)  // Position for small cube 1
		        {
		            cube1SpawnPos = new Vector3(xFlipped, 0.5f, z);
		            foundCube1Pos = true;
		        }
		        else if (tileValue == 9)  // Position for small cube 2
		        {
		            cube2SpawnPos = new Vector3(xFlipped, 0.5f, z);
		            foundCube2Pos = true;
		        }
		    }
		}
		
		// Process the map. For each tileId == 2 create a copy of the tile prefab
        for(int z=0; z<sizeZ; z++)
            for(int x=0; x<sizeX; x++)
            {
                int tileValue = nums[z * sizeX + x + 2];
                
                if (tileValue == 2 || tileValue == 4 ||  tileValue == 5 ||  (tileValue == 6) || tileValue == 8 || tileValue == 9)
                {
                    int xFlipped = (sizeX - 1) - x;   // <--- volteo horizontal

                    // Instantiate the copy at its corresponding location
                    GameObject obj = Instantiate(tile, new Vector3(xFlipped, -0.05f, z), transform.rotation);

                    // Set the new object parent to be the game object containing this script
                    obj.transform.parent = transform;

                    // Añadir el tile a la lista
                    allTiles.Add(obj);
                    
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

                        // Añadir el botón a la lista también
                        allTiles.Add(boton);
                        
                        // Animar el botón desde la misma altura inicial que el tile
                        TileAnimation botonAnim = boton.AddComponent<TileAnimation>();
                        botonAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight + 0.05f);  // Mismo delay, empieza 0.2 más arriba que el tile
                    }
                    else if (tileValue == 5 && botonCruzPrefab != null) // Si es un 5, crear el botón cruz encima del tile
                    {
                        // Crear el botón cruz a la altura final del tile (0.0f) con rotación de 45 grados
                        GameObject botonCruz = Instantiate(botonCruzPrefab, 
                            new Vector3(xFlipped, 0.0f, z),  // Altura final sobre el tile
                            Quaternion.Euler(0, 45, 0)); // Rotar 45 grados en el eje Y
                        botonCruz.transform.parent = transform;
                        botonCruz.name = $"BotonCruz_{x}_{z}";

                        // Añadir el botón a la lista también
                        allTiles.Add(botonCruz);
                        
                        // Animar el botón desde la misma altura inicial que el tile
                        TileAnimation botonCruzAnim = botonCruz.AddComponent<TileAnimation>();
                        botonCruzAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight + 0.05f);
                    }
                    else if (tileValue == 6 && botonDivisorPrefab != null)
                    {
                        // Crear el botón divisor a la altura final del tile (0.0f)
                        GameObject botonDivisor = Instantiate(botonDivisorPrefab, 
                            new Vector3(xFlipped, 0.0f, z),  // Altura final sobre el tile
                            Quaternion.identity);
                        botonDivisor.transform.parent = transform;
                        botonDivisor.name = $"BotonDivisor_{x}_{z}";
                        
                        // Configurar las posiciones de spawn de los cubos
                        BotonDivisor botonScript = botonDivisor.GetComponent<BotonDivisor>();
                        if (botonScript != null)
                        {
                            botonScript.smallCube1Position = cube1SpawnPos;
                            botonScript.smallCube2Position = cube2SpawnPos;
                            Debug.Log($"Botón divisor configurado: Cubo1 en {cube1SpawnPos}, Cubo2 en {cube2SpawnPos}");
                        }
                        
                        // Añadir el botón a la lista también
                        allTiles.Add(botonDivisor);
                        
                        // Animar el botón desde la misma altura inicial que el tile
                        TileAnimation botonDivisorAnim = botonDivisor.AddComponent<TileAnimation>();
                        botonDivisorAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight + 0.05f);
                    }
                }
                else if (tileValue == 7 && tileNaranjaPrefab != null)
                {
                    int xFlipped = (sizeX - 1) - x;
                    
                    GameObject orangeTile = Instantiate(tileNaranjaPrefab, new Vector3(xFlipped, -0.05f, z), transform.rotation);
                    orangeTile.transform.parent = transform;
                    orangeTile.name = $"OrangeTile_{x}_{z}";
                    orangeTile.tag = "OrangeTile"; // Tag especial para tiles naranjas
                    
                    allTiles.Add(orangeTile);
                    
                    TileAnimation tileAnim = orangeTile.AddComponent<TileAnimation>();
                    float delay = (x + z) * delayBetweenTiles;
                    if (delay > maxDelay)
                        maxDelay = delay;
                    tileAnim.StartAnimation(delay, tileAnimationDuration, tileStartHeight);
                    
                    // Añadir el script que detecta si el cubo está vertical
                    OrangeTileBehavior orangeBehavior = orangeTile.AddComponent<OrangeTileBehavior>();
                    
                    // ✨ NUEVO: Asignar el audio de muerte al tile naranja
                    orangeBehavior.deathSound = orangeTileDeathSound;
                    orangeBehavior.deathSoundVolume = orangeTileDeathVolume;
                    
                    Debug.Log($"Tile naranja creado en posición: ({xFlipped}, {z}). Audio asignado: {(orangeTileDeathSound != null ? orangeTileDeathSound.name : "NULL")}");
                }
                else if (tileValue == 3) // Tile de meta (invisible - simula un agujero)
                {
                    int xFlipped = (sizeX - 1) - x;
                    
                    // Crear el tile de meta INVISIBLE (solo collider para detectar llegada)
                    GameObject goalTile = Instantiate(tile, new Vector3(xFlipped, -0.05f, z), transform.rotation);
                    goalTile.transform.parent = transform;
                    goalTile.name = $"GoalTile_{x}_{z}";
                    goalTile.tag = "Goal"; // Importante: asignar tag Goal
                    
                    // Hacer el tile invisible (incluyendo todos sus hijos)
                    Renderer renderer = goalTile.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = false; // Desactivar el renderer para hacerlo invisible
                    }
                    
                    // Desactivar también los renderers de todos los hijos
                    Renderer[] childRenderers = goalTile.GetComponentsInChildren<Renderer>();
                    foreach (Renderer childRenderer in childRenderers)
                    {
                        childRenderer.enabled = false;
                    }
                    
                    // NO añadir el tile de meta a la lista de tiles para que no se anime ni caiga
                    // allTiles.Add(goalTile); 
                    
                    Debug.Log($"Tile de meta (invisible) creado en posición: ({xFlipped}, {z})");
                }
            }
      
      // Calculate total animation time (last tile's delay + animation duration)
      totalAnimationTime = maxDelay + tileAnimationDuration;
      
      // Crear UI del contador DESPUÉS de crear el mapa
      // Usar Invoke para crear el UI después de un frame, asegurando que el fade ya terminó
      Invoke("CreateMoveCounterUIDelayed", 0.1f);
    }
    
    // Nuevo método para crear el UI con delay
    void CreateMoveCounterUIDelayed()
    {
        CreateMoveCounterUI();
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

            // Activar los puentes después de que termine la animación de tiles
            ActivateBridges();

            StartCoroutine(SpawnCubeSound());
        }
    }

    IEnumerator SpawnCubeSound() {
        yield return new WaitForSeconds(0.7f);

        // Reproducir sonido de spawn del cubo
        if (cubeSpawnSound != null)
        {
            AudioSource.PlayClipAtPoint(cubeSpawnSound, cube.transform.position, spawnSoundVolume);
            Debug.Log("Sonido de spawn reproducido para el cubo");
        }
    }
    
    // Método para activar los puentes en su posición inicial
    void ActivateBridges()
    {
        foreach (GameObject bridgeObj in allBridges)
        {
            if (bridgeObj != null)
            {
                bridgeObj.SetActive(true);
                BridgeRiseAnimation bridge = bridgeObj.GetComponent<BridgeRiseAnimation>();
                if (bridge != null)
                {
                    bridge.InitializeBridge();
                }
            }
        }
        
        Debug.Log($"Activados {allBridges.Count} puentes");
    }

    // Método llamado cuando el cubo se cae
    public void OnCubeFell()
    {
        if (isFalling) return; // Evitar múltiples llamadas
        
        isFalling = true;
        Debug.Log("¡El cubo se ha caído! Iniciando animación de caída de tiles...");
        
        // Fade out any small cubes that are still in the scene
        FadeOutSmallCubes();
        
        StartCoroutine(AnimateTilesFalling());
    }
    
    // Method to fade out small cubes when level resets
    void FadeOutSmallCubes()
    {
        // Find all small cubes in the scene
        MoveCubeSmall[] smallCubes = FindObjectsOfType<MoveCubeSmall>();
        
        foreach (MoveCubeSmall smallCube in smallCubes)
        {
            if (smallCube != null && smallCube.gameObject.activeInHierarchy)
            {
                // Add fade out component and start fading
                CubeFadeOut fadeOut = smallCube.gameObject.AddComponent<CubeFadeOut>();
                fadeOut.StartFadeOut(0.5f); // Fade over 0.5 seconds for slower effect
                
                // Disable the movement script so cube doesn't move during fade
                smallCube.enabled = false;
                
                Debug.Log($"Iniciando fade out del cubo pequeño: {smallCube.gameObject.name}");
            }
        }
    }

    // Corrutina para animar la caída de todos los tiles
    IEnumerator AnimateTilesFalling()
    {
        float maxFallDelay = 0f;
        
        // Combinar tiles y puentes en una sola lista
        List<GameObject> allFallingObjects = new List<GameObject>();
        allFallingObjects.AddRange(allTiles);
        
        // Añadir solo puentes que estén activos
        foreach (GameObject bridge in allBridges)
        {
            if (bridge != null && bridge.activeInHierarchy)
            {
                allFallingObjects.Add(bridge);
            }
        }
        
        // Crear una lista con índices aleatorios para el orden de caída
        List<int> randomIndices = new List<int>();
        for (int i = 0; i < allFallingObjects.Count; i++)
        {
            randomIndices.Add(i);
        }
        
        // Mezclar la lista para orden aleatorio
        for (int i = 0; i < randomIndices.Count; i++)
        {
            int temp = randomIndices[i];
            int randomIndex = UnityEngine.Random.Range(i, randomIndices.Count);
            randomIndices[i] = randomIndices[randomIndex];
            randomIndices[randomIndex] = temp;
        }
        
        // Animar cada objeto hacia abajo con delay aleatorio
        for (int i = 0; i < randomIndices.Count; i++)
        {
            int objIndex = randomIndices[i];
            GameObject obj = allFallingObjects[objIndex];
            
            if (obj != null)
            {
                // Usar el mismo patrón de delay que la animación de subida pero más rápido
                float fallDelay = i * delayBetweenFallTiles;
                maxFallDelay = Mathf.Max(maxFallDelay, fallDelay);
                
                // Crear animación de caída
                TileFallAnimation fallAnim = obj.AddComponent<TileFallAnimation>();
                fallAnim.StartFallAnimation(fallDelay, tileFallAnimationDuration, tileFallStartHeight);
                
                Debug.Log($"Objeto {obj.name} comenzará caída en {fallDelay}s");
            }
        }
        
        // Esperar a que termine la animación de caída
        yield return new WaitForSeconds(maxFallDelay + tileFallAnimationDuration + 0.1f);

        // Iniciar fade out a negro antes de reiniciar
        yield return StartCoroutine(FadeToBlackOnFall());

        MoveCounter.Instance.RestartLevel();
        
        // Marcar que NO es primera vez (es un reinicio)
        LevelTransitionManager.Instance.SetFirstTimeInLevel(false);
        
        // Reiniciar la escena
        Debug.Log("Reiniciando escena...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    void FindAllBridges()
    {
        // Buscar todos los GameObjects que contengan "Puente" o "Bridge" en su nombre
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name.Contains("Bridge")) && obj.activeInHierarchy)
            {
                allBridges.Add(obj);
                // Desactivar el puente inicialmente
                obj.SetActive(false);
                Debug.Log($"Puente/Bridge encontrado y desactivado: {obj.name}");
            }
        }
        
        Debug.Log($"Total de puentes encontrados: {allBridges.Count}");
    }

    // Nuevo método para animar la victoria
    public void OnLevelComplete()
    {
        if (isVictoryAnimation) return;
        
        isVictoryAnimation = true;
        Debug.Log("¡Nivel completado! Iniciando animación de victoria...");
        
        StartCoroutine(AnimateVictory());
    }
    
    // Corrutina para animar la victoria
    IEnumerator AnimateVictory()
    {
        // Reproducir sonido de victoria al inicio de la animación
        if (levelCompletedSound != null)
        {
            AudioSource.PlayClipAtPoint(levelCompletedSound, Camera.main.transform.position, levelCompletedSoundVolume);
            Debug.Log("Sonido de nivel completado reproducido");
        }
        
        float maxRiseDelay = 0f;
        
        // Combinar tiles, puentes y botones en una sola lista
        List<GameObject> allObjects = new List<GameObject>();
        allObjects.AddRange(allTiles);
        
        // Añadir puentes activos
        foreach (GameObject bridge in allBridges)
        {
            if (bridge != null && bridge.activeInHierarchy)
            {
                allObjects.Add(bridge);
            }
        }
        
        // Añadir botones
        BotonRedondo[] botonesRedondos = FindObjectsOfType<BotonRedondo>();
        foreach (var boton in botonesRedondos)
        {
            if (boton != null && boton.gameObject.activeInHierarchy)
            {
                allObjects.Add(boton.gameObject);
            }
        }
        
        BotonCruz[] botonesCruz = FindObjectsOfType<BotonCruz>();
        foreach (var boton in botonesCruz)
        {
            if (boton != null && boton.gameObject.activeInHierarchy)
            {
                allObjects.Add(boton.gameObject);
            }
        }
        
        // Crear orden de subida en espiral desde el centro
        List<GameObject> sortedObjects = new List<GameObject>(allObjects);
        
        // Calcular centro del nivel
        Vector3 center = Vector3.zero;
        foreach (var obj in allObjects)
        {
            if (obj != null)
                center += obj.transform.position;
        }
        if (allObjects.Count > 0)
            center /= allObjects.Count;
        
        // Ordenar por distancia al centro (los más cercanos primero)
        sortedObjects.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            float distA = Vector3.Distance(a.transform.position, center);
            float distB = Vector3.Distance(b.transform.position, center);
            return distA.CompareTo(distB);
        });
        
        // Animar cada objeto hacia arriba con delay
        for (int i = 0; i < sortedObjects.Count; i++)
        {
            GameObject obj = sortedObjects[i];
            
            if (obj != null)
            {
                float riseDelay = i * delayBetweenRiseTiles;
                maxRiseDelay = Mathf.Max(maxRiseDelay, riseDelay);
                
                // Deshabilitar componentes de animación existentes
                TileAnimation tileAnim = obj.GetComponent<TileAnimation>();
                if (tileAnim != null) tileAnim.enabled = false;
                
                TileFallAnimation fallAnim = obj.GetComponent<TileFallAnimation>();
                if (fallAnim != null) fallAnim.enabled = false;
                
                // Crear animación de subida
                TileRiseAnimation riseAnim = obj.AddComponent<TileRiseAnimation>();
                riseAnim.StartRiseAnimation(riseDelay, tileRiseAnimationDuration, tileRiseHeight);
            }
        }
        
        // Esperar a que termine la animación de subida
        yield return new WaitForSeconds(maxRiseDelay + tileRiseAnimationDuration + 0.5f);
        
        // Iniciar fade out
        yield return StartCoroutine(FadeToBlack());
        
        // Avanzar al siguiente nivel
        GoToNextLevel();
    }
    
    // Corrutina para hacer fade a negro
    IEnumerator FadeToBlack()
    {
        // Crear un panel negro sobre toda la pantalla
        GameObject fadePanel = new GameObject("FadePanel");
        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        
        // Animar alpha de 0 a 1
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = Color.black;
    }

    // Corrutina para hacer fade a negro (cuando el cubo cae)
    IEnumerator FadeToBlackOnFall()
    {
        // Crear un panel negro sobre toda la pantalla
        GameObject fadePanel = new GameObject("FadePanel_Fall");
        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        
        // Animar alpha de 0 a 1
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDurationOnFall)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeOutDurationOnFall);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = Color.black;
        
        Debug.Log("Fade out completado. Listo para reiniciar nivel.");
    }

    // Nuevo método para avanzar al siguiente nivel
    public void GoToNextLevel()
    {
        MoveCounter.Instance.CompleteLevel();

        // Marcar que SÍ es primera vez en el siguiente nivel
        LevelTransitionManager.Instance.SetFirstTimeInLevel(true);

        // Determinar el siguiente nivel
        string currentSceneName = SceneManager.GetActiveScene().name;
        string nextSceneName = GetNextLevelName(currentSceneName);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("¡Has completado todos los niveles!");
        }
    }
    
    // Método para determinar el nombre del siguiente nivel
    private string GetNextLevelName(string currentLevel)
    {
        // Extraer el número del nivel actual
        if (currentLevel == "Level_10")
        {
            return "End";
        }
        else if (currentLevel.StartsWith("Level_"))
        {
            string numberPart = currentLevel.Substring(6); // Quitar "Level_"
            if (int.TryParse(numberPart, out int currentLevelNumber))
            {
                int nextLevelNumber = currentLevelNumber + 1;
                string nextLevelName = $"Level_{nextLevelNumber}";
                
                // Verificar si el siguiente nivel existe
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    
                    if (sceneName == nextLevelName)
                    {
                        return nextLevelName;
                    }
                }
            }
        }
        
        return null; // No hay más niveles
    }

}
