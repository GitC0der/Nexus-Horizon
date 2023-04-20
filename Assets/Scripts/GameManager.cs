using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject cubePrefab;  // Prefab of the cube to be spawned
    public int numCubesPerLine = 10;      // Number of cubes to spawn per line
    public int numLines = 10;      // Number of lines to spawn
    private float spawnInterval = 0.3f;  // Time between each cube spawn

    private float timer = 0.0f;     // Timer to track time between spawns
    private int cubesSpawned = 0;   // Number of cubes spawned so far
    private int linesSpawned = 0;   // Number of lines spawned so far

    // Use this for initialization
    void Start () {
        InvokeRepeating("SpawnCube", 0.0f, spawnInterval);
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    // Spawn a single cube
    void SpawnCube() {
        // Spawn a new cube
        Instantiate(cubePrefab, transform.position + Vector3.right * cubesSpawned + Vector3.forward * linesSpawned, Quaternion.identity);
        cubesSpawned++;
        // If we've spawned all the cubes in the current line
        if (cubesSpawned >= numCubesPerLine) {
            // Reset the cube counter
            cubesSpawned = 0;
            linesSpawned++;
            // If we've spawned all the lines
            if (linesSpawned >= numLines) {
                // Cancel the repeating invocation of the SpawnCube method
                CancelInvoke("SpawnCube");
            }
        }
    }
}