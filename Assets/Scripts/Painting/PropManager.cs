using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Prepping;
using UnityEngine;

namespace Painting
{
    public class PropManager : MonoBehaviour
    {
        public GameObject lampPrefab;
        public GameObject railingPrefab;
        public GameObject tableSetPrefab;
        public GameObject propHolder;
        public GameObject longACPrefab;
        public GameObject coolingUnit;
        public GameObject couch1;
        public GameObject waterTower;
        public GameObject plant;
        public GameObject wallLamp;
        
        public PropPrefab Lamp() => new (lampPrefab, new Vector3(0,2,0), 1, 4, 1, true);
        public PropPrefab Railing() => new (railingPrefab, new Vector3(-1,-1,-1), 1, 1, 1, false);
        public PropPrefab TableSet() => new (tableSetPrefab, new Vector3(0.5f,0.5f,-0.5f), 2, 2, 2, true);
        public PropPrefab LongAirConditioning() => new(longACPrefab, new Vector3(-0.53f,0.33f,-0.65f),3, 2, 1, true);
        public PropPrefab LargeCoolingUnit() => new(coolingUnit, new Vector3(1, 0.45f, -1), 3, 2, 3, true);
        public PropPrefab Couch1() => new(couch1, new Vector3(-0.4f, 0.5f, -2.2f), 2, 2, 3, true);
        public PropPrefab WaterTower() => new(waterTower, new Vector3(1,3,0.3f),3,5,3, true);
        public PropPrefab Plant() => new(plant, new Vector3(-0.3f, 0.35f, -0.1f), 1, 2, 1, true);
        public PropPrefab WallLamp() => new(wallLamp, new Vector3(0, 2, 0.5f), 1, 1, 1, false);
        
        private PropBox _propBox;

        void Start() {
            
        }

        public void Initialize(Blockbox blockbox) {
            _propBox = new PropBox(blockbox, propHolder);
        }

        [CanBeNull]
        public GameObject Instantiate(PropPrefab prefab, Position3 anchorPos, Vector3 position, Vector3 facing, HashSet<Position3> surfaceBlocks) {
            return _propBox.AddProp(prefab, anchorPos, position, facing, surfaceBlocks);
        }

        public void RemoveAllProps() {
            _propBox.RemoveAllProps();
        }

        public bool RemoveProp(GameObject prop) => _propBox.RemoveProp(prop);
        
        public ActualProp PropAt(Position3 position) => _propBox.PropAt(position);

        public HashSet<ActualProp> RailingsAt(Position3 position) => _propBox.RailingsAt(position);
        public bool RemovePropsAt(Position3 position) {
            return _propBox.RemovePropsAt(position);
        }
        
    }
    
    public class PropPrefab
    {
        private GameObject _prefab;
        private bool _isClearanceHard;
        private int _sizeX;
        private int _sizeY;
        private int _sizeZ;
        private Vector3 _offset;
        
        public PropPrefab(GameObject prefab, Vector3 offset, int sizeX, int sizeY, int sizeZ, bool isClearanceHard) {
            _prefab = prefab;
            _sizeX = sizeX;
            _sizeY = sizeY;
            _sizeZ = sizeZ;
            _isClearanceHard = isClearanceHard;
            _offset = offset;
        }

        public int SizeX() => _sizeX;
        public int SizeY() => _sizeY;
        public int SizeZ() => _sizeZ;

        public bool IsClearanceHard() => _isClearanceHard;
        public Vector3 Offset() => _offset;
        public GameObject GameObject() => _prefab;

    }

    public enum PropType
    {
        LampPost, Couch
    }
    
}