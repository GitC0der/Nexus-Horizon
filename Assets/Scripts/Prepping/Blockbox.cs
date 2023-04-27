using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using DefaultNamespace;
using UnityEngine;

namespace Prepping
{
    public class Blockbox
    {
        public readonly int sizeX;
        public readonly int sizeY;
        public readonly int sizeZ;

        private Block[,,] blocks;

        public Blockbox(int sizeX, int sizeY, int sizeZ) {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            
            blocks = new Block[sizeX, sizeY, sizeZ];
            EmptyBox();
        }

        public void EmptyBox() {
            for (int x = 0; x < sizeX; x++) {
                for (int y = 0; y < sizeY; y++) {
                    for (int z = 0; z < sizeZ; z++) {
                        blocks[x, y, z] = Block.NULL;
                    }
                } 
            }
        }

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
            if (!doForce && currentBlock != Block.NULL) {
                return false;
            }

            blocks[position.x, position.y, position.z] = block;
            return true;
        }

        public bool ForceSetBlock(Block block, Position3 position) => SetBlock(block, position, true);
        public bool TrySetBlock(Block block, Position3 position) => SetBlock(block, position, false);

        public bool IsInsideBox(Position3 position) {
            return IsInside(position.x, 0, sizeX) && IsInside(position.y, 0, sizeY) && IsInside(position.z, 0, sizeZ);
        }

        private void Check(Dictionary<Position3, Block> acc, Position3 position, Position3 displ) {
            Position3 newPos = position + displ;
            if (IsInsideBox(newPos)) {
                Block block = blocks[newPos.x, newPos.y, newPos.z];
                if (!block.Equals(Block.NULL)) {
                    acc.Add(newPos, block);
                }
            }
        }

        public Dictionary<Position3, Block> GetNeighbors(Position3 position) {
            Dictionary<Position3, Block> neighbors = new Dictionary<Position3, Block>();

            Check(neighbors, position, new Position3(1, 0, 0));
            Check(neighbors, position, new Position3(-1, 0, 0));
            Check(neighbors, position, new Position3(0, 1, 0));
            Check(neighbors, position, new Position3(0, -1, 0));
            Check(neighbors, position, new Position3(0, 0, 1));
            Check(neighbors, position, new Position3(0, 0, -1));
            
            //Debug.Log($"Neighbors are {DebugUtils.ToString(neighbors, pos => $"{pos}", block => $"{block}")}");

            return neighbors;
        }
    }
}