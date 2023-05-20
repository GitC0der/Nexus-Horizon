using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Painting;
using Prepping;
using Prepping.Generators;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Light = UnityEngine.Light;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public bool highGraphicsMode;
    public GraphicsLevel graphicsLevel;

    public GameObject buildingPrefab;
    public GameObject parkPrefab;
    public GameObject trainPrefab;
    public GameObject skybridgePrefab;
    public GameObject walkwayPrefab;
    public GameObject plazaPrefab;
    public GameObject utilitiesPrefab;
    public GameObject lightPrefab;

    public enum GraphicsLevel
    {
        Low, Medium, High, Ultra
    }

    private PropManager _propManager;
    
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
    private HashSet<Surface> _surfaces = new();

    private HashSet<GameObject> cubes = new HashSet<GameObject>();
    
    

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
        blockbox = new Blockbox(50, 80, 50);
        
        _propManager = GetComponentInChildren<PropManager>();
        _propManager.Initialize(blockbox);
        
        // Stored there for easy access in any
        ServiceLocator.RegisterService(this);
        ServiceLocator.RegisterService(_propManager);
        
        prefabs = new Dictionary<Block, GameObject> {
            { Block.Building, buildingPrefab },
            { Block.Park, parkPrefab },
            { Block.Void, voidPrefab },
            { Block.Skybridge , skybridgePrefab},
            { Block.Train , trainPrefab},
            { Block.Walkway, walkwayPrefab},
            { Block.Plaza, plazaPrefab},
            { Block.Utilities , utilitiesPrefab}
        };
        
        isRunning = true;

        if (highGraphicsMode) {
            GetComponentInChildren<ShaderController>().EnableShader();
        }
        
        if (instantGeneration) {
            Regenerate();
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
            //GenerateBlock();
        }
        
    }
    
    private void Regenerate() {
        foreach (GameObject cube in cubes) {
            Destroy(cube);
        }
        
        _propManager.RemoveAllProps();

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
        
        //GenerateOutsideTestsurface();
        
        //var surfaces = FindAllsurfacesTest();
        var surfaces = Findsurfaces();
        
        //GenerateSingleRandomsurfaceRoof(surfaces);
        GenerateAllFacades(surfaces);
        GenerateAllFloors(surfaces);
        
        //GenerateAllWallBorders(surfaces);
        
        //GenerateFloorBorders(BorderType.None, surfaces);
        
        OptimizeBlockBox();
        
        SpawnBlocks();
        
        //CombineMeshes();
        
        Lightmapping.BakeAsync();

    }


    private HashSet<Surface> FindAllsurfacesTest() {
        List<GameObject> prefabs = new List<GameObject>() {parkPrefab, buildingPrefab, trainPrefab, walkwayPrefab, skybridgePrefab};
        var surfaces = Findsurfaces();
        foreach (var surface in surfaces) {
            
            
            Block block;
            if (surface.GetOrientation() == Orientation.Roof) {
                block = Block.Train;
            } else if (surface.GetOrientation() == Orientation.Floor) {
                block = Block.Park;
            } else if (surface.GetOrientation() == Orientation.WallE) {
                block = Block.Walkway;
            } else if (surface.GetOrientation() == Orientation.WallW) {
                block = Block.Walkway;
            } else if (surface.GetOrientation() == Orientation.WallS) {
                block = Block.Skybridge;
            } else if (surface.GetOrientation() == Orientation.WallN) {
                block = Block.Skybridge;
            } else {
                throw new Exception("WHAT????");
            }

            /*
            foreach (Position3 pos in surface.GetBlocks()) {
                blockbox.ForceSetBlock(block, pos);
            }
            */
            
            
            if (surface.GetOrientation() == Orientation.Roof) {
                block = new List<Block>() { Block.Park, Block.Skybridge, Block.Train, Block.Walkway }[Random.Range(0, 4)];
                block = Block.Train;
                foreach (Position3 pos in surface.GetBlocks()) {
                    //blockbox.ForceSetBlock(Block.Park, pos);
                    blockbox.ForceSetBlock(block, pos);
                }
            }
            
            
        }

        return surfaces;

    }

    private void GenerateFloorBorders(BorderType borderType, HashSet<Surface> surfaces) {
        foreach (Surface surface in surfaces) {
            if (surface.IsFloor()) {
                foreach (Position3 position in surface.GetBorder(borderType)?.GetPositions() ?? new HashSet<Position3>()) {
                    Block block = Block.Train;
                    blockbox.ForceSetBlock(block, position);
                }
            }
        }
    }
    
    private void GenerateAllWallBorders(HashSet<Surface> surfaces) {
        foreach (Surface surface in surfaces) {
            if (surface.IsFacade()  && surface.GetWidth() > 2 && surface.GetHeight() > 2) {
                foreach (var (borderType, border) in surface.GetBorders()) {
                    Block block;
                    switch (borderType) {
                        case BorderType.Ceiling:
                            block = Block.Park;
                            break;
                        case BorderType.Top:
                            block = Block.Train;
                            break;
                        case BorderType.Ground:
                            block = Block.Skybridge;
                            break;
                        case BorderType.Overhang:
                            block = Block.Walkway;
                            break;
                        case BorderType.Wall:
                            block = Block.Plaza;
                            break;
                        case BorderType.None:
                            block = Block.Utilities;
                            break;
                        default:
                            block = Block.Skybridge;
                            break;
                    }

                    foreach (Position3 pos in border.GetPositions()) {
                        blockbox.ForceSetBlock(block, pos);
                    }
                }
                /*
                var borders = surface.GetBorders();
                foreach (var (position, borderType) in borders) {
                    Block block;
                    switch (borderType) {
                        case BorderType.Ceiling:
                            block = Block.Park;
                            break;
                        case BorderType.Top:
                            block = Block.Train;
                            break;
                        case BorderType.Ground:
                            block = Block.Skybridge;
                            break;
                        case BorderType.Overhang:
                            block = Block.Walkway;
                            break;
                        case BorderType.Wall:
                            block = Block.Plaza;
                            break;
                        case BorderType.None:
                            block = Block.Utilities;
                            break;
                        default:
                            block = Block.Skybridge;
                            break;
                        
                    }
                    blockbox.ForceSetBlock(block, position);

                }
                */
            }
        }
    }

    private void GenerateAllFloors(HashSet<Surface> allSurfaces) {
        foreach (Surface surface in allSurfaces) {
            if (surface.IsFloor() && surface.GetBlocks().Count > 2) {
                FloorPainter fp = new FloorPainter(surface, blockbox, _propManager,highGraphicsMode);
                if (highGraphicsMode) {
                    var lights = fp.GetLights();
                    foreach (var (pos, light) in lights) {
                        var lightObject = Instantiate(lightPrefab, pos, Quaternion.identity);
                        lightObject.GetComponent<Light>().color = light.GetColor();
                        lightObject.GetComponent<Light>().range = light.GetRadius();
                    } 
                }
                
            }
        }
        
        
        
        Lightmapping.BakeAsync();
    }
    
    private void GenerateAllFacades(HashSet<Surface> allSurfaces) {
        foreach (Surface surface in allSurfaces) {
            if (surface.IsFacade() && surface.GetWidth() > 4 && surface.GetHeight() > 4) {
                FacadePainter painter = new FacadePainter(surface, blockbox);
                painter.AddToBlockbox(blockbox);
            }
        }
    }

    private void GenerateSingleRandomSurfaceRoof(HashSet<Surface> surfaces) {
        var p = 0;
        var roofs = surfaces.Where(f => f.GetOrientation() == Orientation.Roof).ToList();
        var highEnough = roofs.Where(f => f.GetFixedCoordinate() > 10).ToList();
        var largeEnough = highEnough.Where(f => f.GetWidth() > 6 || f.GetHeight() > 6).ToList();
        
        DrawOneSurface(largeEnough[0]);
    }

    private void DrawOneSurface(Surface surface) {
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(WaveFunctionCollapse.Roof1, surface.GetWidth(),
            surface.GetHeight(), new Position2(0,0), 'B');
        while (!wfc.IsDone()) {
            wfc.GenerateNextSlot();
        }

        for (int x = 0; x < surface.GetWidth(); x++) {
            for (int z = 0; z < surface.GetHeight(); z++) {
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
                        blockbox.ForceSetBlock(block, surface.GetMinCorner3() + new Position3(x,0,z));
                    }
                    

                    foreach (Position3 p in surface.GetBlocks()) {
                        blockbox.ForceSetBlock(Block.Skybridge, p);
                    }
                }
                
            }
        }
    }

    private HashSet<Surface> Findsurfaces() {
        /*
            - Create a list containing all surfaces, which are lists of positions
            - Create a hashset of all blocks in surfaces
            - For every block of type null:
                - If block has at least one building neighbor
                    - For each of its neighbor building that is NOT already in a surface
                        - Create an empty list for the current surface
                        - Do a DFS to get all the adjacent buildings
                        - Add them all into that list
                        - Add that list to the list of surfaces
        */

        HashSet<Surface> surfaces = new();
        HashSet<Position3> blocksInsurfaces = new();
        
        // Iterating over all blocks
        for (int x = 0; x < blockbox.sizeX; x++) {
            for (int y = 0; y < blockbox.sizeY; y++) {
                for (int z = 0; z < blockbox.sizeX; z++) {
                    Position3 currentPos = new(x, y, z);
                    Dictionary<Position3, Block> neighbors = blockbox.GetRelativeNeighbors(currentPos);
                    
                    // Checking all blocks that are null and have at least one neighbor building
                    if (blockbox.BlockAt(currentPos) == Block.Void && neighbors.ContainsValue(Block.Building)) {
                        foreach (var (relativeNeighborPos, block) in neighbors) {
                            if (block == Block.Building && !blocksInsurfaces.Contains(currentPos + relativeNeighborPos)) {
                                
                                // Retrieving adjacent surface blocks with BFS
                                HashSet<Position3> currentsurface = Bfssurface(currentPos, relativeNeighborPos);
                                if (currentsurface.Count > 1) {
                                    surfaces.Add(new Surface(currentsurface, -relativeNeighborPos, blockbox));
                                    blocksInsurfaces.AddRange(currentsurface);
                                }
                            }
                        }
                    }
                }
            }
        }

        return surfaces;
    }

    private HashSet<Position3> Bfssurface(Position3 startingPos, Position3 normalDirection) {
        Queue<Position3> queue = new Queue<Position3>();
        HashSet<Position3> currentsurface = new();
        queue.Enqueue(startingPos + normalDirection);
        
        while (queue.Count != 0) {
            Position3 bfsPosition = queue.Dequeue();
            currentsurface.Add(bfsPosition);

            var bfsNeighbors = blockbox.GetRelativeNeighbors(bfsPosition);

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
                        && !currentsurface.Contains(examined) && !queue.Contains(examined)
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
                            && !currentsurface.Contains(examined) && !queue.Contains(examined)) {
                            queue.Enqueue(examined);
                        }
                    }
                }
            }
            */

        }

        return currentsurface;
    }


    private void GenerateOutsideTestsurface() {
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
        GameObject cubeHolder = GameObject.Find("Cube Holder");
        for (int x = 0; x < blockbox.sizeX; x++) {
            for (int y = 0; y < blockbox.sizeY; y++) {
                for (int z = 0; z < blockbox.sizeZ; z++) {
                    Position3 blockPosition = new Position3(x, y, z);
                    Block block = blockbox.BlockAt(blockPosition);
                    if (block != Block.Void) {
                        GameObject obj = Instantiate(PrefabFrom(block), blockPosition.AsVector3(), Quaternion.identity, cubeHolder.transform);
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
                    var neighbors = blockbox.GetRelativeNeighbors(new Position3(x, y, z));
                    if (neighbors.Count() == 6 && neighbors.Count(pair => pair.Value == Block.Building) == 6) {
                        willBeRemoved.Add(new Position3(x, y, z));
                    }
                }
            }
        }
        foreach (Position3 pos in willBeRemoved) {
            blockbox.ForceSetBlock(Block.Void, pos);
        }
    }
    
    private void CombineMeshes() {
        // Get all the child cubes in Cube Holder
        GameObject cubeHolder = GameObject.Find("Cube Holder");
        Transform[] cubeTransforms = cubeHolder.GetComponentsInChildren<Transform>();
    
        List<MeshFilter> meshFilters = new List<MeshFilter>();
    
        foreach (Transform cubeTransform in cubeTransforms) {
            MeshFilter cubeMeshFilter = cubeTransform.gameObject.GetComponent<MeshFilter>();
            if (cubeMeshFilter != null) {
                meshFilters.Add(cubeMeshFilter);
            }
        }
    
        // Create an array of CombineInstance objects
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++) {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
    
        // Create a new mesh and combine the meshes into it
        Mesh combinedMesh = new Mesh {
            indexFormat = IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(combineInstances, true, true);
    
        // Set the combined mesh to the MeshFilter component on the empty game object
        GameObject combinedMeshObj = new GameObject("CombinedMesh");
        MeshFilter combinedMeshFilter = combinedMeshObj.AddComponent<MeshFilter>();
        combinedMeshFilter.sharedMesh = combinedMesh;
        combinedMeshObj.AddComponent<MeshRenderer>();
    
        foreach (Transform cubeTransform in cubeTransforms) {
            Destroy(cubeTransform.gameObject);
        }
    }
    


}
