using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Painting;
using Prepping;
using Prepping.Generators;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;
    private GameObject voidPrefab;

    public Dictionary<Block, GameObject> prefabs;

    private float spawnInterval = 0.001f;
    private float timer = 0.0f;
    private int x = 0;
    private int y = 0;
    private int z = 0;
    private bool isRunning = false;
    private bool isPlacingOne = false;
    private Blockbox blockbox;
    private IGenerator generator;
    private bool instantGeneration = true;

    private HashSet<GameObject> cubes = new HashSet<GameObject>();
    
    
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
        blockbox = new Blockbox(50, 80, 50);
        //blockbox = new Blockbox(30, 30, 30);
        
        isRunning = true;
        
        // Selectects the type of generator
        //generator = new RandomCuboids(blockbox, true);
        //generator = new NeighborDetectionGen(blockbox);

        if (instantGeneration) {
            Regenerate();
            /*
            while (!generator.IsDone()) {
                GenerateBlock();
            }
            OptimizeBlockBox();
            SpawnBlocks();
            */
            //CombineMeshes();
        }
        
        GenerateFacade();
    }
    
    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P)) {
            isRunning = !isRunning;
        }
        
        timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E)) {
            timer = spawnInterval;
            isPlacingOne = true;
            //Debug.Log("hahaha");
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Regenerate();
        }
        
        if (!isRunning && !isPlacingOne) {
            return;
        }

        if (!instantGeneration) {
            GenerateBlock();
        }
        
    }
    
    private List<List<Position3>> FindFacades() {
        /*
         * – Create and a list "allFacades" (a list of all facades)
         * – Create a set "allFacadePositions" (a set of the positions of all blocks present
         *   in at least one facade)
         * – For every block in block box of type "void":
         *      – If currentBlock is of type void, get its neighbours.
         *          – If there is at least one neighbour of type "building":
         *              – Create an empty list "facade"
         *              – For every "building" block adjacent to currentBlock:
         *                  – If that block is already in allFacadeBlocks, skip it.
         *                  – Do a BFS and add the resulting blocks to facade.
         *              – If facade is nonempty, add facade to allFacades
         */
        List<List<Position3>> allFacades = new List<List<Position3>>();
        HashSet<Position3> allFacadePositions = new HashSet<Position3>();
        for (int i = 0; i < blockbox.sizeX; i++) {
            for (int j = 0; j < blockbox.sizeY; j++) {
                for (int k = 0; k < blockbox.sizeZ; k++) {
                    Position3 currentPos = new Position3(i, j, k);
                    Block currentBlock = blockbox.BlockAt(currentPos);
                    
                    // Skip all blocks but the void ones
                    if (currentBlock == Block.Void) {
                        Dictionary<Position3, Block> neighbours = blockbox.GetNeighbors(currentPos);

                        // Only keep blocks of type "building"
                        foreach (var (neighbourPos, neighbourBlock) in neighbours) {
                            if (neighbourBlock != Block.Building) {
                                neighbours.Remove(neighbourPos);
                            }
                        }
                        
                        // Only continue if at there is at least one neighbour remaining
                        if (neighbours.Count > 0) {
                            foreach (var (neighbourPos, _) in neighbours) {
                                if (!allFacadePositions.Contains(neighbourPos)) {
                                    List<Position3> facade = BFS();
                                    
                                    // Add the positions of the new facade blocks to the set
                                    foreach (var pos in facade) {
                                        allFacadePositions.Add(pos);
                                    }
                                     
                                    // Add the facade to the list of all facades
                                    allFacades.Add(facade);
                                }
                            }
                        }
                    }
                }
            }
        }

        return allFacades;
    }

    // Todo: Implement BFS
    private List<Position3> BFS() {
        return new List<Position3>();
    }

    private void GenerateFacade() {
        int height = 50;
        int width = 15;
        Position3 origin = new Position3(-height, 0, -width);
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(WaveFunctionCollapse.Facade1, width, height, new Position2(2, 48), 'D');
        while (!wfc.IsDone()) {
            wfc.GenerateNextSlot();
        }

        char[][] table = wfc.GetOutput();
        for (int x = 0; x < table[0].Length; x++) {
            for (int y = 0; y < table.Length; y++) {
                char c = table[y][x];
                GameObject pref = null;
                switch (c) {
                    case 'D':
                        pref = trainPrefab;
                        break;
                    case 'W':
                        pref = skybridgePrefab;
                        break;
                    case 'C':
                        pref = parkPrefab;
                        break;
                    case '-':
                        pref = buildingPrefab;
                        break;
                    case '@':
                        break;
                    default:
                        break;
                }

                if (pref != null) {
                    GameObject obj = Instantiate(pref, new Position3(origin.x - x,origin.y - y, -10).AsVector3(), Quaternion.identity);
                    cubes.Add(obj);
                }
            }
        }
    }

    private void SpawnBlocks() {
        for (int x = 0; x < blockbox.sizeX; x++) {
            for (int y = 0; y < blockbox.sizeY; y++) {
                for (int z = 0; z < blockbox.sizeZ; z++) {
                    Position3 blockPosition = new Position3(x, y, z);
                    Block block = blockbox.BlockAt(blockPosition);
                    if (block != Block.Void && block != Block.NULL) {
                        GameObject obj = Instantiate(PrefabFrom(block), blockPosition.AsVector3(), Quaternion.identity);
                        cubes.Add(obj);
                    }
                }
            }
        }
    }
    
    private void Regenerate() {
        GameObject[] objects = FindObjectsOfType<GameObject>();
        foreach (GameObject cube in cubes) {
            Destroy(cube);
        }

        cubes = new HashSet<GameObject>();
        /*
        foreach (GameObject obj in objects)
        {
            
            if (obj.name != "Prefabs" && obj.name != "Directional Light" && obj.name != "Main Camera" && obj.name != "DebugManager" && obj.name != "GameManager" && blockbox.IsInsideBox(new Position3(obj.transform.position))) {
                Destroy(obj);
            }
        }
        */
        
        
        blockbox.EmptyBox();
        generator = new AnchoredCuboids(blockbox, true);
        
        while (!generator.IsDone()) {
            GenerateBlock();
        }
        OptimizeBlockBox();
        SpawnBlocks();
        //CombineMeshes();
    }
    
    private void GenerateBlock() {
        Position3 blockPosition = generator.GetNextPosition();
        Block block = generator.GenerateNextBlock();


        GameObject obj;
        if (block != Block.Void) {
            //obj = Instantiate(PrefabFrom(block), blockPosition.AsVector3(), Quaternion.identity);
            //cubes.Add(obj);
        }
        
        ++blockCount;

        if (block != Block.Void) {
            isPlacingOne = false;
        } else {
            isPlacingOne = true;
            timer = spawnInterval;
        }
    }

    private void OptimizeBlockBox() {
        List<Position3> willBeRemoved = new List<Position3>();
        for (int x = 0; x < blockbox.sizeX; x++) {
            for (int y = 0; y < blockbox.sizeY; y++) {
                for (int z = 0; z < blockbox.sizeZ; z++) {
                    var neighbors = blockbox.GetNeighbors(new Position3(x, y, z));
                    if (neighbors.Count == 6 && neighbors.ToList().FindAll(pair => pair.Value == Block.Building).Count == 6) {
                        willBeRemoved.Add(new Position3(x,y,z));
                    }
                }
            }
        }
        foreach (Position3 pos in willBeRemoved) {
            blockbox.ForceSetBlock(Block.Void, pos);
        }    
        
    }

    private void CombineMeshes() {
        List<Mesh> meshes = new List<Mesh>();
        
        foreach (GameObject cube in cubes) {
            MeshFilter cubeMeshFilter = cube.GetComponent<MeshFilter>();
            if (cubeMeshFilter != null) {
                meshes.Add(cubeMeshFilter.sharedMesh);
            }
        }
        
        // Create an array of CombineInstance objects
        CombineInstance[] combineInstances = new CombineInstance[meshes.Count];
        for (int i = 0; i < meshes.Count; i++) {
            combineInstances[i].mesh = meshes[i];
            combineInstances[i].transform = Matrix4x4.identity;
        }

        // Create a new mesh and combine the meshes into it
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances);

        // Set the combined mesh to the MeshFilter component on the empty game object
        MeshFilter combinedMeshFilter = GameObject.Find("CombinedMesh").GetComponent<MeshFilter>();
        combinedMeshFilter.sharedMesh = combinedMesh;
        
        foreach (GameObject cube in cubes) {
            Destroy(cube);
        }

        cubes = new HashSet<GameObject>();
    }


}
