using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Prepping;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Painting
{
    public class FloorPainter
    {
        private Surface _surface;
        private Blockbox _blockbox;
        private Dictionary<Vector3, Light> _lights;
        private PropManager _propManager;
        private bool _enableLights;
        private FloorTheme _theme;
        
        public FloorPainter(Surface surface, Blockbox blockbox, PropManager propManager, bool enableLights) {
            if (!surface.IsFloor()) throw new ArgumentException("Surface must be a floor!");

            _surface = surface;
            _lights = new Dictionary<Vector3, Light>();
            _propManager = propManager;
            _blockbox = blockbox;
            _enableLights = enableLights;
            _theme = ChooseTheme();
            
            /*
            if (100 * Random.value < 20) {
                List<Position3> blocks = surface.GetBlocks().ToList();
                Position3 pos = blocks[Random.Range(0, blocks.Count)];
                Light light = new Light(Light.color1, 15);
                _lights.Add(pos.AsVector3() + new Vector3(0.2f, 0.5f, 0.3f), light);
            }
            */

            switch (_theme) {
                case FloorTheme.Dining:
                    PlaceRailing();
                    PlaceLampPosts();
                    PlaceTables();
                    break;
                case FloorTheme.Utilities:
                    PlaceLongAC();
                    break;
                default:
                    break;
            }
            
        }

        private FloorTheme ChooseTheme() {
            bool hasDoors = _blockbox.GetDoorsLeadingTo(_surface.GetBorderPositions()).Count > 0;
            return hasDoors ? FloorTheme.Dining : FloorTheme.Utilities;
        }

        private void PlaceRailing() {
            Border border = _surface.GetBorder(BorderType.None);
            if (border != null) {
                foreach (var (position, list) in border.GetDirections()) {
                    foreach (Vector3 facing in list) {
                        Vector3 displ = 0.4f * facing + 0.3f * Vector3.up;
                        displ += new Vector3(facing.z,0, -facing.x);
                        _propManager.Instantiate(_propManager.Railing(), position, position.AsVector3() + displ, facing, _surface.GetBlocks());
                    }
                }
            }
        }

        private void PlaceLampPosts() {
            Border border = _surface.GetBorder(BorderType.None);
            if (border != null) {
                if (100 * Random.value < 35) {
                    var pos = border.GetPositions().ToList()[Random.Range(0, border.GetPositions().Count)];
                    HashSet<Vector3> listFacing = border.GetDirections()[pos];
                    foreach (Vector3 facing in listFacing) {
                        var gameObject = _propManager.Instantiate(_propManager.Lamp(), pos, pos.AsVector3() + 2f*Vector3.up, facing, _surface.GetBlocks());
                        if (!_enableLights && gameObject != null) gameObject.GetComponentInChildren<UnityEngine.Light>().enabled = false;
                    }
                        
                }
            }
        }

        private void PlaceTables() {
            if (_surface.GetBlocks().Count > 35) {
                int placedCount = 0;
                int iterationsCount = 0;
                do {
                    ++iterationsCount;
                    Position3 position = _surface.GetBlocks().ToList()[Random.Range(0, _surface.GetBlocks().Count)];
                    if (!_surface.IsInBorders(position)) {
                        Vector3 displ = 0.5f * _surface.GetWidthDirection().AsVector3() + 0.5f * _surface.GetHeightDirection().AsVector3() + 0.5f*Vector3.up;
                        var gameObject = _propManager.Instantiate(_propManager.TableSet(), position, position.AsVector3() + displ,
                            Vector3.back, _surface.GetBlocks());
                        if (gameObject != null) ++placedCount;
                    }

                } while (placedCount < 3 && iterationsCount < 20);
            }
        }

        private void PlaceLongAC() {
            if (_surface.GetBlocks().Count > 20) {
                int iterationsCount = 0;
                do {
                    ++iterationsCount;
                    Position3 position = _surface.GetBlocks().ToList()[Random.Range(0, _surface.GetBlocks().Count)];
                    if (!_surface.IsInBorders(position)) {
                        Vector3 displ = Vector3.up;
                        var gameObject = _propManager.Instantiate(_propManager.LongAirConditioning(), position,
                            position.AsVector3() + displ, RandomFloorOrientation(), _surface.GetBlocks());
                        if (gameObject != null) {
                            break;
                        }
                    }
                } while (iterationsCount < 20);
            }
        }

        private Vector3 RandomFloorOrientation() {
            float rnd = Random.value;
            switch (rnd) {
                case var _ when rnd < 0.25:
                    return Vector3.left;
                case var _ when rnd < 0.5:
                    return Vector3.right;
                case var _ when rnd < 0.75:
                    return Vector3.forward;
            }

            return Vector3.back;

        }
        
        public Dictionary<Vector3, Light> GetLights() => _lights;
    }

    public enum FloorTheme
    {
        Utilities, Dining
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