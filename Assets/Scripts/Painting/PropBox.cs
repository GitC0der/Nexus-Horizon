using System.Collections.Generic;
using Prepping;
using UnityEngine;

namespace Painting
{
    public class PropBox
    {
        private Blockbox _blockbox;
        private HashSet<ActualProp> _props;
        private GameObject _propHolder;

        public PropBox(Blockbox blockbox, GameObject propHolder) {
            _blockbox = blockbox;
            _propHolder = propHolder;
            _props = new HashSet<ActualProp>();
        }

        public GameObject AddProp(PropPrefab prefab, Vector3 position, Vector3 facing) {
            var prop = new ActualProp(prefab, _propHolder, position, facing);
            _props.Add(prop);
            return prop.GetGameObject();
        }
    }

    public class ActualProp
    {
        private GameObject _gameObject;

        public ActualProp(PropPrefab prefab, GameObject parent, Vector3 position, Vector3 facing) {
            _gameObject = Object.Instantiate(prefab.GameObject(), position, Quaternion.identity, parent.transform);
                
            // Calculate the target rotation based on the given direction
            Quaternion targetRotation = Quaternion.LookRotation(facing, Vector3.up);

            // TODO: Figure out why we need to do this
            // Adjust the rotation by subtracting 90 degrees from the Y-axis rotation
            Quaternion adjustedRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y - 90f, 0f);

            // Set the rotation of the GameObject to the adjusted rotation
            _gameObject.transform.rotation = adjustedRotation;
        }

        public GameObject GetGameObject() => _gameObject;
    }

}