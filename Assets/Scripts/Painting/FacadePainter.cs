using System;
using System.Collections.Generic;
using System.Linq;
using Prepping;
using Random = UnityEngine.Random;

namespace Painting
{
    public class FacadePainter
    {
        private Surface _surface;
        private Blockbox _blockBox;
        private Dictionary<Position3, Slot> _currentOutput;
        private bool _isDone;

        public FacadePainter(Surface surface, Blockbox blockbox) {
            if (!surface.IsFacade())
                throw new ArgumentException("Surface must be a facade, i.e Y must not be its constant axis!");
            _surface = surface;
            _isDone = false;
            _currentOutput = new Dictionary<Position3, Slot>();
            _blockBox = blockbox;
            
            // TODO: Adapt this
            DrawWindows();
            DrawDoors();

        }

        private void DrawWindows() {
            for (int y = _surface.GetMinCorner3().y + 1; y < _surface.GetMaxCorner3().y; y += 2) {
                int min;
                int max;
                if (_surface.GetConstantAxis() == ConstantAxis.X) {
                    min = _surface.GetMinCorner3().z;
                    max = _surface.GetMaxCorner3().z;
                } else {
                    min = _surface.GetMinCorner3().x;
                    max = _surface.GetMaxCorner3().x;
                }

                Position3 pos;
                Slot prev = (Random.value > 0.5) ? Slot.Window: Slot.Wall;
                if (_surface.GetConstantAxis() == ConstantAxis.X) {
                    _currentOutput.Add(new Position3(_surface.GetFixedCoordinate(), y, min + 1), prev);
                } else {
                    _currentOutput.Add(new Position3(min + 1, y, _surface.GetFixedCoordinate()), prev);
                }
                for (int i = min + 2; i < max; i++) {
                    pos = _surface.GetConstantAxis() == ConstantAxis.X ? new Position3(_surface.GetFixedCoordinate(), y, i): new Position3(i, y, _surface.GetFixedCoordinate());
                    Slot current;
                    if (prev == Slot.Wall) {
                        current = (Random.value < 0.3) ? Slot.Wall: Slot.Window;
                    } else {
                        current = (Random.value < 0.7) ? Slot.Window : Slot.Wall;
                    }

                    if (!_surface.GetBorders(_blockBox).ContainsKey(pos)) {
                        _currentOutput.Add(pos, current);

                    }
                }
            }
        }

        private void DrawDoors() {
            var borders = _surface.GetBorders(_blockBox);
            List<Position3> groundPos = new List<Position3>();
            foreach (var (position, borderType) in borders) {
                if (borderType == BorderType.Ground) groundPos.Add(position);
            }

            if (Random.value < 0.3 && groundPos.Count > 2) {
                Position3 doorPos = groundPos[(int)Math.Round((groundPos.Count - 2) * Random.value) + 1];
                _currentOutput[doorPos] = Slot.Door;
                _currentOutput[doorPos + Position3.up] = Slot.Door;
            }
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
                            block = Block.Skybridge;
                            break;
                        case Slot.Door:
                            block = Block.Train;
                            break;
                    }

                    blockbox.ForceSetBlock(block, position);
                }
            }
        }


        public enum Slot
        {
            Window, Wall, Door
        }
    }
}