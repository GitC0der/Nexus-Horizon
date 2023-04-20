using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject cubePrefab;  // Prefab of the cube to be spawned

    public int numCubesPerLine = 10;      // Number of cubes to spawn per line
    public int numLines = 10;      // Number of lines to spawn
    private float spawnInterval = 0.1f;  // Time between each cube spawn

    private float timer = 0.0f;     // Timer to track time between spawns
    private int widthX = 10;
    private int widthY = 10;
    private int widthZ = 10;

    private int x = 0;
    private int y = 0;
    private int z = 0;

    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0;
            SpawnCube(transform.position + x * Vector3.right + y * Vector3.forward + z * Vector3.up);
            ++x;
            if (x >= widthX) {
                x = 0;
                ++y;
            }


            if (y >= widthY) {
                y = 0;
                ++z;
            }

            if (z > widthZ) {
                z = 0;
                timer = -100000;
            }
        }
    }

    // Spawn a single cube
    void SpawnCube(Vector3 position) {
        // Spawn a new cube
        Instantiate(cubePrefab, position, Quaternion.identity);
    }
}