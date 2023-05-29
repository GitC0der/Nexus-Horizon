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
        
        public FloorPainter(Surface surface, Blockbox blockbox, bool enableLights) {
            if (!surface.IsFloor()) throw new ArgumentException("Surface must be a floor!");

            _surface = surface;
            _lights = new Dictionary<Vector3, Light>();
            _propManager = SL.Get<PropManager>();
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
                    PlaceCouch1();
                    PlacePlants();
                    PlaceWallLamps();
                    break;
                case FloorTheme.Utilities:
                    PlaceWaterTowers();
                    PlaceCoolingUnit();
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

        private void PlaceWallLamps() {
            Dictionary<Position3, Position3> facings = new();
            foreach (Position3 pos in _surface.GetBorderPositions()) {
                var neighbors = _blockbox.GetRelativeNeighbors(pos + 2 * Position3.up);
                foreach (var (relativePos, block) in neighbors) {
                    if (block == Block.Building) {
                        var shift = _blockbox.ShiftAt(pos);
                        if (shift.Exist()) {
                            Position3 facing = relativePos;
                            if (!facings.ContainsKey(pos)) facings.Add(pos + shift.Get().AsPosition3(), facing);
                        }
                    }
                }
            }
            
            int target = (int)Math.Round(facings.Count / 4.0);
            if (target == 0) target = 1;
            int total = 0;
            for (int i = 0; i < target; i++) {
                if (100 * Random.value < 50) ++total;
            }

            if (total == 0) return;
            int placedCount = 0;
            int iterationsCount = 0;
            do {
                ++iterationsCount;
                Position3 position = facings.Keys.ToArray()[Random.Range(0, facings.Count)];
                Vector3 facing = facings[position].AsVector3();
                PropPrefab prefab = _propManager.WallLamp();
                Vector3 newPos = ActualPos(position.AsVector3(), prefab.Offset(), facing);
                var shift = _blockbox.ShiftAt(newPos.AsPosition3());
                if (shift.Exist()) {
                    var gameObject = _propManager.Instantiate(prefab, position, newPos + shift.Get(), facing, _surface.GetBlocks());
                    if (gameObject != null) {
                        if (!_enableLights) gameObject.GetComponentInChildren<UnityEngine.Light>().enabled = false;
                        ++placedCount;
                    }
                }
            } while (placedCount < total && iterationsCount < 20);

        }
        
        private void PlacePlants() {
            switch (_surface.GetBlocks().Count) {
                case < 30:
                    Place(_propManager.Plant(), 1, 10);
                    break;
                case < 75:
                    Place(_propManager.Plant(), 2, 10);
                    break;
                default:
                    Place(_propManager.Plant(), 3, 10);
                    break;
            }
        }
        
        private void PlaceCouch1() {
            switch (_surface.GetBlocks().Count) {
                case < 40:
                    Place(_propManager.Couch1(), 1, 5);
                    break;
                case < 80:
                    Place(_propManager.Couch1(), 2, 10);
                    break;
                default:
                    Place(_propManager.Couch1(), 3, 15);
                    break;
            }
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

        private void PlaceWaterTowers() {
            int towersCount = 0;
            switch (_surface.GetBlocks().Count) {
                case < 40:
                    towersCount = 1;
                    break;
                case < 90:
                    towersCount = 2;
                    break;
                default:
                    towersCount = 3;
                    break;
            }

            for (int i = 0; i < towersCount; i++) {
                if (100 * Random.value < 30) Place(_propManager.WaterTower(), 1, 15);
            }
        }

        private void PlaceLampPosts() {
            int lampCount = _surface.GetBorderPositions().Count switch {
                < 20 => 1,
                < 35 => 2,
                < 40 => 3,
                _ => 4
            };

            Border border = _surface.GetBorder(BorderType.None);
            for (int i = 0; i < lampCount; i++) {
                if (border != null) {
                    if (100 * Random.value < 35) {
                        var pos = border.GetPositions().ToList()[Random.Range(0, border.GetPositions().Count)];
                        HashSet<Vector3> listFacing = border.GetDirections()[pos];
                        foreach (Vector3 facing in listFacing) {
                            Vector3 propPos = ActualPos(pos.AsVector3(), _propManager.Lamp().Offset(), facing);
                            var gameObject = _propManager.Instantiate(_propManager.Lamp(), pos, pos.AsVector3() + 2f*Vector3.up, facing, _surface.GetBlocks());
                            if (!_enableLights && gameObject != null) gameObject.GetComponentInChildren<UnityEngine.Light>().enabled = false;
                        }
                        
                    }
                }
            }
            
            
            
        }

        private int Place(PropPrefab prefab, int countTarget, int maxIterations, bool isInBorders = false) {
            int placedCount = 0;
            int iterationsCount = 0;
            do {
                ++iterationsCount;
                Position3 position;
                if (isInBorders) {
                    position = _surface.GetBlocks().ToList()[Random.Range(0, _surface.GetBlocks().Count)];
                } else {
                    position = _surface.GetBorderPositions().ToList()[Random.Range(0, _surface.GetBorderPositions().Count)];
                }
                
                Vector3 facing = RandomFloorOrientation();
                Vector3 newPos = ActualPos(position.AsVector3(), prefab.Offset(), facing);
                var gameObject = _propManager.Instantiate(prefab, position, newPos, facing, _surface.GetBlocks());
                if (gameObject != null) ++placedCount;
                
            } while (placedCount < countTarget && iterationsCount < maxIterations);

            return placedCount;
        }
        
        private void PlaceTables() {
            if (_surface.GetBlocks().Count > 35) {
                int placedCount = 0;
                int iterationsCount = 0;
                do {
                    ++iterationsCount;
                    Position3 position = _surface.GetBlocks().ToList()[Random.Range(0, _surface.GetBlocks().Count)];
                    if (!_surface.IsInBorders(position)) {
                        Vector3 facing = RandomFloorOrientation();
                        Vector3 newPos = ActualPos(position.AsVector3(), _propManager.TableSet().Offset(), facing);
                        //var gameObject = _propManager.Instantiate(_propManager.TableSet(), position, newPos, facing, _surface.GetBlocks());
                        var gameObject = _propManager.Instantiate(_propManager.TableSet(), position, newPos, facing,
                            _surface.GetBlocks());
                        if (gameObject != null) ++placedCount;
                    }

                } while (placedCount < 3 && iterationsCount < 20);
            }
        }

        private void PlaceCoolingUnit() {
            switch (_surface.GetBlocks().Count) {
                case < 20:
                    break;
                case < 35:
                    Place(_propManager.LargeCoolingUnit(), 1, 10);
                    break;
                case < 60:
                    Place(_propManager.LargeCoolingUnit(), 2, 20);
                    break;
                case < 100:
                    Place(_propManager.LargeCoolingUnit(), 4, 20);
                    break;
                default:
                    Place(_propManager.LargeCoolingUnit(), 4, 20);
                    break;
            }
        }

        private void PlaceLongAC() {
            switch (_surface.GetBlocks().Count) {
                case < 20:
                    break;
                case < 40:
                    Place(_propManager.LongAirConditioning(), 1, 20);
                    break;
                case < 70:
                    Place(_propManager.LongAirConditioning(), 2, 20);
                    break;
                default:
                    Place(_propManager.LongAirConditioning(), 3, 20);
                    break;
            }
            /*
            if (_surface.GetBlocks().Count > 20) {
                int iterationsCount = 0;
                do {
                    ++iterationsCount;
                    Position3 position = _surface.GetBlocks().ToList()[Random.Range(0, _surface.GetBlocks().Count)];
                    if (!_surface.IsInBorders(position)) {
                        Vector3 facing = RandomFloorOrientation();
                        Vector3 propPos = ActualPos(position.AsVector3(), _propManager.LongAirConditioning().Offset(), facing);
                        //Vector3 displ = 0.33f * Vector3.up - 0.65f*facing - 0.53f*facing.RotatedLeft();
                        //var gameObject = _propManager.Instantiate(_propManager.LongAirConditioning(), position, position.AsVector3() + displ, facing, _surface.GetBlocks());
                        var gameObject = _propManager.Instantiate(_propManager.LongAirConditioning(), position, propPos,
                            facing, _surface.GetBlocks());
                        if (gameObject != null) {
                            break;
                        }
                    }
                } while (iterationsCount < 20);
            }
            */
        }

        private Vector3 ActualPos(Vector3 pos, Vector3 offset, Vector3 facing) {
            return pos + offset.x * facing.RotatedLeft() + offset.y * Vector3.up + offset.z * facing;
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