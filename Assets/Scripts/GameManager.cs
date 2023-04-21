using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data;
using Prepping;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;
    private GameObject voidPrefab;

    public Dictionary<Block, GameObject> prefabs;

    private float spawnInterval = 0.01f;
    private float timer = 0.0f;
    private int x = 0;
    private int y = 0;
    private int z = 0;
    private bool isRunning = false;
    private bool isPlacingOne = false;
    
    
    // DEBUGGING
    private int blockCount = 0;

    private string lastPrefabName = "";

    private GameObject PrefabFrom(Block block) {
        GameObject prefab;
        prefabs.TryGetValue(block, out prefab);
        return prefab;
    }
    
    // DEBUGGING
    //[Obsolete("This method is for debugging only.")]
    public Position3 CurrentPos() => new Position3(x, y, z);
    
    // Use this for initialisation
    void Start () {
        prefabs = new Dictionary<Block, GameObject>() {
            { Block.Building, buildingPrefab },
            { Block.Park, parkPrefab },
            { Block.Void, voidPrefab }
        };
        BlockBox.Instantiate();

        Position3 position = new Position3(0, 0, 0);
        Block block = Block.Building;
        BlockBox.AddBlock(block, position);
        Instantiate(PrefabFrom(block), position.AsVector3(), Quaternion.identity);
        ++blockCount;
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown("p")) {
            isRunning = !isRunning;
        }
        
        timer += Time.deltaTime;

        if (Input.GetKeyDown("e")) {
            timer = spawnInterval;
            isPlacingOne = true;
            Debug.Log("hahaha");
        }
        
        if (!isRunning && !isPlacingOne) {
            return;
        }

        if (timer >= spawnInterval) {
            timer = 0;

            ++x;
            if (x >= BlockBox.sizeX) {
                x = 0;
                ++z;
            }

            if (z >= BlockBox.sizeZ) {
                z = 0;
                ++y;
            }

            if (y >= BlockBox.sizeY) {
                y = 0;
                timer = -100000;
                isRunning = false;
            }
            
            // DEBUGGING
            if (z == 1) {
                var p = 0;
            }

            Debug.Log("-------- New block ---------");
            Position3 currentPos = new Position3(x, y, z);
            Dictionary<Position3, Block> neighbors = BlockBox.GetNeighbors(currentPos);
            Block block = Distribution.PickBlock(neighbors, currentPos);
            BlockBox.AddBlock(block, currentPos);
            if (block == Block.NULL) {
                throw new Exception("Fatal ERROR: a NULL block was generated");
            }
            if (block != Block.Void) {
                Instantiate(PrefabFrom(block), currentPos.AsVector3(), Quaternion.identity);
            }

            ++blockCount;

            if (block != Block.Void) {
                isPlacingOne = false;
            } else {
                isPlacingOne = true;
                timer = spawnInterval;
            }

        }
        
        


    }


}
