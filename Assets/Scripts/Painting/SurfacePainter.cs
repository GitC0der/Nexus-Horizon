using System.Collections.Generic;
using Prepping;
using UnityEngine;

namespace Painting
{
    public class SurfacePainter
    {
        private Surface _surface;
        private Dictionary<Position3, Slot> _currentOutput;
        private bool _isDone;

        public SurfacePainter(Surface surface) {
            _surface = surface;
            _isDone = false;
            _currentOutput = new Dictionary<Position3, Slot>();
            
            // TODO: Adapt this
            for (int y = surface.GetMinCorner3().y + 1; y < surface.GetMaxCorner3().y; y += 2) {
                int min;
                int max;
                if (surface.GetConstantAxis() == ConstantAxis.X) {
                    min = surface.GetMinCorner3().z;
                    max = surface.GetMaxCorner3().z;
                } else {
                    min = surface.GetMinCorner3().x;
                    max = surface.GetMaxCorner3().x;
                }

                Position3 pos;
                Slot prev = (Random.value > 0.5) ? Slot.Window: Slot.Wall;
                if (surface.GetConstantAxis() == ConstantAxis.X) {
                    _currentOutput.Add(new Position3(surface.GetFixedCoordinate(), y, min + 1), prev);
                } else {
                    _currentOutput.Add(new Position3(min + 1, y, surface.GetFixedCoordinate()), prev);
                }
                for (int i = min + 2; i < max; i++) {
                    pos = surface.GetConstantAxis() == ConstantAxis.X ? new Position3(surface.GetFixedCoordinate(), y, i): new Position3(i, y, surface.GetFixedCoordinate());
                    Slot current;
                    if (prev == Slot.Wall) {
                        current = (Random.value < 0.3) ? Slot.Wall: Slot.Window;
                    } else {
                        current = (Random.value < 0.7) ? Slot.Window : Slot.Wall;
                    }
                    _currentOutput.Add(pos, current);
                }
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
                    }

                    blockbox.ForceSetBlock(block, position);
                }
            }
        }


        public enum Slot
        {
            Window, Wall,
        }
    }
}