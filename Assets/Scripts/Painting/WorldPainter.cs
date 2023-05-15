using System;
using System.Collections.Generic;
using static Painting.WaveFunctionCollapse;

namespace Painting
{

    public class WorldPainter
    {
        private HashSet<Tile> _wfcInputTiles;
        private HashSet<char> _wfcInputChars;
        private HashSet<Surface> _facades;

        public WorldPainter(HashSet<Surface> facades, Tile inputTile) {
            _wfcInputTiles = inputTile.GetAllSubtiles();
            _wfcInputChars = inputTile.GetChars();
        }
    }
}