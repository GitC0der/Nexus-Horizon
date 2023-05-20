﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Prepping;
using Unity.VisualScripting;
using UnityEngine;

namespace Painting
{
    public class PropBox
    {
        private Blockbox _blockbox;
        private HashSet<ActualProp> _props;
        private GameObject _propHolder;
        private HashSet<Position3> _occupiedBlocks;

        public PropBox(Blockbox blockbox, GameObject propHolder) {
            _blockbox = blockbox;
            _propHolder = propHolder;
            _props = new HashSet<ActualProp>();
            _occupiedBlocks = new HashSet<Position3>();
        }

        [CanBeNull]
        public GameObject AddProp(PropPrefab prefab, Position3 anchorPos, Vector3 position, Vector3 facing, HashSet<Position3> surfaceBlocks) {
            HashSet<Position3> blocksIfPossible = CanPlace(prefab, anchorPos, facing, surfaceBlocks);
            if (blocksIfPossible.Count != 0) {
                var prop = new ActualProp(prefab, _propHolder, position, facing);
                _props.Add(prop);
                if (prefab.IsClearanceHard()) _occupiedBlocks.AddRange(blocksIfPossible);
                
                //TODO: DEBUG
                foreach (Position3 pos in _occupiedBlocks) {
                    _blockbox.ForceSetBlock(Block.Utilities, pos);
                }
                
                return prop.GetGameObject();
            }

            return null;
        }
        
        public HashSet<Position3> CanPlace(PropPrefab prefab, Position3 pos, Vector3 facing, HashSet<Position3> surfaceBlocks) {
            HashSet<Position3> positions = new HashSet<Position3>();
            
            for (int x = 0; x < prefab.SizeX(); x++) {
                for (int y = 1; y < prefab.SizeY() + 1; y++) {
                    for (int z = 0; z < prefab.SizeZ(); z++) {
                        Vector3 displ = new Vector3(x, y, z);
                        switch (facing) {
                            case var _ when facing == new Vector3(-1,0,0):
                                displ = new Vector3(displ.z, displ.y, -displ.x);
                                break;
                            case var _ when facing == new Vector3(0,0,1):
                                displ = new Vector3(-displ.x, displ.y, -displ.z);
                                break;
                            case var _ when facing == new Vector3(1,0,0):
                                displ = new Vector3(-displ.z, displ.y, displ.x);
                                break;
                        }
                        Position3 newPos = pos + new Position3(displ);
                        
                        // TODO: This works ONLY for floors. Not facades!
                        if (!surfaceBlocks.Contains(new Position3(newPos.x, pos.y, newPos.z)) || _blockbox.BlockAt(newPos + Position3.up) != Block.Void || (prefab.IsClearanceHard() && _occupiedBlocks.Contains(newPos))) {
                            return new HashSet<Position3>();
                        }
                        
                        positions.Add(newPos);
                    }
                }
            }

            if (prefab.SizeX() == 0 && prefab.SizeY() == 0 && prefab.SizeZ() == 0) positions.Add(pos);

            return positions;

        }

        public void RemoveAllProps() {
            foreach (ActualProp prop in _props) {
                Object.Destroy(prop.GetGameObject());
            }

            _props = new HashSet<ActualProp>();
        }
    }

    public class ActualProp
    {
        private GameObject _gameObject;

        public ActualProp(PropPrefab prefab, GameObject parent, Vector3 position, Vector3 facing) {
            //_gameObject = Object.Instantiate(prefab.GameObject(), position, Quaternion.identity, parent.transform);
            _gameObject = Object.Instantiate(prefab.GameObject(), position, prefab.GameObject().transform.rotation, parent.transform);

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