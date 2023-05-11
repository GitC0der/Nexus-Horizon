using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Painting;
using Prepping;
using Prepping.Generators;
using Unity.VisualScripting;
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
    private HashSet<Facade> _facades = new();

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
            { Block.Void, voidPrefab },
            { Block.Skybridge , skybridgePrefab},
            { Block.Train , trainPrefab},
            { Block.Walkway, walkwayPrefab}
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
    
    private void Regenerate() {
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
        
        GenerateOutsideTestFacade();

        var facades = FindAllFacadesTest();
        
        GenerateSingleRandomFacadeRoof(facades);
        
        OptimizeBlockBox();

        SpawnBlocks();
    }


    private HashSet<Facade> FindAllFacadesTest() {
        List<GameObject> prefabs = new List<GameObject>() {parkPrefab, buildingPrefab, trainPrefab, walkwayPrefab, skybridgePrefab};
        var facades = FindFacades();
        foreach (var facade in facades) {
            
            
            Block block;
            if (facade.GetOrientation() == Orientation.Roof) {
                block = Block.Train;
            } else if (facade.GetOrientation() == Orientation.Floor) {
                block = Block.Park;
            } else if (facade.GetOrientation() == Orientation.WallE) {
                block = Block.Walkway;
            } else if (facade.GetOrientation() == Orientation.WallW) {
                block = Block.Walkway;
            } else if (facade.GetOrientation() == Orientation.WallS) {
                block = Block.Skybridge;
            } else if (facade.GetOrientation() == Orientation.WallN) {
                block = Block.Skybridge;
            } else {
                throw new Exception("WHAT????");
            }

            /*
            foreach (Position3 pos in facade.GetBlocks()) {
                blockbox.ForceSetBlock(block, pos);
            }
            */
            
            
            if (facade.GetOrientation() == Orientation.Roof) {
                block = new List<Block>() { Block.Park, Block.Skybridge, Block.Train, Block.Walkway }[Random.Range(0, 4)];
                block = Block.Train;
                foreach (Position3 pos in facade.GetBlocks()) {
                    //blockbox.ForceSetBlock(Block.Park, pos);
                    blockbox.ForceSetBlock(block, pos);
                }
            }
            
            
        }

        return facades;

    }

    private void GenerateSingleRandomFacadeRoof(HashSet<Facade> facades) {
        var p = 0;
        var roofs = facades.Where(f => f.GetOrientation() == Orientation.Roof).ToList();
        var highEnough = roofs.Where(f => f.GetFixedCoordinate() > 10).ToList();
        var largeEnough = highEnough.Where(f => f.GetWidth() > 6 || f.GetHeight() > 6).ToList();
        
        DrawOneFacade(largeEnough[0]);
    }

    private void DrawOneFacade(Facade facade) {
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(WaveFunctionCollapse.Roof1, facade.GetWidth(),
            facade.GetHeight(), new Position2(0,0), 'B');
        while (!wfc.IsDone()) {
            wfc.GenerateNextSlot();
        }

        for (int x = 0; x < facade.GetWidth(); x++) {
            for (int z = 0; z < facade.GetHeight(); z++) {
                Position2 pos = new Position2();
                Block block;
                var output = wfc.GetOutput();
                if (x < output.Length && z < output[0].Length) {
                    switch (wfc.GetOutput()[x][z]) {
                        case 'B':
                            block = Block.Train;
                            break;
                        case 'S':
                            block = Block.Park;
                            break;
                        case 'C':
                            block = Block.Skybridge;
                            break;
                        case '-':
                            block = Block.Building;
                            break;
                        default:
                            block = Block.Void;
                            break;
                    }
                    
                    
                    if (block != Block.Void) {
                        blockbox.ForceSetBlock(block, facade.GetMinCorner3() + new Position3(x,0,z));
                    }
                    

                    foreach (Position3 p in facade.GetBlocks()) {
                        blockbox.ForceSetBlock(Block.Skybridge, p);
                    }
                }
                
            }
        }
    }

    private HashSet<Facade> FindFacades() {
        /*
            - Create a list containing all facades, which are lists of positions
            - Create a hashset of all blocks in facades
            - For every block of type null:
                - If block has at least one building neighbor
                    - For each of its neighbor building that is NOT already in a facade
                        - Create an empty list for the current facade
                        - Do a DFS to get all the adjacent buildings
                        - Add them all into that list
                        - Add that list to the list of facades
        */

        HashSet<Facade> facades = new();
        HashSet<Position3> blocksInFacades = new();
        
        // Iterating over all blocks
        for (int x = 0; x < blockbox.sizeX; x++) {
            for (int y = 0; y < blockbox.sizeY; y++) {
                for (int z = 0; z < blockbox.sizeX; z++) {
                    Position3 currentPos = new(x, y, z);
                    Dictionary<Position3, Block> neighbors = blockbox.GetNeighbors(currentPos);
                    
                    // Checking all blocks that are null and have at least one neighbor building
                    if (blockbox.BlockAt(currentPos) == Block.Void && neighbors.ContainsValue(Block.Building)) {
                        foreach (var (relativeNeighborPos, block) in neighbors) {
                            if (block == Block.Building && !blocksInFacades.Contains(currentPos + relativeNeighborPos)) {
                                
                                // Retrieving adjacent facade blocks with BFS
                                HashSet<Position3> currentFacade = BfsFacade(currentPos, relativeNeighborPos);
                                facades.Add(new Facade(currentFacade, -relativeNeighborPos));
                                blocksInFacades.AddRange(currentFacade);
                            }
                        }
                    }
                }
            }
        }

        return facades;
    }

    private HashSet<Position3> BfsFacade(Position3 startingPos, Position3 normalDirection) {
        Queue<Position3> queue = new Queue<Position3>();
        HashSet<Position3> currentFacade = new();
        queue.Enqueue(startingPos + normalDirection);
        
        while (queue.Count != 0) {
            Position3 bfsPosition = queue.Dequeue();
            currentFacade.Add(bfsPosition);

            var bfsNeighbors = blockbox.GetNeighbors(bfsPosition);

            // TODO: Fix roof not found because of adjacency with floor
            // Solution: store which of the two block is building beforehand, and add in queue only if the same block is building
            // instead of checking if not both of them are building
            
            foreach (var (relativeBfsNeighborPos, b) in bfsNeighbors) {
                Position3 examined = relativeBfsNeighborPos + bfsPosition;
                
                // Just to avoid errors with the BlockAt below
                if (blockbox.IsInsideBox(examined + normalDirection) &&
                    blockbox.IsInsideBox(examined - normalDirection)) {
                    
                    Block block1 = blockbox.BlockAt(examined + normalDirection);
                    Block block2 = blockbox.BlockAt(examined - normalDirection);
                    if (relativeBfsNeighborPos != normalDirection
                        && relativeBfsNeighborPos != -normalDirection
                        && b == Block.Building
                        && !currentFacade.Contains(examined) && !queue.Contains(examined)
                        && !(block1 == Block.Building && block2 == Block.Building)) {
                        queue.Enqueue(examined);
                    }
                }
            }
            
            /*
            // If condition to avoid errors with the next two "BlockAt" calls
            if (blockbox.IsInsideBox(bfsPosition + normalDirection) && blockbox.IsInsideBox(bfsPosition - normalDirection)) {
                Block block1 = blockbox.BlockAt(bfsPosition + normalDirection);
                Block block2 = blockbox.BlockAt(bfsPosition - normalDirection);
                                        
                // Check if examined block is not hidden behind other blocks
                if (!(block1 == Block.Building && block2 == Block.Building)) {
                    foreach (var (relativeBfsNeighborPos, b) in bfsNeighbors) {
                        Position3 examined = relativeBfsNeighborPos + bfsPosition;
                        if (relativeBfsNeighborPos != normalDirection 
                            && relativeBfsNeighborPos != -normalDirection 
                            && b == Block.Building 
                            && !currentFacade.Contains(examined) && !queue.Contains(examined)) {
                            queue.Enqueue(examined);
                        }
                    }
                }
            }
            */

        }

        return currentFacade;
    }


    private void GenerateOutsideTestFacade() {
        int height = 50;
        int width = 15;
        Position3 origin = new Position3(-height, 30, -width + 70);
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(WaveFunctionCollapse.Facade2, width, height, new Position2(2, 48), 'D');
        while (!wfc.IsDone()) {
            wfc.GenerateNextSlot();
        }

        char[][] table = wfc.GetOutput();
        for (int x = 0; x < table[0].Length; x++) {
            for (int y = 0; y < table.Length; y++) {
                char c = table[y][x];
                GameObject pref = null;
                bool isBalcony = false;
                switch (c) {
                    case 'B':
                        isBalcony = true;
                        pref = buildingPrefab;
                        break;
                    case 'A':
                        pref = walkwayPrefab;
                        break;
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
                        // TODO: Remove this
                        pref = buildingPrefab;
                        isBalcony = false;
                        break;
                    default:
                        isBalcony = false;
                        break;
                }

                if (pref != null) {
                    Vector3 offset = isBalcony ? new Vector3(0, 0, -1): Vector3.zero;
                    GameObject obj = Instantiate(pref, new Position3(origin.x - x,origin.y - y, -10).AsVector3() + offset, Quaternion.identity);
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
                    if (block != Block.Void) {
                    //if (block != Block.Void && block != Block.NULL) {
                        GameObject obj = Instantiate(PrefabFrom(block), blockPosition.AsVector3(), Quaternion.identity);
                        cubes.Add(obj);
                    }
                }
            }
        }
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
