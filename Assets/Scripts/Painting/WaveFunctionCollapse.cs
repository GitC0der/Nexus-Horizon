﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultNamespace;
//using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;



namespace Painting
{
    
    
    // TODO 1) Turn all string[] into List<char[]>
    // TODO 2) Complete no arg constructor in TileMap
    // TODO 3) Create SetChar in TileMap
    // TODO 4) Complete the WaveFunctionCollapse

    public class WaveFunctionCollapse
    {
        public const char EMPTY_CHAR = '?';
        public const char ERROR_CHAR = '@';
        public const int DIMENSION = 4;

        private const int ENTROPY_COMPLETE = int.MaxValue;
        private const bool IS_BORDERLESS = false;
        private Tile input;
        private List<Tile> tiles;
        private List<Tile> tiles_distinct;
        private bool isDone;
        private PriorityQueue<Position2> queue;
        private List<char> inputChars;

        private Tile output;
        private Dictionary<Position2, List<Tile>> possibilities;
        private Dictionary<Position2, int> entropies;

        public static readonly Tile ExampleTile = new Tile(new string[] {
            "---o----X-",
            "XXXoXXXXX-",
            "---o------",
            "---ooooo--",
            "-------o--",
            "--XXXXXoXX",
            "--X----o--",
            "oooooooo--",
            "--X-------",
            "--X-------"
        });
        
        public static readonly Tile Example2Tile = new Tile(new string[] {
            "-XOX------",
            "-XOX------",
            "-XOX------",
            "-XOX------",
            "-XOXXXXX--",
            "-XOOOOOX--",
            "-XXXXXOX--",
            "-----XOX--",
            "-----XOX--",
            "-----XOX--"
        });
        
        public static readonly Tile HugeExample = new Tile(new string[] {
            "--X------X----------",
            "--X------XXXXXXX----",
            "--X------------X----",
            "--XXXXXX-------XXX--",
            "-------X---------X--",
            "ooooooooooo------X--",
            "-------X--o----ooooo",
            "-------X--o----o-X--",
            "XXXXXXXX--o----o-XXX",
            "----------o----o----",
            "XXX-------oooooooooo",
            "--X-----------------",
            "oooooo--------------",
            "--X--o-ooooooooooooo",
            "--X--ooo------------",
            "--X-----------------",
            "--X-------oooooo----",
            "--XXXXX---o----o----",
            "------X---o----ooooo",
            "------X---o---------"
        });

        /*
        public WaveFunctionCollapse(Tile inputMap, int dimension) {
            if (inputMap.Height() < dimension || inputMap.Width() < dimension) {
                throw new ArgumentException($"Dimension must be bigger than both length and width of input map!");
            }
        */
        public WaveFunctionCollapse(Tile inputMap, int outputWidth, int outputHeight) {
            tiles = new List<Tile>();
            for (int row = 0; row <= inputMap.Height() - DIMENSION; row++) {
                for (int col = 0; col <= inputMap.Width() - DIMENSION; col++) {
                    tiles.Add(inputMap.GetSubTile(new Position2(col, row), new Position2(col + DIMENSION - 1, row + DIMENSION - 1), false));
                }
            }
            
            Debug.Log(tiles[0]);
            Debug.Log(tiles[5]);

            this.input = inputMap;
            inputChars = inputMap.GetChars();
            
            // TODO: Fix tiles_distinct
            tiles_distinct = tiles.Distinct().ToList();
            isDone = false;
            
            // TODO: Allow for specified width and height
            output = new Tile(inputMap.Height(), inputMap.Width());
            //output = new Tile(outputHeight, outputWidth);

            possibilities = new Dictionary<Position2, List<Tile>>();
            queue = new PriorityQueue<Position2>();
            entropies = new Dictionary<Position2, int>();
            for (int x = 0; x < inputMap.Width(); x++) {
                for (int y = 0; y < inputMap.Height(); y++) {
                    Position2 position = new Position2(x, y);
                    if ( x < inputMap.Width() - DIMENSION + 1 && y < inputMap.Height() - DIMENSION + 1) {
                        possibilities.Add(position, new List<Tile>(tiles));
                    }
                    queue.Enqueue(position, inputChars.Count);
                    entropies.Add(position, inputChars.Count);
                }
            }
            

            /*
            int y = Random.Range(0, inputMap.Length());
            int x = Random.Range(0, inputMap.Width());
            */
            //TODO: Fix this
            SetCharAndUpdateAll(new Position2(3, 3), 'X');
            //output.SetChar(new Position2(3,3), 'X');

        }

        public string OutputToString() {
            if (!IsDone()) Debug.Log("WARNING: WFC generator is not done! Result will be incomplete");
            return $"{output}";
        }

        public void GenerateNextSlot() {
            /*
                - Choose the position with the lowest entropy
                - For each tile that contains this position
                    - Create a list
                    - For each of its possibilities
                        - Add the character at this position to the list
                - Create a list that contains all characters that appear in each and every of the previously mentioned list
                - Pick a random character inside this list
                
                - For each tile that contains this position
                    - For each of its possible tiles
                        - Remove tile if the character at this position is not the same as the randomly chosen one
                    - Set the entropy to the number of possible tiles
                    
            */
            if (isDone) throw new ArgumentException("Attempting to generate tile while the generator is done");

            if (entropies[queue.Peek()] == 0) {
                //SetCharAndUpdatePossibilities(queue.Peek(), ERROR_CHAR);
                SetCharAndUpdateAll(queue.Peek(), ERROR_CHAR);
                Debug.Log($"ERROR: No possible character at {queue.Peek()}");
                return;
            }
            
            // TODO: DEBUG. FIX THIS
            Position2 slot = queue.Dequeue();

            /*
            int lowestPriority = int.MaxValue;
            Position2 slot = new Position2(-1, -1);
            foreach (var (pos, priority) in entropies) {
                if (priority < lowestPriority) {
                    lowestPriority = priority;
                    slot = pos;
                }
            }
            */

            List<char> possibleChars = PossibleChars(slot);

            char pickedChar;
            
            // TODO: Debugging. Reverse this
            
            if (possibleChars.Count != 0) {
                int count = possibleChars.Count;
                if (count == 3) {
                    pickedChar = possibleChars[2];
                } else if (count == 2){
                    pickedChar = possibleChars[1];
                }
                else {
                    pickedChar = possibleChars[0];
                }
                
                //pickedChar = possibleChars[Random.Range(0, possibleChars.Count)];
            } else {
                pickedChar = ERROR_CHAR;
            }
            
            // TODO: DEBUG. FIX THIS
            //entropies[slot] = int.MaxValue;
            //queue.Update(slot, int.MaxValue);
            
            /*
            if (possibleChars.Count != 0) {
                pickedChar = possibleChars[Random.Range(0, possibleChars.Count)];
            }
            else {
                pickedChar = ERROR_CHAR;
            }
            */
            

            SetCharAndUpdateAll(slot, pickedChar);
            
            if (queue.Count == 0) {
                isDone = true;
            }
        }

        private List<char> PossibleChars(Position2 p) {
            Dictionary<Position2, Tile> containingP = output.SubTilesContaining(p, DIMENSION);
            List<List<char>> listOfPossibleChars = new List<List<char>>();
            
            foreach (var (position, tile) in containingP) {
                List<char> characters = new List<char>();
                if (!output.IsInside(p + position)) {
                    throw new Exception("FATAL ERROR: everything is broken. THat is physically not possible");
                }

                List<Tile> possibleTiles;
                try {
                    possibleTiles = possibilities[p + position];
                }
                catch {
                    throw new Exception("EXTREMELY WEIRD THING HAPPENED. PROGRAM BROKEN");
                }
                
                
                
                foreach (Tile possibleTile in possibleTiles) {
                    characters.Add(possibleTile.CharAt(-position));
                }
                listOfPossibleChars.Add(characters);
            }

            List<char> possibleChars = new();
            foreach (char inputChar in inputChars) {
                bool isInAll = true;
                foreach (List<char> charLists in listOfPossibleChars) {
                    if (!charLists.Contains(inputChar)) {
                        isInAll = false;
                        break;
                    }
                }
                if (isInAll) possibleChars.Add(inputChar);
            }

            return possibleChars;
        }

        private void SetCharAndUpdateAll(Position2 p, char newChar) {
            SetCharAndUpdatePossibilities(p, newChar);
            UpdateEntropies(p, DIMENSION);
        }
        
        private void SetCharAndUpdatePossibilities(Position2 p, char newChar) {
            output.SetChar(p, newChar);
            Dictionary<Position2, Tile> tilesContainingP = output.SubTilesContaining(p, DIMENSION);
            List<Tile> removedTiles = new List<Tile>();
            foreach (var (position, tile) in tilesContainingP) {
                List<Tile> possibleTiles = possibilities[p + position];
                foreach (Tile possibleTile in possibleTiles) {
                    char c = possibleTile.CharAt(-position);
                    // TODO: Handle error chars
                    //if (!(c == newChar || c == EMPTY_CHAR || c == ERROR_CHAR)) {
                    if (!(c == newChar || c == EMPTY_CHAR || c == ERROR_CHAR) && !(newChar == ERROR_CHAR || newChar == EMPTY_CHAR)) {
                        removedTiles.Add(possibleTile);
                    }

                   
                }
                
                possibleTiles.RemoveAll(c => removedTiles.Contains(c));
                possibilities[p + position] = possibleTiles;

                // TODO: Improve this
                removedTiles = new List<Tile>();
            }
        }

        // TODO: Prefer width and height over dimension
        public void UpdateEntropies(Position2 p, int dimension) {
            for (int x = p.x - dimension + 1; x < p.x + dimension - 1; x++) {
                for (int y = p.y - dimension + 1; y < p.y + dimension - 1; y++) {
                    Position2 position = new Position2(x, y);
                    if (output.IsInside(position)) {
                        List<char> possibleChars = PossibleChars(position);
                        // TODO: DEBUG
                        if (possibleChars.Count == 0) {
                            Debug.Log($"No possible char at {position}");
                        }

                        if (entropies[position] != ENTROPY_COMPLETE) {
                            entropies[position] = possibleChars.Count;
                            queue.Update(position, possibleChars.Count);
                        }
                    }
                }
            }
            
            //TODO: Fix this
            entropies[p] = ENTROPY_COMPLETE;
            queue.Update(p, ENTROPY_COMPLETE);
            




        }

        public bool IsDone() => isDone;
        
        public override string ToString() {
            return $"WFC[\n{Utils.ToString(tiles.ToList(), t => $"{t}")}\n]";
        }

        public class Tile
        {
            protected char[][] table;
            protected HashSet<char> characters;

            protected int width;
            protected int height;

            internal Tile(char[][] table, HashSet<char> characters, int width, int height) {
                this.table = table;
                this.characters = characters;
                this.width = width;
                this.height = height;
            }

            internal Tile(int height, int width) {
                if (height <= 0 || width <= 0) throw new ArgumentException("Height and width must be >0");
                table = new char[height][];
                for (int y = 0; y < height; y++) {
                    table[y] = new char[width];
                    for (int x = 0; x < width; x++) {
                        table[y][x] = EMPTY_CHAR;
                    }
                }
                
                
                this.width = width;
                this.height = height;
                this.characters = new HashSet<char>();
                characters.Add(EMPTY_CHAR);
            }
            private Tile(char[][] table) {
                
                if (table.Length == 0) throw new ArgumentException("Table is empty!");
                int lineLength = table[0].Length;
                characters = new HashSet<char>();
                foreach (char[] cs in table) {
                    if (cs.Length != lineLength) {
                        throw new ArgumentException("All lines must be of the same length!");
                    }
                    for (int x = 0; x < width; x++) {
                        characters.Add(cs[x]);
                    }
                }

                this.table = table;
                height = table.Length;
                width = lineLength;
            }

            internal Tile(string[] strings) {
                height = strings.Length;
                width = strings[0].Length;
                characters = new HashSet<char>();
                this.table = new char[height][];
                //foreach (string s in strings) {
                for (int y = 0; y < height; y++) {
                    if (strings[y].Length != width) {
                        throw new ArgumentException("All lines must be of the same length!");
                    }

                    table[y] = new char[width];
                    for (int x = 0; x < width; x++) {
                        table[y][x] = strings[y].Substring(x, 1).ToCharArray()[0];
                        characters.Add(strings[y][x]);
                    }
                }

            }

            public Tile GetSubTile(Position2 corner1, Position2 corner2, bool flexible) {
                int minX = (corner1.x < corner2.x) ? corner1.x: corner2.x;
                int minY = (corner1.y < corner2.y) ? corner1.y: corner2.y;
                return GetSubTile(corner2.y - corner1.y + 1, corner2.x - corner1.x + 1, minX, minY, flexible);
            }
            
            public Tile GetSubTile(int height, int width, int topLeftCol, int topLeftRow, bool flexible) {
                if (height <= 0 || width <= 0) {
                    throw new ArgumentException($"Height and width must be > 0");
                }

                if (!(0 <= topLeftCol && topLeftCol <= this.width - width && 0 <= topLeftRow && topLeftRow <= this.height - height)) {
                    //throw new ArgumentException("topLeftCol or topLeftRow are invalid: the tile is sticking out of the table!");
                }

                char[][] newTable = new char[height][];
                for (int y = 0; y < height; y++) {
                    newTable[y] = new char[width];
                    for (int x = 0; x < width; x++) {
                        if (IsInside(topLeftCol + x, topLeftRow + y)) {
                            newTable[y][x] = table[topLeftRow + y][topLeftCol + x];
                        } else if (flexible) {
                            newTable[y][x] = EMPTY_CHAR;
                        }
                    }
                }

                /*
                char[][] newTable = new char[height][];
                for (int i = 0; i < height; i++) {
                    newTable[i] = ExtractInLine(i + topLeftRow, topLeftCol, topLeftCol + height - 1);
                }
                */
                
                var newCharacters = new HashSet<char>();
                return new Tile(newTable, newCharacters, width, height);
            }

            public char[] ExtractInLine(int lineNumber, int from, int to) {
                if (!(0 <= from && to < height)) {
                    throw new ArgumentException("Trying to extract outside of string!");
                }

                if (!(0 <= lineNumber && lineNumber < height)) {
                    throw new ArgumentException("Trying to access a line outside the table");
                }

                if (from > to) {
                    (from, to) = (to, from);
                }

                return new ArraySegment<char>(table[lineNumber], from, to - from + 1).ToArray();
            }

            public List<char> GetChars() {
                HashSet<char> chars = new();
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        char c = table[y][x];
                        if (!chars.Contains(c)) {
                            chars.Add(c);
                        }
                    }
                }

                return chars.ToList();
            }
            
            public char[][] GetTable() => table;

            public bool IsInside(Position2 pos) => IsInside(pos.x, pos.y);
            public bool IsInside(int x, int y) {
                return (0 <= x && x < width && 0 <= y && y < height);
            }
            
            public Dictionary<Position2, Tile> SubTilesContaining(Position2 pos, int dimension) {
                Dictionary<Position2, Tile> possibleTiles = new();
                for (int x = -dimension + 1; x <=  0; x++) {
                    for (int y = -dimension + 1; y <= 0; y++) {
                        int newX = pos.x + x;
                        int newY = pos.y + y;
                        if (IsInside(newX + dimension - 1, newY + dimension - 1) && IsInside(newX, newY)) {
                            // TODO: Get rid of dimension in favor of tileWidth and tileHeight
                            possibleTiles.Add(new Position2(x,y), GetSubTile(dimension, dimension, pos.x + x, pos.y + y, false));
                        }
                    }
                }

                return possibleTiles;
            }

            public char CharAt(Position2 position) {
                if (!IsInside(position)) throw new ArgumentException("Position is outside the tile!");
                return table[position.y][position.x];
            }

            public int Height() => height;

            public int Width() => width;

            public bool IsCompatible(Tile other) {
                for (int s = 0; s < table.Length; s++) {
                    for (int i = 0; i < table.Length; i++) {
                        char char1 = table[s][i];
                        char char2 = other.table[s][i];
                        if (!char1.Equals(char2) && !char1.Equals(EMPTY_CHAR) && !char2.Equals(EMPTY_CHAR)) {
                            return false;
                        }
                    }
                }

                return true;
            }

            public bool SetChar(Position2 position, char newChar) {
                if (!IsInside(position)) {
                    return false;
                }

                table[position.y][position.x] = newChar;
                return true;
            }

            public bool IsEmpty() => characters.All(c => c is EMPTY_CHAR or ERROR_CHAR);

            public override string ToString() {
                StringBuilder builder = new StringBuilder();
                builder.Append($"Tile[\n");
                foreach (var s in table) {
                    builder.Append($"'{new string(s)}',\n");
                }

                builder.Append($"]");
                return builder.ToString();
            }

            public override int GetHashCode() {
                // TODO: Improve this, maybe remove Equals entirely
                return base.GetHashCode();
            }

            public override bool Equals(object obj) {
                if (obj == null) return false;
                Tile tile = (Tile)obj;
                if (tile.width != width || tile.height != height) return false;
                for (int x = 0; x < tile.width; x++) {
                    for (int y = 0; y < tile.height; y++) {
                        if (tile.table[y][x] != table[y][x]) return false;
                    }
                }

                return true;
            }
        }

        public class Bitmap : Tile
        {
            public Bitmap(char[][] table, HashSet<char> characters, int width, int height) : base(table, characters, width, height) { }
            
            public bool SetChar(Position2 position, char newChar) {
                if (!IsInside(position)) {
                    return false;
                }

                table[position.y][position.x] = newChar;
                return true;
            }
        }

        public static void TEST_Subtiles() {
            var generator = new WaveFunctionCollapse(ExampleTile, 10, 10);
            generator.output.SubTilesContaining(new Position2(9,9), 3);  // should be 1
            generator.output.SubTilesContaining(new Position2(0, 0), 3);  // should be 1
            generator.output.SubTilesContaining(new Position2(1, 2), 3);  // should be 6
        }

        public static void TEST_Queue() {
            Tile tile1 = new Tile(new string[] {
                "----",
                "----",
                "----",
                "-X--",
            });
            Tile tile2 = new Tile(new string[] {
                "----",
                "----",
                "----",
                "-X--",
            });
            Tile tile3 = new Tile(new string[] {
                "ABCE",
                "----",
                "----",
                "ZEGZ",
            });

            var queue = new PriorityQueue<Tile>();
            queue.Enqueue(tile1, 43);
            queue.Enqueue(tile3, 23);
            queue.Update(tile2, 1);
        }
    }
}