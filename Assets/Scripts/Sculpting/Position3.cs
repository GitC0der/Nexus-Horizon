using System;
using UnityEngine;

namespace Prepping
{
    public struct Position3
    {
        public static readonly Position3 zero = new Position3(0, 0, 0);
        public static readonly Position3 one = new Position3(1, 1, 1);
        public static readonly Position3 up = new Position3(0, 1, 0);
        public static readonly Position3 down = new Position3(0, -1, 0);
        public static readonly Position3 left = new Position3(-1, 0, 0);
        public static readonly Position3 right = new Position3(1, 0, 0);
        public static readonly Position3 forward = new Position3(0, 0, 1);
        public static readonly Position3 back = new Position3(0, 0, -1);
        
        public readonly int x;
        public readonly int y;
        public readonly int z;
        
        public static Position3 operator +(Position3 a, Position3 b) => new (a.x + b.x, a.y + b.y, a.z + b.z);

        public static Position3 operator -(Position3 a, Position3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Position3 operator -(Position3 a) => new(-a.x, -a.y, -a.z);
        
        public static Position3 operator *(Position3 a, float d) => new ((int)Math.Round(a.x * d), (int)Math.Round(a.y * d), (int)Math.Round(a.z * d));

        public static Position3 operator *(float d, Position3 a) => new ((int)Math.Round(a.x * d), (int)Math.Round(a.y * d), (int)Math.Round(a.z * d));

        public static Position3 operator /(Position3 a, float d) => new ((int)Math.Round(a.x / d), (int)Math.Round(a.y / d), (int)Math.Round(a.z / d));

        public static bool operator ==(Position3 lhs, Position3 rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        
        public static bool operator !=(Position3 lhs, Position3 rhs) => !(lhs == rhs);

        
        public static Vector3 AsVector3(Position3 from) => new(from.x, from.y, from.z);

        public Position3(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Position3(Vector3 from) {
            this.x = (int)Math.Round(from.x);
            this.y = (int)Math.Round(from.y);
            this.z = (int)Math.Round(from.z);
        }

        public Vector3 AsVector3() => new(x, y, z);

        public Vector3 To(Position3 position) => AsVector3(position - this);

        public override string ToString() => $"({x}, {y}, {z})";
        
        public override bool Equals(object obj) {
            if (obj == null) return false;
            return this == (Position3)obj;
        }

        public override int GetHashCode() {
            return HashCode.Combine(x, y, z);
        }
        
    }
}