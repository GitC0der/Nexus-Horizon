using System;
using UnityEngine;

namespace Painting
{
    public struct Position2
    {
        public static readonly Position2 zero = new Position2(0, 0);
        public static readonly Position2 one = new Position2(1, 1);
        public static readonly Position2 up = new Position2(0, 1);
        public static readonly Position2 down = new Position2(0, -1);
        public static readonly Position2 left = new Position2(-1, 0);
        public static readonly Position2 right = new Position2(1, 0);

        public readonly int x;
        public readonly int y;
        
        public static Position2 operator +(Position2 a, Position2 b) => new (a.x + b.x, a.y + b.y);

        public static Position2 operator -(Position2 a, Position2 b) => new(a.x - b.x, a.y - b.y);

        public static Position2 operator -(Position2 a) => new(-a.x, -a.y);

        public static bool operator ==(Position2 lhs, Position2 rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
        
        public static bool operator !=(Position2 lhs, Position2 rhs) => lhs.x != rhs.x && lhs.y != rhs.y;

        
        public static Vector2 AsVector2(Position2 from) => new(from.x, from.y);

        public Position2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Position2(Vector2 from) {
            this.x = (int)Math.Round(from.x);
            this.y = (int)Math.Round(from.y);
        }

        public Vector2 AsVector2() => new(x, y);

        public Vector2 To(Position2 position) => AsVector2(position - this);

        public override string ToString() => $"({x}, {y})";

        public override bool Equals(object obj) {
            if (obj == null) return false;
            return this == (Position2)obj;
        }

        public override int GetHashCode() {
            return HashCode.Combine(x, y);
        }
    }
}