using System.Collections.Generic;
using System.ComponentModel.Design;
using DefaultNamespace;
using UnityEngine;

namespace Prepping
{
    public static class BlockBox
    {
        public const int sizeX = 15;
        public const int sizeY = 15;
        public const int sizeZ = 15;

        private static Block[,,] blocks = new Block[100,100,100];

        public static void Instantiate() {
            for (int x = 0; x < sizeX; x++) {
                for (int y = 0; y < sizeY; y++) {
                    for (int z = 0; z < sizeZ; z++) {
                        blocks[x, y, z] = Block.NULL;
                    }
                } 
            }
        }
        
        private static bool IsInside(int value, int min, int max) {
            return min <= value && value < max;
        }

        public static void AddBlock(Block block, Position3 position) {
            blocks[position.x, position.y, position.z] = block;
        }

        private static void Check(Dictionary<Position3, Block> acc, Position3 position, Position3 displ) {
            Position3 newPos = position + displ;
            if (IsInside(newPos.x, 0, sizeX) && IsInside(newPos.y, 0, sizeY) && IsInside(newPos.z, 0, sizeZ)) {
                Block block = blocks[newPos.x, newPos.y, newPos.z];
                if (!block.Equals(Block.NULL)) {
                    acc.Add(newPos, block);
                }
            }
        }

        public static Dictionary<Position3, Block> GetNeighbors(Position3 position) {
            Dictionary<Position3, Block> neighbors = new Dictionary<Position3, Block>();

            Check(neighbors, position, new Position3(1, 0, 0));
            Check(neighbors, position, new Position3(-1, 0, 0));
            Check(neighbors, position, new Position3(0, 1, 0));
            Check(neighbors, position, new Position3(0, -1, 0));
            Check(neighbors, position, new Position3(0, 0, 1));
            Check(neighbors, position, new Position3(0, 0, -1));
            
            Debug.Log($"Neighbors are {DebugUtils.ToString(neighbors, pos => $"{pos}", block => $"{block}")}");

            /*
            for (int x = -1; x < 1; x++) {
                for (int y = -1; y < 1; y++) {
                    for (int z = -1; z < 1; z++) {
                        if (!(x == 0 && y == 0 && z == 0)) {
                            Position3 newPos = position + new Position3(x, y, z);
                            if (IsInside(newPos.x, 0, sizeX) && IsInside(newPos.y, 0, sizeY) && IsInside(newPos.z, 0, sizeZ)) {
                                Block block = blocks[newPos.x, newPos.y, newPos.z];
                                if (!block.Equals(Block.NULL)) {
                                    neighbors.Add(newPos, block);
                                }
                            }
                        }
                    }
                }
            }
            */

            return neighbors;
        }
    }
}