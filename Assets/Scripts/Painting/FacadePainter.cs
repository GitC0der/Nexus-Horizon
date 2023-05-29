using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Prepping;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Painting
{
    public class FacadePainter
    {
        private const float WINDOW_SHIFT = 0.3f;
        private const float DOOR_SHIFT = 0.4f;
        private Surface _surface;
        private Blockbox _blockBox;
        private Dictionary<Position3, Slot> _currentOutput;
        private Dictionary<Position3, Vector3> _currentShifts;
        private bool _isDone;

        public FacadePainter(Surface surface, Blockbox blockbox) {
            if (!surface.IsFacade())
                throw new ArgumentException("Surface must be a facade, i.e Y must not be its constant axis!");
            _surface = surface;
            _isDone = false;
            _currentOutput = new Dictionary<Position3, Slot>();
            _currentShifts = new Dictionary<Position3, Vector3>();
            _blockBox = blockbox;
            
            
            if (surface.GetBlocks().Count == 0 || !blockbox.IsStrictlyInside(surface.GetBlocks().ToList()[0] - 2 * surface.GetNormal()) 
                                               || !blockbox.IsStrictlyInside(surface.GetBlocks().ToList()[0] + 2 * surface.GetNormal())) {
                return;
            }
            
            // TODO: Adapt this
            if (_surface.GetWidth() > 3) DrawWindows();
            
            
            if (surface.GetHeight() > 2) DrawDoors();

            _isDone = true;

        }

        private void Windows_SmallRectangular() {
            float random = 100*Random.value;
            if (random < 70) {
                for (int y = _surface.GetMinCorner3().y + 1; y < _surface.GetMaxCorner3().y; y++) {
                    Position3 pos = new Position3(_surface.GetMinCorner3().x, y, _surface.GetMinCorner3().z);
                    for (int i = 1; i < _surface.GetWidth() - 1; i++) {
                        pos += _surface.GetWidthDirection();
                        _currentOutput.Add(pos, Slot.Window);
                        _currentShifts.Add(pos, -WINDOW_SHIFT * _surface.GetNormal().AsVector3());
                    }
                }
            }
        }

        private void Windows_Lines(bool fullLines, bool onlyWindows, bool businessStyle) {
            int step = businessStyle ? 1 : 2;
            for (int y = _surface.GetMinCorner3().y + 1; y < _surface.GetMaxCorner3().y; y += step) {
                int min;
                int max;
                if (_surface.GetConstantAxis() == ConstantAxis.X) {
                    min = _surface.GetMinCorner3().z;
                    max = _surface.GetMaxCorner3().z;
                }
                else {
                    min = _surface.GetMinCorner3().x;
                    max = _surface.GetMaxCorner3().x;
                }

                Position3 pos;
                Slot prev = (Random.value > 0.5) ? Slot.Window : Slot.Wall;
                for (int i = min + 1; i < max; i++) {
                    pos = _surface.GetConstantAxis() == ConstantAxis.X
                        ? new Position3(_surface.GetFixedCoordinate(), y, i)
                        : new Position3(i, y, _surface.GetFixedCoordinate());
                    if (!_surface.IsInBorders(pos) && _surface.Contains(pos)) {
                        Slot current;
                        if (prev == Slot.Wall) {
                            current = (Random.value < 0.3) ? Slot.Wall : Slot.Window;
                        }
                        else {
                            current = (Random.value < 0.7) ? Slot.Window : Slot.Wall;
                        }

                        current = onlyWindows ? Slot.Window : current;
                        if (!_surface.IsInBorders(pos)) {
                            _currentOutput.Add(pos, current);
                            if (fullLines) {
                                _currentShifts.Add(pos, -WINDOW_SHIFT * _surface.GetNormal().AsVector3());
                            } else {
                                if (current == Slot.Window) {
                                    _currentShifts.Add(pos, -WINDOW_SHIFT * _surface.GetNormal().AsVector3());
                                } else {
                                    _currentShifts.Add(pos, Vector3.zero);
                                }
                            }
                            
                        }
                        
                        prev = current;
                    }
                }
            }
        }
        
        private void DrawWindows() {
            if (_surface.GetBlocks().Count < 40 && _surface.GetBlocks().Count == _surface.GetWidth() * _surface.GetHeight()) {
                Windows_SmallRectangular();
            } else if (_surface.GetBlocks().Count == _surface.GetWidth() * _surface.GetHeight()) {
                float rng = 100 * Random.value;
                switch (rng) {
                    case < 40:
                        Windows_Lines(true, true, true);
                        break;
                    case < 80:
                        Windows_Lines(true, true, false);
                        break;
                    case < 90:
                        Windows_Lines(false, false, false);
                        break;
                    default:
                        break;
                }

            } else {
                switch (100*Random.value) {
                    case < 30:
                        Windows_Lines(true, false, false);
                        break;
                    case < 80:
                        Windows_Lines(false, false, false);
                        break;
                    case < 90:
                        Windows_Lines(true, true, false);
                        break;
                    default:
                        break;
                }
                

            }
        }

        private void DrawDoors() {
            /*
            Optional<Border> groundBorder = _surface.GetBorder(BorderType.Ground);
            HashSet<Position3> groundPos = (groundBorder.Exist()) 
                ? groundBorder.Get().GetPositions().ToHashSet()
                : new HashSet<Position3>();
            */

            Border groundBorder = _surface.GetBorder(BorderType.Ground);
            HashSet<Position3> groundPos = groundBorder?.GetPositions().ToHashSet() ?? new HashSet<Position3>();

            // TODO: UNDO if failure
            /*
            foreach (var (position, borderType) in borders) {
                if (borderType == BorderType.Ground) groundPos.Add(position);
            }
            */

            if (groundPos.Count > 2) {
                Position3 doorPos = groundPos.ToList()[(int)Math.Round((groundPos.Count - 2) * Random.value) + 1];
                if (IsRoomForDoor(doorPos) && Random.value < 0.8) {
                    _currentOutput[doorPos] = Slot.Door;
                    _currentOutput[doorPos + Position3.up] = Slot.Door;
                    _currentShifts[doorPos] = -DOOR_SHIFT * _surface.GetNormal().AsVector3();
                    _currentShifts[doorPos + Position3.up] = -DOOR_SHIFT * _surface.GetNormal().AsVector3();
                    _currentOutput[doorPos + Position3.down] = Slot.Wall;
                    _currentShifts[doorPos + Position3.down] = Vector3.zero;
                    _blockBox.SetDoor(new[] { doorPos, doorPos + Position3.up });
                }
            }
        }

        private bool IsRoomForDoor(Position3 pos) {
            for (int i = 0; i < 3; i++) {
                if (_blockBox.BlockAt(pos + (i-1) * _surface.GetWidthDirection() + _surface.GetNormal() + Position3.down) != Block.Building) return false;
                if (_blockBox.BlockAt(pos + (i-1) * _surface.GetWidthDirection() + 2*_surface.GetNormal() + Position3.down) != Block.Building) return false;
                for (int y = 0; y < 3; y++) {
                    Position3 newPos = pos + (i-1) * _surface.GetWidthDirection() + y*Position3.up;
                    if (!_blockBox.IsStrictlyInside(newPos) || !_blockBox.IsStrictlyInside(newPos + 2*_surface.GetNormal()) || _blockBox.BlockAt(newPos) == Block.Void) return false;
                    if (_blockBox.BlockAt(newPos + _surface.GetNormal()) != Block.Void) return false;
                    if (_blockBox.BlockAt(newPos + 2*_surface.GetNormal()) != Block.Void) return false;
                }
            }

            

            //if (!_blockBox.IsStrictlyInside(pos + 2 * _surface.GetNormal()) || _blockBox.BlockAt(pos + 2 * _surface.GetNormal()) == Block.Void) return false;

            return true;
        }
        
        public bool IsDone() => _isDone;
        
        

        public void AddToBlockbox(Blockbox blockbox) {
            foreach (var (position, slot) in _currentOutput) {
                if (_surface.Contains(position)) {
                    Block block = Block.Void;
                    switch (slot) {
                        case Slot.Wall:
                            block = Block.Building;
                            break;
                        case Slot.Window:
                            block = Block.Window;
                            break;
                        case Slot.Door:
                            block = Block.Door;
                            break;
                    }

                    blockbox.ForceSetBlock(block, position, _currentShifts[position]);
                }
            }
        }

        public Dictionary<Position3, Vector3> GetShifts() => _currentShifts;
        public Dictionary<Position3, Block> GetBlocks() {
            Dictionary<Position3, Block> blocks = new();
            foreach (var (pos, slot) in _currentOutput) {
                Block block;
                switch (slot) {
                    case Slot.Window:
                        block = Block.Window;
                        break;
                    case Slot.Wall:
                        block = Block.Building;
                        break;
                    case Slot.Door:
                        block = Block.Door;
                        break;
                    default:
                        block = Block.Building;
                        break;
                }
                blocks.Add(pos, block);
            }

            return blocks;
        }

        public enum Slot
        {
            Window, Wall, Door
        }

        public enum FacadeTheme
        {
            Business, Residential, Slum
        }
    }
}