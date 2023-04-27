﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Prepping.Generators
{
    public class AnchoredCuboids : IGenerator
    {
        private Position3 previousPosition;
        private bool isDone = false;
        private double recordedVolume = 0;
        private bool anchored;
        private List<Position3> buildingBlocks = new List<Position3>();


        // Parameters
        private int minCuboidSizeY = 6;
        private int maxCuboidSizeY = 30;
        private int minCuboidSizeX = 6;
        private int maxCuboidSizeX = 20;
        private int minCuboidSizeZ = 6;
        private int maxCuboidSizeZ = 15;
        private int minCuboidVolume = 20;
        private int maxCuboidVolume = 2000;
        private float minThreshTotalVolume = 0.30f;
        private float maxThreshTotalVolume = 0.30f;

        public AnchoredCuboids(Blockbox blockbox, bool anchored) : base(blockbox) {
            

            previousPosition = new Position3(-1, 0, 0);

            float blockboxVolume = blockbox.sizeX * blockbox.sizeY * blockbox.sizeZ;
            double threshVolume = Random.Range(minThreshTotalVolume * blockboxVolume, maxThreshTotalVolume * blockboxVolume);


            for (int x = 0; x < blockbox.sizeX; x++) {
                for (int z = 0; z < blockbox.sizeZ; z++) {
                    Position3 pos = new Position3(x, 0, z);
                    buildingBlocks.Add(pos);
                    blockbox.TrySetBlock(Block.Building, pos);
                }
            }
            
            while (recordedVolume < threshVolume) {
                var DEBUG = true;


                Position3 startPos;
                if (anchored) {
                    startPos = buildingBlocks[Random.Range(0, buildingBlocks.Count)];
                } else {
                    int startX = Random.Range(0, blockbox.sizeX - minCuboidSizeX - 1);
                    int startY = Random.Range(0, blockbox.sizeX - minCuboidSizeX - 1);
                    int startZ = Random.Range(0, blockbox.sizeX - minCuboidSizeX - 1);
                    startPos = new Position3(startX, startY, startZ);
                }
                GenerateCuboid(startPos, true, true);
            }
            
        }

        private int GenerateCuboid(Position3 anchor, bool randomDirection, bool volumeBased) {
            float buildingSizeX = 0;
            float buildingSizeY = 0;
            float buildingSizeZ = 0;
            if (volumeBased) {
                float buildingVolume = Random.Range(minCuboidVolume, maxCuboidVolume);
                float maxHeight = buildingVolume / (minCuboidSizeX * minCuboidSizeZ);
                maxHeight = Math.Clamp(maxHeight, minCuboidSizeY, maxCuboidSizeY); 
                buildingSizeY = (int)Math.Round(Random.Range(minCuboidSizeY, maxHeight));

                float maxWidthX = buildingVolume / (buildingSizeY * minCuboidSizeX);
                maxWidthX = Math.Clamp(maxWidthX, minCuboidSizeX, maxCuboidSizeX);
                buildingSizeX = (float)Math.Round(Random.Range(minCuboidSizeX, maxWidthX));

                buildingSizeZ = (float)Math.Round(buildingVolume / (buildingSizeY * buildingSizeX));
                buildingSizeZ = Math.Clamp(buildingSizeZ, minCuboidSizeZ, maxCuboidSizeZ);
            } else {
                buildingSizeX = Random.Range(minCuboidSizeX, maxCuboidSizeX);
                buildingSizeY = Random.Range(minCuboidSizeY, maxCuboidSizeY);
                buildingSizeZ = Random.Range(minCuboidSizeZ, maxCuboidSizeZ);
            }
            int minX = anchor.x;
            int minY = anchor.y;
            int minZ = anchor.z;
            int maxX = (int)Math.Min(blockbox.sizeX - 1, anchor.x + buildingSizeX - 1);
            int maxY = (int)Math.Min(blockbox.sizeY - 1, anchor.y + buildingSizeY - 1);
            int maxZ = (int)Math.Min(blockbox.sizeZ - 1, anchor.z + buildingSizeZ - 1);
            if (randomDirection) {
                if (Random.value > 0.5) {
                    minX = 2 * anchor.x - maxX;
                    maxX = anchor.x;
                }
                if (Random.value > 0.5) {
                    minY = 2 * anchor.y - maxY;
                    maxY = anchor.y;
                }
                if (Random.value > 0.5) {
                    minZ = 2 * anchor.z - maxZ;
                    maxZ = anchor.z;
                }
            }

            int addedVolume = 0;
            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    for (int z = minZ; z < maxZ; z++) {
                        Position3 pos = new Position3(x, y, z);
                        if (blockbox.IsInsideBox(pos) && blockbox.BlockAt(pos) == Block.NULL) {
                            recordedVolume += 1;
                            addedVolume += 1;
                            buildingBlocks.Add(pos);
                        }
                        blockbox.TrySetBlock(Block.Building, pos);
                    }
                }
            }

            return addedVolume;
        }
        
        protected override Block GenerateBlock(Position3 position, bool doForce) {
            /*
            Block prevBlock = blockbox.BlockAt(position);
            if (prevBlock == Block.Building) {
                return prevBlock;
            }
            
            Dictionary<Position3, Block> neighbors = blockbox.GetNeighbors(position);
            Block block = BlockSelection.PickBlock(neighbors, position);
            blockbox.TryAddBlock(block, position);
            if (block == Block.NULL) {
                throw new Exception("Fatal ERROR: a NULL block was generated");
            }

            if (block == Block.Building) {
                return Block.Void;
            }

            return block;
            */
            Block block = blockbox.BlockAt(position);
            return block == Block.NULL ? Block.Void: block;
        }

        public override bool IsDone() => isDone;

        public override Position3 GetPreviousPosition() => previousPosition;

        public override Position3 GetNextPosition() {
            int x = previousPosition.x;
            int y = previousPosition.y;
            int z = previousPosition.z;

            ++x;
            if (x >= blockbox.sizeX) {
                x = 0;
                ++z;
            }

            if (z >= blockbox.sizeZ) {
                z = 0;
                ++y;
            }

            if (y >= blockbox.sizeY) {
                y = 0;
                isDone = true;
            }

            return new Position3(x, y, z);
        }

        protected override Position3 SetPreviousPosition(Position3 position) {
            previousPosition = position;
            return position;
        }

        private class Cuboid
        {
            private Position3 corner1;
            private Position3 corner2;
            
            public const int MinCuboidHeight = 6;
            public const int MaxCuboidHeight = 30;
            public const int MinCuboidWidthX = 6;
            public const int MaxCuboidWidthX = 20;
            public const int MinCuboidWidthZ = 6;
            public const int MaxCuboidWidthZ = 15;
            public const int MinCuboidVolume = 20;
            public const int MaxCuboidVolume = 2000;

            public Cuboid(Position3 corner1, Position3 corner2) {
                this.corner1 = corner1;
                this.corner2 = corner2;
            }

            public Cuboid(Position3 startPos, int sizeX, int sizeY, int sizeZ) :
                this(startPos, startPos + new Position3(sizeX, sizeY, sizeZ)) {
                
            }

            public Position3 RandomInside() {
                int x = Random.Range(corner1.x, corner2.x + 1);
                int y = Random.Range(corner1.y, corner2.y + 1);
                int z = Random.Range(corner1.z, corner2.z + 1);
                return new Position3(x, y, z);
            }
        }
    }
}