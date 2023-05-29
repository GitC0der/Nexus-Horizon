using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using DefaultNamespace;
using Painting;
using Unity.VisualScripting;
using UnityEngine;

namespace Prepping
{
    /// <summary>
    ///     Represents an area that will be filled by basic blocks
    /// </summary>
    public class Blockbox
    {
        public readonly int _sizeX;
        public readonly int _sizeY;
        public readonly int _sizeZ;

        private Block[][][] _blocks;
        private Vector3[][][] _shifts;
        private HashSet<Position3> _doorPositions;

        /// <summary>
        ///     Create a blockbox
        /// </summary>
        /// <param name="sizeX">Width in X of the box</param>
        /// <param name="sizeY">Height in Y of the box</param>
        /// <param name="sizeZ">Depth in Z of the box</param>
        public Blockbox(int sizeX, int sizeY, int sizeZ) {
            this._sizeX = sizeX;
            this._sizeY = sizeY;
            this._sizeZ = sizeZ;
            _doorPositions = new HashSet<Position3>();
            
            EmptyBox();
        }

        /// <summary>
        ///     Empty the blockbox, i.e set all blocks to Block.NULL
        /// </summary>
        public void EmptyBox() {
            _blocks = new Block[_sizeX][][];
            _shifts = new Vector3[_sizeX][][];
            for (int x = 0; x < _sizeX; x++) {
                _blocks[x] = new Block[_sizeY][];
                _shifts[x] = new Vector3[_sizeY][];
                for (int y = 0; y < _sizeY; y++) {
                    _blocks[x][y] = new Block[_sizeZ];
                    _shifts[x][y] = new Vector3[_sizeZ];
                    for (int z = 0; z < _sizeZ; z++) {
                        _blocks[x][y][z] = Block.Void;
                        _shifts[x][y][z] = new Vector3(0,0,0);
                    }
                } 
            }

            _doorPositions = new HashSet<Position3>();
        }

        public void SetDoor(Position3[] positions) {
            _doorPositions.AddRange(positions);
        }

        public HashSet<Position3> GetDoorsLeadingTo(IEnumerable<Position3> surfaceBorder) {
            HashSet<Position3> doorsFound = new HashSet<Position3>();
            foreach (Position3 pos in surfaceBorder) {
                foreach (var (neighborPos, _) in GetRelativeNeighbors(pos)) {
                    var newPos = pos + neighborPos;
                    if (newPos.y == pos.y && _doorPositions.Contains(newPos + Position3.up)) doorsFound.Add(newPos);
                }
            }

            return doorsFound;
        }

        /// <summary>
        ///     Return the block at the given location. Must check that the location is inside the box beforehand
        /// </summary>
        /// <param name="position">The examined location</param>
        /// <returns>The block at the given location</returns>
        /// <exception cref="ArgumentException">If the position is not in the box</exception>
        public Block BlockAt(Position3 position) {
            if (!IsInsideBox(position)) {
                throw new ArgumentException($"Position {position} was not inside the blockbox");
            }
            return _blocks[position.x][position.y][position.z];
        }

        public Optional<Vector3> ShiftAt(Position3 position) {
            return !IsInsideBox(position) ? new Optional<Vector3>() : new Optional<Vector3>(_shifts[position.x][position.y][position.z]);
        }
        
        private bool IsInside(int value, int min, int max) {
            return min <= value && value < max;
        }

        public bool IsDoor(Position3 position) => _doorPositions.Contains(position);

        private bool SetBlock(Block block, Position3 position, bool doForce, Vector3 shift = default) {
            if (!IsInsideBox(position)) {
                return false;
            }

            Block currentBlock = BlockAt(position);
            if (!doForce && currentBlock != Block.Void) {
            //if (!doForce && currentBlock != Block.NULL) {
                return false;
            }

            if (currentBlock == Block.Door && _doorPositions.Contains(position)) _doorPositions.Remove(position);

            _blocks[position.x][position.y][position.z] = block;
            _shifts[position.x][position.y][position.z] = shift;
            if (block == Block.Door && !_doorPositions.Contains(position)) _doorPositions.Add(position);
            return true;
        }
        
        /// <summary>
        ///     Forces the generation of the given block, i.e it will replace any block already present at that location
        /// </summary>
        /// <param name="block">The new block</param>
        /// <param name="position">The position of the new block</param>
        /// <param name="shift">The shift of the postion of the block. Especially useful for facades, with balconies and such</param>
        /// <returns>True if a block was already present, false otherwise</returns>
        public bool ForceSetBlock(Block block, Position3 position, Vector3 shift = default) => SetBlock(block, position, true, shift);
        
        /// <summary>
        ///     Tries to place a block at a given location. If a block is already present, does nothing
        /// </summary>
        /// <param name="block">The new block</param>
        /// <param name="position">The position of the new block</param>
        /// /// <param name="shift">The shift of the postion of the block. Especially useful for facades, with balconies and such</param>
        /// <returns>True if the new block was successfully placed, false otherwise</returns>
        public bool TrySetBlock(Block block, Position3 position, Vector3 shift = default) => SetBlock(block, position, false, shift);

        /// <summary>
        ///     Return true if the given position is inside the box, false otherwise
        /// </summary>
        public bool IsInsideBox(Position3 position) {
            return IsInside(position.x, 0, _sizeX) && IsInside(position.y, 0, _sizeY) && IsInside(position.z, 0, _sizeZ);
        }

        public bool IsStrictlyInside(Position3 position) {
            return (position.x != 0 && position.x != _sizeX) && (position.y != 0 && position.y != _sizeY) &&
                   (position.z != 0 && position.z != _sizeZ) && IsInsideBox(position);
        }

        private void Check(Dictionary<Position3, Block> acc, Position3 position, Position3 displ) {
            Position3 newPos = position + displ;
            if (IsInsideBox(newPos)) {
                Block block = _blocks[newPos.x][newPos.y][newPos.z];
                //if (!block.Equals(Block.NULL)) {
                    acc.Add(displ, block);
                //}
            }
        }

        /// <summary>
        ///     Create a dictionary that maps each neighboring block to their location relative to the given position.
        ///     For example, if the block on top of the given position is a Building, it will be mapped this way:
        ///     <code> {new Position(0, 1, 0) -> Building} </code>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Dictionary<Position3, Block> GetRelativeNeighbors(Position3 position) {
            Dictionary<Position3, Block> neighbors = new Dictionary<Position3, Block>();

            Check(neighbors, position, new Position3(1, 0, 0));
            Check(neighbors, position, new Position3(-1, 0, 0));
            Check(neighbors, position, new Position3(0, 1, 0));
            Check(neighbors, position, new Position3(0, -1, 0));
            Check(neighbors, position, new Position3(0, 0, 1));
            Check(neighbors, position, new Position3(0, 0, -1));
            
            return neighbors;
        }
        
    }
}