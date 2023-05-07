using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

namespace Prepping
{
    /// <summary>
    ///     Represents an area that will be filled by basic blocks
    /// </summary>
    public class Blockbox
    {
        public readonly int sizeX;
        public readonly int sizeY;
        public readonly int sizeZ;

        private Block[,,] blocks;
        
        /// <summary>
        ///     Create a blockbox
        /// </summary>
        /// <param name="sizeX">Width in X of the box</param>
        /// <param name="sizeY">Height in Y of the box</param>
        /// <param name="sizeZ">Depth in Z of the box</param>
        public Blockbox(int sizeX, int sizeY, int sizeZ) {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            
            blocks = new Block[sizeX, sizeY, sizeZ];
            EmptyBox();
        }

        /// <summary>
        ///     Empty the blockbox, i.e set all blocks to Block.NULL
        /// </summary>
        public void EmptyBox() {
            for (int x = 0; x < sizeX; x++) {
                for (int y = 0; y < sizeY; y++) {
                    for (int z = 0; z < sizeZ; z++) {
                        blocks[x, y, z] = Block.Void;
                        //blocks[x, y, z] = Block.NULL;
                    }
                } 
            }
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
            return blocks[position.x, position.y, position.z];
        } 
        
        private bool IsInside(int value, int min, int max) {
            return min <= value && value < max;
        }

        private bool SetBlock(Block block, Position3 position, bool doForce) {
            if (!IsInsideBox(position)) {
                return false;
            }

            Block currentBlock = BlockAt(position);
            if (!doForce && currentBlock != Block.Void) {
            //if (!doForce && currentBlock != Block.NULL) {
                return false;
            }

            blocks[position.x, position.y, position.z] = block;
            return true;
        }

        /// <summary>
        ///     Forces the generation of the given block, i.e it will replace any block already present at that location
        /// </summary>
        /// <param name="block">The new block</param>
        /// <param name="position">The position of the new block</param>
        /// <returns>True if a block was already present, false otherwise</returns>
        public bool ForceSetBlock(Block block, Position3 position) => SetBlock(block, position, true);
        
        /// <summary>
        ///     Tries to place a block at a given location. If a block is already present, does nothing
        /// </summary>
        /// <param name="block">The new block</param>
        /// <param name="position">The position of the new block</param>
        /// <returns>True if the new block was successfully placed, false otherwise</returns>
        public bool TrySetBlock(Block block, Position3 position) => SetBlock(block, position, false);

        /// <summary>
        ///     Return true if the given position is inside the box, false otherwise
        /// </summary>
        public bool IsInsideBox(Position3 position) {
            return IsInside(position.x, 0, sizeX) && IsInside(position.y, 0, sizeY) && IsInside(position.z, 0, sizeZ);
        }

        public bool IsStrictlyInside(Position3 position) {
            return (position.x != 0 && position.x != sizeX) && (position.y != 0 && position.y != sizeY) &&
                   (position.z != 0 && position.z != sizeZ) && IsInsideBox(position);
        }

        private void Check(Dictionary<Position3, Block> acc, Position3 position, Position3 displ) {
            Position3 newPos = position + displ;
            if (IsInsideBox(newPos)) {
                Block block = blocks[newPos.x, newPos.y, newPos.z];
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
        public Dictionary<Position3, Block> GetNeighbors(Position3 position) {
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