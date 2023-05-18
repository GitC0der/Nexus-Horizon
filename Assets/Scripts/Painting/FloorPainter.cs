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
        private Blockbox _blockbox;
        private Dictionary<Vector3, Light> _lights;
        private PropManager _propManager;
        private bool _enableLights;
        
        public FloorPainter(Surface surface, Blockbox blockbox, PropManager propManager, bool enableLights) {
            if (!surface.IsFloor()) throw new ArgumentException("Surface must be a floor!");

            _surface = surface;
            _lights = new Dictionary<Vector3, Light>();
            _propManager = propManager;
            _blockbox = blockbox;
            _enableLights = enableLights;
            
            /*
            if (100 * Random.value < 20) {
                List<Position3> blocks = surface.GetBlocks().ToList();
                Position3 pos = blocks[Random.Range(0, blocks.Count)];
                Light light = new Light(Light.color1, 15);
                _lights.Add(pos.AsVector3() + new Vector3(0.2f, 0.5f, 0.3f), light);
            }
            */

            PlaceRailing();
            PlaceLampPosts();
        }

        private void PlaceRailing() {
            Border border = _surface.GetBorder(BorderType.None);
            if (border != null) {
                foreach (var (position, facing) in border.GetDirections()) {
                    Vector3 displ = 0.4f * facing + 0.3f * Vector3.up;
                    displ = displ + new Vector3(facing.y,0, -facing.x);
                    _propManager.Instantiate(_propManager.Railing(), position.AsVector3() + displ, facing);
                }
            }
        }

        private void PlaceLampPosts() {
            Border border = _surface.GetBorder(BorderType.None);
            if (border != null) {
                if (100 * Random.value < 20) {
                    var pos = border.GetPositions().ToList()[Random.Range(0, border.GetPositions().Count)];
                    Vector3 facing = border.GetDirections()[pos];
                    var gameObject = _propManager.Instantiate(_propManager.Lamp(), pos.AsVector3() + 2f*Vector3.up, facing);
                    if (!_enableLights) gameObject.GetComponentInChildren<UnityEngine.Light>().enabled = false;
                }
            }
        }
        
        public Dictionary<Vector3, Light> GetLights() => _lights;
    }

    public enum FloorTheme
    {
        Utilities, 
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