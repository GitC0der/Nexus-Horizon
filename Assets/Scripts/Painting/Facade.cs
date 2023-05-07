using System;
using System.Collections.Generic;
using Prepping;

namespace Painting
{
    public class Facade
    {
        // TODO: Use Position instead of Position3
        private HashSet<Position3> _blocks;
        private Orientation _orientation;

        public Facade(HashSet<Position3> blocks, Position3 normal) {
            _blocks = blocks;
            if (normal == new Position3(0, 1, 0)) {
                _orientation = Orientation.Roof;
            } else if (normal == new Position3(0, -1, 0)) {
                _orientation = Orientation.Floor;
            } else if (normal == new Position3(1, 0, 0)) {
                _orientation = Orientation.WallE;
            } else if (normal == new Position3(-1, 0, 0)) {
                _orientation = Orientation.WallW;
            } else if (normal == new Position3(0, 0, 1)) {
                _orientation = Orientation.WallN;
            } else if (normal == new Position3(0, 0, -1)) {
                _orientation = Orientation.WallS;
            } else {
                throw new ArgumentException($"normal: {normal} must be one of the following (1,0,0), (-1,0,0), (0,1,0), (0,-1,0), (0,0,1), and (0,0,-1).");
            }
        }

        public HashSet<Position3> GetBlocks() => _blocks;

    }
    
    public enum Orientation
    {
        Roof, WallN, WallS, WallE, WallW, Floor
    }
}