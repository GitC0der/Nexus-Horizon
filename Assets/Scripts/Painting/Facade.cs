using System;
using System.Collections.Generic;
using System.Linq;
using Prepping;
using Random = UnityEngine.Random;

namespace Painting
{
    public class Facade
    {
        // TODO: Use Position instead of Position3
        private HashSet<Position3> _blocks;
        private Orientation _orientation;
        private ConstantAxis _constantAxis;
        private Position3 _normal;
        private int _height;
        private int _width;
        private int _fixedCoordinate;
        private Position2 _minCorner;
        private Position2 _maxCorner;
        private Position3 _minCorner3;
        private Position3 _maxCorner3;

        public Facade(HashSet<Position3> blocks, Position3 normal) {
            _blocks = blocks;
            _normal = normal;
            if (normal == new Position3(0, 1, 0)) {
                _orientation = Orientation.Roof;
                _constantAxis = ConstantAxis.Y;
            } else if (normal == new Position3(0, -1, 0)) {
                _orientation = Orientation.Floor;
                _constantAxis = ConstantAxis.Y;
            } else if (normal == new Position3(1, 0, 0)) {
                _orientation = Orientation.WallE;
                _constantAxis = ConstantAxis.X;
            } else if (normal == new Position3(-1, 0, 0)) {
                _orientation = Orientation.WallW;
                _constantAxis = ConstantAxis.X;
            } else if (normal == new Position3(0, 0, 1)) {
                _orientation = Orientation.WallN;
                _constantAxis = ConstantAxis.Z;
            } else if (normal == new Position3(0, 0, -1)) {
                _orientation = Orientation.WallS;
                _constantAxis = ConstantAxis.Z;
            } else {
                throw new ArgumentException($"normal: {normal} must be one of the following (1,0,0), (-1,0,0), (0,1,0), (0,-1,0), (0,0,1), and (0,0,-1).");
            }

            int xMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMin = int.MaxValue;
            int yMax = int.MinValue;
            int zMin = int.MaxValue;
            int zMax = int.MinValue;
            
            foreach (Position3 pos in blocks) {
                if (pos.x < xMin) xMin = pos.x;
                if (pos.x > xMax) xMax = pos.x;
                if (pos.y < yMin) yMin = pos.y;
                if (pos.y > yMax) yMax = pos.y;
                if (pos.z < zMin) zMin = pos.z;
                if (pos.z > zMax) zMax = pos.z;
            }

            if (_orientation is Orientation.Floor or Orientation.Roof) {
                _width = xMax - xMin + 1;
                _height = zMax - zMin + 1;
                _minCorner = new Position2(xMin, zMin);
                _maxCorner = new Position2(xMax, zMax);
                _fixedCoordinate = blocks.ToList()[0].y;
                _minCorner3 = new Position3(xMin, _fixedCoordinate, zMin);
                _maxCorner3 = new Position3(xMax, _fixedCoordinate, zMax);
            } else if (_orientation is Orientation.WallN or Orientation.WallS) {
                _width = xMax - xMin + 1;
                _height = yMax - yMin + 1;
                _minCorner = new Position2(xMin, yMin);
                _maxCorner = new Position2(xMax, yMax);
                _fixedCoordinate = blocks.ToList()[0].z;
                _minCorner3 = new Position3(xMin, yMin, _fixedCoordinate);
                _maxCorner3 = new Position3(xMax, yMax, _fixedCoordinate);
            } else {
                _width = yMax - yMin + 1;
                _height = zMax - zMin + 1;
                _minCorner = new Position2(yMin, zMin);
                _maxCorner = new Position2(yMax, zMax);
                _fixedCoordinate = blocks.ToList()[0].x;
                _minCorner3 = new Position3(_fixedCoordinate, yMin, zMin);
                _maxCorner3 = new Position3(_fixedCoordinate, yMax, zMax);
            }
        }
        
        public Position2 GetMinCorner2() => _minCorner;

        public Position2 GetMaxCorner2() => _maxCorner;
        
        public Position3 GetMinCorner3() => _minCorner3;

        public Position3 GetMaxCorner3() => _maxCorner3;
        
        public int GetFixedCoordinate() => _fixedCoordinate;
        public Position2 RandomPos() {
            Position3 block = _blocks.ToList()[Random.Range(0, _blocks.Count)];
            if (_orientation == Orientation.Floor || _orientation == Orientation.Roof) {
                return new Position2(block.x, block.z);
            } else if (_orientation == Orientation.WallN || _orientation == Orientation.WallS) {
                return new Position2(block.x, block.y);
            } else {
                return new Position2(block.y, block.z);
            }

        }

        public HashSet<Position3> GetBlocks() => _blocks;

        public int GetHeight() => _height;

        public int GetWidth() => _width;

        public Orientation GetOrientation() => _orientation;

    }
    
    public enum Orientation
    {
        Roof, WallN, WallS, WallE, WallW, Floor
    }

    public enum ConstantAxis
    {
        X, Y, Z
    }
}