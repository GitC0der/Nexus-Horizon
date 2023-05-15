using System;

namespace Painting
{
    public class FloorPainter
    {
        private Surface _surface;
        
        public FloorPainter(Surface surface) {
            if (!surface.IsFloor()) throw new ArgumentException("Surface must be a floor!");
            
        }
    }
}