using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;
    private GameObject voidPrefab;

    private List<GameObject> prefabs;

    private float spawnInterval = 0.33f;
    private float timer = 0.0f;
    private int widthX = 3;
    private int widthY = 3;
    private int widthZ = 3;
    private int x = 0;
    private int y = 0;
    private int z = 0;

    private string lastPrefabName = "";
    
    private Dictionary<string, Dictionary<string, float>> probabilities = new Dictionary<string, Dictionary<string, float>>()
    {
        { "Building", new Dictionary<string, float> {
            { "Building", 0.5f },
            { "Park", 0.1f },
            { "Train", 0.1f },
            { "SkyBridge", 0.1f },
            { "Walkway", 0.1f },
            { "Void", 0.1f}
        }},
        { "Park", new Dictionary<string, float> {
            { "Building", 0.1f },
            { "Park", 0.5f },
            { "Train", 0.1f },
            { "SkyBridge", 0.1f },
            { "Walkway", 0.1f },
            { "Void", 0.1f}
        }},
        { "Train", new Dictionary<string, float> {
            { "Building", 0.1f },
            { "Park", 0.1f },
            { "Train", 0.5f },
            { "SkyBridge", 0.1f },
            { "Walkway", 0.1f },
            { "Void", 0.1f}
        }},
        { "SkyBridge", new Dictionary<string, float> {
            { "Building", 0.1f },
            { "Park", 0.1f },
            { "Train", 0.1f },
            { "SkyBridge", 0.5f },
            { "Walkway", 0.1f },
            { "Void", 0.1f}
        }},
        { "Walkway", new Dictionary<string, float> {
            { "Building", 0.1f },
            { "Park", 0.1f },
            { "Train", 0.1f },
            { "SkyBridge", 0.1f },
            { "Walkway", 0.5f },
            { "Void", 0.1f}
        }},
        { "Void", new Dictionary<string, float> {
            { "Building", 0.1f },
            { "Park", 0.1f },
            { "Train", 0.1f },
            { "SkyBridge", 0.1f },
            { "Walkway", 0.1f },
            { "Void", 0.5f}
        }}
    };
    
    // Use this for initialisation
    void Start () {
        prefabs = new List<GameObject>() { buildingPrefab, parkPrefab, trainPrefab, skybridgePrefab, walkwayPrefab, voidPrefab };
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)  {
            timer = 0;

            // Determine the probability distribution for the next prefab
            Dictionary<string, float> nextProbabilities;
            if (string.IsNullOrEmpty(lastPrefabName)) {
                // If this is the first prefab, use the default probabilities
                nextProbabilities = new Dictionary<string, float>(probabilities["Building"]);
            } else {
                // Otherwise use the probabilities for the current prefab type
                nextProbabilities = new Dictionary<string, float>(probabilities[lastPrefabName]);
            }

            // Choose the next prefab based on the probability distribution associated to the previous prefab
            SpawnPrefab(transform.position + x * Vector3.right + y * Vector3.forward + z * Vector3.up, nextProbabilities);
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

    void SpawnPrefab(Vector3 position, Dictionary<string, float> probabilityMap)
    {
        // This is to account for potential errors where the total probability isn't equal to 1
        float totalProba = 0.0f;
        foreach (var item in probabilityMap) {
            totalProba += item.Value;
        }

        // Select a random value to determine the next prefab
        float randomValue = Random.Range(0.0f, totalProba);
        float cumulativeProbas = 0.0f;
        
        foreach (var item in probabilityMap) {
            cumulativeProbas += item.Value;
            
            // Determine which Prefab's probabilities to select from the probabilityMap
            if (randomValue < cumulativeProbas) {
                lastPrefabName = item.Key; // Store the name of the last prefab placed
                
                // If it is of type "Void", do not place anything
                if (item.Key != "Void") {
                    Instantiate(prefabs.Find(p => p.name.Contains(item.Key)), position, Quaternion.identity);
                }
                break;
            }
        }
    }
}
