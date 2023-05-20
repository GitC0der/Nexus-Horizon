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
        
        public PropPrefab Lamp() => new PropPrefab(lampPrefab, 1, 4, 1, true);
        public PropPrefab Railing() => new PropPrefab(railingPrefab, 1, 1, 1, false);
        public PropPrefab TableSet() => new PropPrefab(tableSetPrefab, 2, 2, 2, true);
        public PropPrefab LongAirConditioning() => new(longACPrefab, 4, 2, 2, true);

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
        
    }
    
    public class PropPrefab
    {
        private GameObject _prefab;
        private bool _isClearanceHard;
        private int _sizeX;
        private int _sizeY;
        private int _sizeZ;
        
        public PropPrefab(GameObject prefab, int sizeX, int sizeY, int sizeZ, bool isClearanceHard) {
            _prefab = prefab;
            _sizeX = sizeX;
            _sizeY = sizeY;
            _sizeZ = sizeZ;
            _isClearanceHard = isClearanceHard;
        }

        public int SizeX() => _sizeX;
        public int SizeY() => _sizeY;
        public int SizeZ() => _sizeZ;

        public bool IsClearanceHard() => _isClearanceHard;
        public GameObject GameObject() => _prefab;

    }

    public enum PropType
    {
        LampPost, Couch
    }
    
}