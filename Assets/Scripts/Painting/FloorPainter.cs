using System;
using System.Collections.Generic;
using System.Linq;
using Prepping;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Painting
{
    public class FloorPainter
    {
        private Surface _surface;
        private Dictionary<Vector3, Light> _lights;
        
        public FloorPainter(Surface surface) {
            if (!surface.IsFloor()) throw new ArgumentException("Surface must be a floor!");

            _lights = new Dictionary<Vector3, Light>();
            if (100 * Random.value < 20) {
                List<Position3> blocks = surface.GetBlocks().ToList();
                Position3 pos = blocks[Random.Range(0, blocks.Count)];
                Light light = new Light(Light.color1, 15);
                _lights.Add(pos.AsVector3() + new Vector3(0.2f, 0.5f, 0.3f), light);
            }
        }

        public Dictionary<Vector3, Light> GetLights() => _lights;
    }

    public class Light
    {
        public static readonly Color color1 = new Color(1, 6f / 255, 233f / 255);
        private Color _color;
        private int _radius;

        public Light(Color color, int radius) {
            _color = color;
            _radius = radius;
        }

        public Color GetColor() => _color;

        public int GetRadius() => _radius;
    }
}