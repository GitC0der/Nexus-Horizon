using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;
    
    private GameObject[] prefabs;
    private int nbPrefabs = 5;

    private float spawnInterval = 0.1f;  // Time between each cube spawn

    private float timer = 0.0f;     // Timer to track time between spawns
    private int widthX = 3;
    private int widthY = 3;
    private int widthZ = 3;

    private int x = 0;
    private int y = 0;
    private int z = 0;

    // Use this for initialization
    void Start () {
        // Initiasize the prefabs array with the five given gameObjects
        prefabs = new GameObject[] { buildingPrefab, parkPrefab, trainPrefab, skybridgePrefab, walkwayPrefab };
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

            if (z >= widthZ) {
                z = 0;
                timer = -100000;
            }
        }
    }

    // Spawn a single cube
    void SpawnCube(Vector3 position) {
        int rand = Random.Range(0, nbPrefabs);
        // Spawn a new cube using a randomly selected prefab from the prefabs array
        Instantiate(prefabs[rand], position, Quaternion.identity);
    }
}