using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;

    private List<GameObject> prefabs;
    private Dictionary<string, float> map = new Dictionary<string, float>()
    {
        { "Building", 0.4f },
        { "Park", 0.15f },
        { "Train", 0.15f },
        { "SkyBridge", 0.1f },
        { "Walkway", 0.2f }
    };

    private float spawnInterval = 0.1f; // Time between each cube spawn
    private float timer = 0.0f; // Timer to track time between spawns
    private int widthX = 3;
    private int widthY = 3;
    private int widthZ = 3;
    private int x = 0;
    private int y = 0;
    private int z = 0;

    private string lastPrefabName = ""; // Store the name of the last prefab placed

    // Use this for initialisation
    void Start () {
        prefabs = new List<GameObject>() { buildingPrefab, parkPrefab, trainPrefab, skybridgePrefab, walkwayPrefab };
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0;

            // Determine the probability distribution for the next prefab
            Dictionary<string, float> nextMap = new Dictionary<string, float>(map); // Create a copy of the original map
            if (!string.IsNullOrEmpty(lastPrefabName))
            {
                // Double the probability of the same type of prefab
                float oldProbability = nextMap[lastPrefabName];
                nextMap[lastPrefabName] = Mathf.Min(1.0f, oldProbability * 2);
            }

            // Choose the next prefab based on the probability distribution
            SpawnPrefab(transform.position + x * Vector3.right + y * Vector3.forward + z * Vector3.up, nextMap);
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
        float totalProbability = 0.0f;
        foreach (var item in probabilityMap)
        {
            totalProbability += item.Value;
        }

        float randomValue = Random.Range(0.0f, totalProbability);

        float cumulativeProbability = 0.0f;
        foreach (var item in probabilityMap)
        {
            cumulativeProbability += item.Value;
            if (randomValue < cumulativeProbability)
            {
                lastPrefabName = item.Key; // Store the name of the last prefab placed
                Instantiate(prefabs.Find(p => p.name.Contains(item.Key)), position, Quaternion.identity);
                break;
            }
        }
    }
}
