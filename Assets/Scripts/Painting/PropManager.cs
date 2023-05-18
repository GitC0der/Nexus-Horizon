using System;
using Prepping;
using UnityEngine;

namespace Painting
{
    public class PropManager : MonoBehaviour
    {
        public GameObject lampPrefab;
        public GameObject railingPrefab;
        public GameObject propHolder;
        
        public PropPrefab Lamp() => new PropPrefab(lampPrefab, 0.3, 3.4, 1);

        public PropPrefab Railing() => new PropPrefab(railingPrefab, 1.6, 1.2, 0.3);

        private PropBox _propBox;

        void Start() {
            
        }

        public void Initialize(Blockbox blockbox) {
            _propBox = new PropBox(blockbox, propHolder);
        }

        public GameObject Instantiate(PropPrefab prefab, Vector3 position, Vector3 facing) {
            return _propBox.AddProp(prefab, position, facing);
        }

        public void RemoveAllProps() {
            _propBox.RemoveAllProps();
        }
        
    }
    
    public class PropPrefab
    {
        private GameObject _prefab;
        private double _sizeX;
        private double _sizeY;
        private double _sizeZ;
        
        public PropPrefab(GameObject prefab, double sizeX, double sizeY, double sizeZ) {
            _prefab = prefab;
        }

        public double SizeX() => _sizeX;
        public double SizeY() => _sizeY;
        public double SizeZ() => _sizeZ;
        public GameObject GameObject() => _prefab;

    }

    public enum PropType
    {
        LampPost, Couch
    }
    
}