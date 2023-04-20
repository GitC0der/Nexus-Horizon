using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject cubePrefab;  // Prefab of the cube to be spawned
    private int numCubesPerLine = 3;      // Number of cubes to spawn per line
    private int numLines = 3;      // Number of lines to spawn
    private int numLayers = 3; // Number of layers to spawn
    private float spawnInterval = 0.3f;  // Time between each cube spawn

    private float timer = 0.0f;     // Timer to track time between spawns
    private int cubesSpawned = 0;   // Number of cubes spawned so far
    private int linesSpawned = 0;   // Number of lines spawned so far
    private int layersSpawned = 0; // Number of layers spawned so far

    // Use this for initialization
    void Start () {
        InvokeRepeating("SpawnCube", 0.0f, spawnInterval);
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    // Spawn a single cube
    void SpawnCube()
    {
        // Spawn a new cube
        Vector3 nextCubePos = transform.position + Vector3.right * cubesSpawned + Vector3.forward * linesSpawned + new Vector3(0, layersSpawned, 0);
        Instantiate(cubePrefab, nextCubePos, Quaternion.identity);
        cubesSpawned++;
        
        Debug.Log(nextCubePos);
        
        // Start setting certain counters to their default values once all cubes in the current line have been spawned
        if (cubesSpawned >= numCubesPerLine) {
            // Reset the cube counter
            cubesSpawned = 0;
            linesSpawned++;
            
            // If we've spawned all the lines
            if (linesSpawned >= numLines) {
                // Reset the cube and lines counters
                cubesSpawned = 0;
                linesSpawned = 0;
                layersSpawned++;

                if (layersSpawned >= numLayers) {
                    // Cancel the repeating invocation of the SpawnCube method
                    CancelInvoke("SpawnCube");
                }
            }
        }
    }
}