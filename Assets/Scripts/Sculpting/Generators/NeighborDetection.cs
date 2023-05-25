using System;
using System.Collections.Generic;
using Data;

namespace Prepping.Generators
{
    
    // TODO: Find official name. Look at WaveFUnctionCollapse github or MarkovJunior
    public class NeighborDetection : IGenerator
    {
        private Position3 previousPosition;

        private bool isDone = false;

        public NeighborDetection(Blockbox blockbox) : 
            base(blockbox) {
            this.previousPosition = Position3.zero;
            previousPosition = new Position3(-1, 0, 0);
        }

        public override bool IsDone() => isDone;

        public override Position3 GetPreviousPosition() => previousPosition;

        public override Position3 GetNextPosition() {
            int x = previousPosition.x;
            int y = previousPosition.y;
            int z = previousPosition.z;

            ++x;
            if (x >= blockbox._sizeX) {
                x = 0;
                ++z;
            }

            if (z >= blockbox._sizeZ) {
                z = 0;
                ++y;
            }

            if (y >= blockbox._sizeY) {
                y = 0;
                isDone = true;
            }

            return new Position3(x, y, z);
        }

        protected override Position3 SetPreviousPosition(Position3 position) {
            previousPosition = position;
            return position;
        }

        protected override Block GenerateBlock(Position3 position, bool doForce) {
            Block prevBlock = blockbox.BlockAt(position);
            //if (prevBlock != Block.NULL && !doForce) {
                //return prevBlock;
            //}
            
            Dictionary<Position3, Block> neighbors = blockbox.GetRelativeNeighbors(position);
            Block block = BlockSelection.PickBlock(neighbors, position);
            blockbox.TrySetBlock(block, position);
            //if (block == Block.NULL) {
                //throw new Exception("Fatal ERROR: a NULL block was generated");
            //}

            return block;
        }
    }
}