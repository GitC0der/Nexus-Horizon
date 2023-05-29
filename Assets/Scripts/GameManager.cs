using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Interactions;
using JetBrains.Annotations;
using Painting;
using Prepping;
using Prepping.Generators;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Light = UnityEngine.Light;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public bool useShader;
    public bool useLights;
    public bool debugMode;
    public bool isDeterministic;
    //public GraphicsLevel graphicsLevel;

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
    private InteractionsManager _interactionsManager;
    
    private GameObject voidPrefab;

    public Dictionary<Block, GameObject> prefabs;
    private Dictionary<string, GameObject> _propPrefabs;

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

    private Dictionary<Position3, GameObject> _cubes = new();
    private readonly Dictionary<string, Vector3> _offsets = new Dictionary<string, Vector3> {
        { "lamp", new Vector3(0, 1, 0) },
        { "plant", new Vector3(0.25f, -0.75f, 0.25f)},
        { "railing", new Vector3(-0.4f, -0.5f, -1f)}
    };


    public GameObject PrefabFrom(Block block) {
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

        _interactionsManager = GetComponentInChildren<InteractionsManager>();
        _interactionsManager.Initialize();
        
        // Stored there for easy access in any
        SL.RegisterService(this);
        SL.RegisterService(_propManager);
        SL.RegisterService(_interactionsManager);
        
        prefabs = new Dictionary<Block, GameObject> {
            { Block.Building, buildingPrefab },
            { Block.Park, parkPrefab },
            { Block.Void, voidPrefab },
            { Block.Window , skybridgePrefab},
            { Block.Door , trainPrefab},
            { Block.Walkway, walkwayPrefab},
            { Block.Plaza, plazaPrefab},
            { Block.Utilities , utilitiesPrefab}
        };
        
        _propPrefabs = new Dictionary<string, GameObject>();

        GameObject prefabParent = GameObject.Find("Prop Prefabs");
        if (prefabParent != null) {
            foreach (Transform child in prefabParent.transform) {
                _propPrefabs[child.name] = child.gameObject;
            }
        }

        if (isDeterministic) {
            Random.InitState(9365);
        }
        
        isRunning = true;

        if (useShader) {
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

    public Blockbox GetBlockbox() => blockbox;

    public List<GameObject> GetPropPrefabs() => _propPrefabs.Values.ToList();

    public GameObject BlockGameObjectAt(Position3 pos) {
        if (_cubes.ContainsKey(pos)) return _cubes[pos];
        return null;
    }
    
    private void Regenerate() {
        foreach (var (_, cube) in _cubes) {
            Destroy(cube);
        }
        
        for (int i = GameObject.Find("Cube Holder").transform.childCount - 1; i >= 0; i--)
        {
            Transform child = GameObject.Find("Cube Holder").transform.GetChild(i);
            Destroy(child.gameObject);
        }
        
        
        _propManager.RemoveAllProps();

        _cubes = new();
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
        
        // GenerateOutsideTestsurface();
        GenerateWfcDemoTerrace();
        
        //var surfaces = FindAllsurfacesTest();
        var surfaces = Findsurfaces();
        _surfaces = surfaces;

        if (true) {
            GenerateAllFacades(surfaces);
            //DrawSurfaceNormals(surfaces);
        } else {
            GenerateAllWallBorders(surfaces);
            
        }
        GenerateAllFloors(surfaces);
        
        
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
                block = Block.Door;
            } else if (surface.GetOrientation() == Orientation.Floor) {
                block = Block.Park;
            } else if (surface.GetOrientation() == Orientation.WallE) {
                block = Block.Walkway;
            } else if (surface.GetOrientation() == Orientation.WallW) {
                block = Block.Walkway;
            } else if (surface.GetOrientation() == Orientation.WallS) {
                block = Block.Window;
            } else if (surface.GetOrientation() == Orientation.WallN) {
                block = Block.Window;
            } else {
                throw new Exception("WHAT????");
            }

            /*
            foreach (Position3 pos in surface.GetBlocks()) {
                blockbox.ForceSetBlock(block, pos);
            }
            */
            
            
            if (surface.GetOrientation() == Orientation.Roof) {
                block = new List<Block>() { Block.Park, Block.Window, Block.Door, Block.Walkway }[Random.Range(0, 4)];
                block = Block.Door;
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
                    Block block = Block.Door;
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
                            block = Block.Door;
                            break;
                        case BorderType.Ground:
                            block = Block.Window;
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
                            block = Block.Window;
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
                FloorPainter fp = new FloorPainter(surface, blockbox, _propManager, useLights);
                if (useShader) {
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

    [CanBeNull]
    public Surface FindSurfaceBelow(Position3 pos) {
        foreach (Surface surface in _surfaces) {
            if (surface.IsFloor() && surface.Contains(pos + Position3.down)) return surface;
        }

        return null;
    }
    
    private void GenerateAllFacades(HashSet<Surface> allSurfaces) {
        foreach (Surface surface in allSurfaces) {
            if (surface.IsFacade() && surface.GetWidth() > 4 && surface.GetHeight() > 4) {
                FacadePainter painter = new FacadePainter(surface, blockbox);
                painter.AddToBlockbox(blockbox);
            }
        }
    }

    private void DrawSurfaceNormals(HashSet<Surface> surfaces) {
        var newSurfaces = surfaces.Where(s => s.GetWidth() > 3 && s.GetHeight() > 3).ToList();
        foreach (Surface surface in newSurfaces) {
            Position3 pos = surface.GetBlocks().ToList()[surface.GetBlocks().Count / 2];
            if (blockbox.IsInsideBox(pos + 2 * surface.GetNormal())) {
                blockbox.ForceSetBlock(Block.Utilities, pos + surface.GetNormal(), Vector3.zero);
                blockbox.ForceSetBlock(Block.Utilities, pos + 2*surface.GetNormal(), Vector3.zero);
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
                            block = Block.Door;
                            break;
                        case 'S':
                            block = Block.Park;
                            break;
                        case 'C':
                            block = Block.Window;
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
                        blockbox.ForceSetBlock(Block.Window, p);
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
        for (int x = 0; x < blockbox._sizeX; x++) {
            for (int y = 0; y < blockbox._sizeY; y++) {
                for (int z = 0; z < blockbox._sizeX; z++) {
                    Position3 currentPos = new(x, y, z);
                    Dictionary<Position3, Block> neighbors = blockbox.GetRelativeNeighbors(currentPos);
                    
                    // Checking all blocks that are void and have at least one neighbor building
                    if (blockbox.BlockAt(currentPos) == Block.Void && neighbors.ContainsValue(Block.Building)) {
                        foreach (var (relativeNeighborPos, block) in neighbors) {
                            if (block == Block.Building && !blocksInsurfaces.Contains(currentPos + relativeNeighborPos)) {
                                
                                // Retrieving adjacent surface blocks with BFS
                                HashSet<Position3> currentSurface = BfsSurface(currentPos, relativeNeighborPos);
                                if (currentSurface.Count > 1) {
                                    surfaces.Add(new Surface(currentSurface, -relativeNeighborPos, blockbox));
                                    blocksInsurfaces.AddRange(currentSurface);
                                }
                            }
                        }
                    }
                }
            }
        }

        return surfaces;
    }

    public Surface GetSurfaceOn(Position3 position) {
        return _surfaces.FirstOrDefault(surface => surface.Contains(position));
    }

    public void RemoveAllPropsOn(Surface surface) {
        if (surface == null) return;
        foreach (Position3 pos in surface.GetBlocks()) {
            _propManager.RemovePropsAt(pos + Position3.up);
        }
    }

    public bool ReplaceBlockAt(Position3 position, Vector3 shift, Block block) {
        blockbox.ForceSetBlock(block, position, shift);
        if (_cubes.ContainsKey(position)) {
            Destroy(_cubes[position]);
            _cubes.Remove(position);
        }

        var cube = Instantiate(PrefabFrom(block), position.AsVector3() + shift, Quaternion.identity, GameObject.Find("Cube Holder").transform);
        if (cube != null) _cubes.Add(position, cube);
        return true;

    }

    private HashSet<Position3> BfsSurface(Position3 startingPos, Position3 normalDirection) {
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

    private void GenerateOutsideTestSurface() {
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
                    case 'X':
                        pref = parkPrefab;
                        break;
                    case 'o' or 'O':
                        pref = trainPrefab;
                        break;
                    case '@':
                        // TODO: Remove this
                        //pref = buildingPrefab;
                        pref = plazaPrefab;
                        isBalcony = false;
                        break;
                    default:
                        isBalcony = false;
                        break;
                }

                if (pref != null) {
                    Vector3 offset = isBalcony ? new Vector3(0, 0, -1): Vector3.zero;
                    Position3 position = new Position3(origin.x - x, origin.y - y, -10);
                    GameObject obj = Instantiate(pref, position.AsVector3() + offset, Quaternion.identity);
                    _cubes.Add(position,obj);
                }
            }
        }
    }

    private bool IsAtBorder(Vector3 pos, Vector3 origin, float range) {
        float epsilon = 0.001f;
        return Mathf.Abs(origin.x - pos.x) < epsilon || Mathf.Abs((origin.x - (range - 1)) - pos.x) < epsilon
            || Mathf.Abs(origin.y - pos.z) < epsilon || Mathf.Abs((origin.y - (range - 1)) - pos.z) < epsilon;
    }

    private void GenerateWfcDemoTerrace() {
        // Create the surface
        const int startX = -20, startZ = 0, y = 0, range = 30;
        HashSet<Position3> blocks = new HashSet<Position3>();
        for (int x = startX; x < startX + range; x++) {
            for (int z = startZ; z < startZ + range; z++) {
                blocks.Add(new Position3(x, y, z));
            }
        }
        Surface surface = new Surface(blocks, Position3.up, blockbox);
        
        // Apply Wave Function Collapse to get an output
        Position3 origin = new Position3(startX, y, startZ);
        char initialChar = '-';
        Position2 initialPos = new Position2(0, 0);
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(WaveFunctionCollapse.DemoTerrace, surface.GetWidth(),
            surface.GetHeight(), initialPos, initialChar);
        while (!wfc.IsDone()) {
            wfc.GenerateNextSlot();
        }

        // Use the WFC output to create the scene
        char[][] table = wfc.GetOutput();
        for (int x = startX; x < startX + table[0].Length; x++) {
            for (int z = startZ; z < startZ + table.Length; z++) {
                char c = table[z - startZ][x - startX];
                GameObject prefCube;
                GameObject prefModel = null;
                Vector3 offset = new Vector3();
                switch (c) {
                    // Lamp
                    case 'L':
                        prefCube = lightPrefab;
                        prefModel = _propManager.lampPrefab;
                        offset = _offsets["lamp"];
                        break;
                    // Plant
                    case 'P':
                        prefCube = parkPrefab;
                        prefModel = _propManager.plant;
                        offset = _offsets["plant"];
                        break;
                    // Table
                    case 'T':
                        prefCube = plazaPrefab;
                        break;
                    case 'S':
                        prefCube = skybridgePrefab;
                        break;
                    case '-':
                        prefCube = buildingPrefab;
                        break;
                    case '@':
                        prefCube = trainPrefab;
                        break;
                    default:
                        prefCube = voidPrefab;
                        break;
                }
                
                var posCube = new Position3(origin.x - x + startX, y, origin.z - z + startZ);
                if (prefCube != null) {
                    // If at the border set the orientation of the lamps
                    if (IsAtBorder(posCube.AsVector3(), origin.AsVector3(), range) && prefModel == _propManager.lampPrefab) {
                        GameObject o = Instantiate(lightPrefab, posCube.AsVector3(), Quaternion.identity);
                        _cubes.Add(posCube, o);
                        
                        // Place the lamp model
                        var posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
                        var facing = Vector3.forward;
                        var rot = Quaternion.identity;
                        
                        // Find the correct facing and rotation
                        if (x == startX) {
                            facing = Vector3.left;
                        } else if (x == startX + (range - 1)) {
                            facing = Vector3.right;
                            rot = Quaternion.Euler(0f, 180f, 0f);
                        } else if (z == startZ) {
                            facing = Vector3.back;
                            rot = Quaternion.Euler(0f, -90f, 0f);
                        } else if (z == startZ + (range - 1)) {
                            facing = Vector3.forward;
                            rot = Quaternion.Euler(0f, 90f, 0f);
                        }
                        
                        var objModel = Instantiate(prefModel, ActualPos(posModel.AsVector3(),
                            _offsets["lamp"], facing), rot);
                        _cubes.Add(posModel, objModel);
                    } else {
                        // Place the cube floor to demonstrate the algorithm
                        GameObject objCube = Instantiate(prefCube, posCube.AsVector3(), Quaternion.identity);
                        _cubes.Add(posCube, objCube);

                        // Place corresponding models
                        var posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
                        if (prefModel != null) {
                            // Todo: Find the correct facing
                            var objModel = Instantiate(prefModel, ActualPos(posModel.AsVector3(),
                                offset, Vector3.left), Quaternion.identity);
                            _cubes.Add(posModel, objModel);
                        }
                    }
                }
            }
        }
        
        // Manually place the borders
        for (int z = startZ - 1; z < startZ + range + 1; z++) {
            // Instantiate upper border
            var posCube = new Position3(origin.x + 1, y, origin.z - z);
            GameObject o = Instantiate(utilitiesPrefab, posCube.AsVector3(), Quaternion.identity);
            _cubes.Add(posCube, o);
            
            // Place the railing model
            var posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
            var facing = Vector3.forward;
            var rot = Quaternion.identity;
            var objModel = Instantiate(_propManager.railingPrefab, ActualPos(posModel.AsVector3(),
                _offsets["railing"], facing), rot);
            _cubes.Add(posModel, objModel);
            
            // Instantiate lower border
            posCube = new Position3(origin.x - range, y, origin.z - z);
            o = Instantiate(utilitiesPrefab, posCube.AsVector3(), Quaternion.identity);
            _cubes.Add(posCube, o);
            
            // Place the railing model
            posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
            facing = Vector3.back;
            rot = Quaternion.Euler(0f, 180f, 0f);
            objModel = Instantiate(_propManager.railingPrefab, ActualPos(posModel.AsVector3(),
                _offsets["railing"], facing), rot);
            _cubes.Add(posModel, objModel);
        }

        for (int x = startX; x < startX + range; x++) {
            // Instantiate upper border
            var posCube = new Position3(origin.x - x + startX, y, origin.z + 1);
            GameObject o = Instantiate(utilitiesPrefab, posCube.AsVector3(), Quaternion.identity);
            _cubes.Add(posCube, o);
            
            // Place the railing model
            var posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
            var facing = Vector3.left;
            var rot = Quaternion.Euler(0f, -90f, 0f);
            var objModel = Instantiate(_propManager.railingPrefab, ActualPos(posModel.AsVector3(),
                _offsets["railing"], facing), rot);
            _cubes.Add(posModel, objModel);
        
            // Instantiate lower border
            posCube = new Position3(origin.x - x + startX, y, origin.z - range);
            o = Instantiate(utilitiesPrefab, posCube.AsVector3(), Quaternion.identity);
            _cubes.Add(posCube, o);
            
            // Place the railing model
            posModel = new Position3(posCube.x, posCube.y + 1, posCube.z);
            facing = Vector3.right;
            rot = Quaternion.Euler(0f, 90f, 0f);
            objModel = Instantiate(_propManager.railingPrefab, ActualPos(posModel.AsVector3(),
                _offsets["railing"], facing), rot);
            _cubes.Add(posModel, objModel);
        }
    }
    
    private Vector3 ActualPos(Vector3 pos, Vector3 offset, Vector3 facing) {
        return pos + offset.x * facing.RotatedLeft() + offset.y * Vector3.up + offset.z * facing;
    }

    private HashSet<Position3> BfsTerrace(Position3 startingPos) {
        Position3 normalDirection = Position3.up;
        Queue<Position3> queue = new Queue<Position3>();
        HashSet<Position3> currentSurface = new();
        queue.Enqueue(startingPos + normalDirection);
        
        while (queue.Count != 0) {
            Position3 bfsPosition = queue.Dequeue();
            currentSurface.Add(bfsPosition);
            
            // Todo: Should not use the blockbox

            var bfsNeighbors = blockbox.GetRelativeNeighbors(bfsPosition);
            foreach (var (relativeBfsNeighborPos, b) in bfsNeighbors) {
                Position3 examined = relativeBfsNeighborPos + bfsPosition;
                
                Block block1 = blockbox.BlockAt(examined + normalDirection);
                Block block2 = blockbox.BlockAt(examined - normalDirection);
                if (relativeBfsNeighborPos != normalDirection
                    && relativeBfsNeighborPos != -normalDirection
                    && b == Block.Building
                    && !currentSurface.Contains(examined) && !queue.Contains(examined)
                    && !(block1 == Block.Building && block2 == Block.Building)) {
                    queue.Enqueue(examined);
                }
            }
        }
        return currentSurface;
    }

    private void SpawnBlocks() {
        GameObject cubeHolder = GameObject.Find("Cube Holder");
        for (int x = 0; x < blockbox._sizeX; x++) {
            for (int y = 0; y < blockbox._sizeY; y++) {
                for (int z = 0; z < blockbox._sizeZ; z++) {
                    Position3 blockPosition = new Position3(x, y, z);
                    Block block = blockbox.BlockAt(blockPosition);
                    var shift = blockbox.ShiftAt(blockPosition);
                    if (shift.Exist() && block != Block.Void) {
                        GameObject obj = Instantiate(PrefabFrom(block), blockPosition.AsVector3() + shift.Get(), Quaternion.identity, cubeHolder.transform);
                        _cubes.Add(blockPosition, obj);
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
        for (int x = 0; x < blockbox._sizeX; x++) {
            for (int y = 0; y < blockbox._sizeY; y++) {
                for (int z = 0; z < blockbox._sizeZ; z++) {
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