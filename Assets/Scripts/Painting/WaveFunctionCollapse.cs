using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = UnityEngine.Random;
//using NUnit.Framework;


namespace Painting
{
    
    public class WaveFunctionCollapse
    {
        public const char EMPTY_CHAR = '?';
        public const char ERROR_CHAR = '@';

        private const int ENTROPY_COMPLETE = int.MaxValue;
        private const bool IS_BORDERLESS = false;
        private HashSet<Tile> tiles;
        private bool isDone;
        private PriorityQueue<Position2> queue;
        private HashSet<char> inputChars;
        private readonly int _dimension;

        private Tile output;
        private Dictionary<Position2, HashSet<Tile>> possibilities;
        private Dictionary<Position2, int> entropies;

        public static readonly Tile Facade2 = new Tile(new string[] {
            "---WWWW----WWWW-----",
            "--------------------",
            "---AAAAAA----WWWW---",
            "---AAAAAA----BBBB---",
            "--------------------",
            "--WWWWW-----WWWWW---",
            "-BBBBBBBBBBBBBBBBBB-",
            "--------------------",
            "--WWWWW-----WWWWW---",
            "----------------AAA-",
            "-----CCCCC------AAA-",
            "-WW--C----------AAA-",
            "-----C--WWWW--------",
            "CCCCCC--WWWW--------",
            "--------------------",
            "---WWWW-----WWWWW---",
            "---BBBB-----BBBBB---",
            "--------------------",
            "----DD--WWWW----WWW-",
            "----DD--WWWW----WWW-"
        });

        public static readonly Tile Facade1 = new Tile(new string[] {
            "--------------------",
            "------WWWW----WWWW--",
            "--WWWW----WWWW------",
            "------WWWW----WWWW--",
            "--WWWW----WWWW------",
            "--------------------",
            "--WWWWWWWWW---CCCCCC",
            "--------------C-----",
            "--WWWWWWWWW---C-----",
            "---CCCCCCCCCCCC-----",
            "---C---WWWW---WWWW--",
            "---C----------------",
            "CCCC---WWWW---WWWW--",
            "--------------------",
            "--W----WWWWWWWWWWW--",
            "--------------------",
            "--W----WWWWWWWWWWW--",
            "--------------------",
            "----DD---WWWW--WWWW-",
            "----DD---WWWW--WWWW-",
        });

        public static readonly Tile Roof1 = new Tile(new string[] {
            "BBBBBBBBBBBBBBBBBBBB",
            "B------------------B",
            "B---SSSS-------C---B",
            "B---S----------C---B",
            "B---S--------------B",
            "B------------------B",
            "B------------------B",
            "B-----------SS-----B",
            "B------------S-----B",
            "B---CC-------S-----B",
            "B------------S-----B",
            "B------------S-----B",
            "B---CC------SS-----B",
            "B------------------B",
            "B-------C----------B",
            "B-------C----------B",
            "B-----S-----S------B",
            "B-----SSSSSSS------B",
            "B------------------B",
            "BBBBBBBBBBBBBBBBBBBB",
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
        
        public static readonly Tile DemoTerrace = new Tile(new string[] {
            "-L---L----L---L-----",
            "----SS-------------L",
            "----SS--------TT--P-",
            "----SS--------TT----",
            "L---------P---------",
            "--CCC--------------L",
            "--CCC-------------SS",
            "----------------P-SS",
            "----TT----CCC-----SS",
            "L---TT----CCC-------",
            "--P----------------L",
            "--------P---TT------",
            "---SS-------TT------",
            "L--SS-------------P-",
            "---SS---TT----------",
            "--------TT----------",
            "------P-----CCC----L",
            "------------CCC-----",
            "L--P----------------",
            "---L-----L---L----L-",
        });
        
        public static readonly Tile DemoTerraceOld = new Tile(new string[] {
            "-L---L----L---L-----",
            "-----P-------------L",
            "----CCC-------TT--P-",
            "----CCC-------TT----",
            "L---------P---------",
            "-------------------L",
            "--------------------",
            "---P-------CC-------",
            "----TT-----CC-------",
            "L---TT-----CC-------",
            "--P----------------L",
            "--------P---TT------",
            "------------TT------",
            "L-----------------P-",
            "--------TT----------",
            "---P----TT----------",
            "------------CCC----L",
            "------------CCC-----",
            "L--P----------------",
            "---L-----L---L----L-",
        });

        // TODO: Fix "sticking out of table" error when input length != input width
        
        // TODO: Add backtracking
        // TODO: Allow for predefined chars (i.e first place them, then generate)
        /// <summary>
        ///     Create a WaveFunctionCollapse generator. It receives a pattern as input and tries to reproduce it on an
        ///     output of given width and height. Sometimes the algorithms fails to find a solution at a specific point
        ///     and thus generates an error. Backtracking will be implemented later to avoid that. <br></br><br></br>
        ///     It also needs a first character to be placed. Note that this character should be placed in a coherent location with respect to
        ///     the input pattern, otherwise errors will happen and the pattern will be broken
        /// </summary>
        /// <param name="inputMap">The input pattern</param>
        /// <param name="outputWidth">The desired width of the output</param>
        /// <param name="outputHeight">The desired height</param>
        /// <param name="initPos">The position of the first character to be placed</param>
        /// <param name="initChar">The first character to be placed</param>
        /// <param name="dimension">The dimension of the tiles</param>
        public WaveFunctionCollapse(Tile inputMap, int outputWidth, int outputHeight, Position2 initPos, char initChar, int dimension) {
            _dimension = dimension;
            tiles = new HashSet<Tile>();
            for (int row = 0; row <= inputMap.Width() - _dimension; row++) {
                for (int col = 0; col <= inputMap.Height() - _dimension; col++) {
                    tiles.Add(inputMap.GetSubTile(new Position2(col, row), new Position2(col + _dimension - 1, row + _dimension - 1), false));
                }
            }
            
            inputChars = inputMap.GetChars();

            Initialize(outputWidth, outputHeight, initPos, initChar);

        }
        
        /// <summary>
        ///     This is an optimized constructor. That way, no need to copy the input tiles and char each and every time.
        ///     Note that you need to precompute the inputTiles and chars.
        ///     For a single generation, prefer the other constructor
        /// </summary>
        public WaveFunctionCollapse(HashSet<Tile> inputTiles, HashSet<char> inputCharacters, int outputWidth, int outputHeight, Position2 initPos, char initChar) {
            tiles = inputTiles;
            inputChars = inputCharacters;
            Initialize(outputWidth, outputHeight, initPos, initChar);
        }

        private void Initialize(int outputWidth, int outputHeight, Position2 initPos, char initChar) {
            if (initPos.x >= outputWidth || initPos.y >= outputHeight) {
                throw new ArgumentException($"Initpos {initPos} is outside the output (of width = {outputWidth} and height = {outputHeight})");
            }
            
            output = new Tile(outputHeight, outputWidth);

            possibilities = new Dictionary<Position2, HashSet<Tile>>();
            queue = new PriorityQueue<Position2>();
            entropies = new Dictionary<Position2, int>();
            for (int x = 0; x < outputWidth; x++) {
                for (int y = 0; y < outputHeight; y++) {
                    Position2 position = new Position2(x, y);
                    if ( x < outputWidth - _dimension + 1 && y < outputHeight - _dimension + 1) {
                        possibilities.Add(position, new HashSet<Tile>(tiles));
                    }
                    queue.Enqueue(position, inputChars.Count);
                    entropies.Add(position, inputChars.Count);
                }
            }
            
            SetCharAndUpdateAll(initPos, initChar);
        }

        /// <summary>
        ///     Simply returns the output as a string. For visualization purposes only! Use GetOutput otherwise
        /// </summary>
        /// <returns></returns>
        public string OutputToString() {
            //if (!IsDone()) Debug.Log("WARNING: WFC generator is not done! Result will be incomplete");
            return $"{output}";
        }

        /// <summary>
        ///     Returns the output character table
        /// </summary>
        /// <returns></returns>
        public char[][] GetOutput() {
            //if (!isDone) Debug.Log("WARNING: WFC generator is not done! Result will be incomplete");
            return output.GetTable();
        }

        public char[][] GenerateAndGetOutput() {
            while (!IsDone()) {
                GenerateNextSlot();
            }

            return null;
        }
        
        /// <summary>
        ///     Generate the next character. The position with the fewest possibilities (i.e the lowest entropy) will
        ///     be selected, and one of the possible character will be randomly selected. For now the probability distribution
        ///     of the possible character is linear, it could be changed later. <br></br>
        ///    
        /// </summary>
        /// <returns>The char that has just been placed</returns>
        /// <exception cref="ArgumentException"></exception>
        public char GenerateNextSlot() {
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

            // If the next slot has no possible character
            if (entropies[queue.Peek()] == 0) {
                SetCharAndUpdateAll(queue.Peek(), ERROR_CHAR);
                //Debug.Log($"ERROR: No possible character at {queue.Peek()}");
                return ERROR_CHAR;
            }
            
            Position2 slot = queue.Dequeue();
            
            // TODO: Store the possible characters instead of the entropies
            List<char> possibleChars = PossibleChars(slot);
            char pickedChar;

            
            if (possibleChars.Count != 0) {
                pickedChar = possibleChars[Random.Range(0, possibleChars.Count)];
            } else {
                pickedChar = ERROR_CHAR;
            }
            
            SetCharAndUpdateAll(slot, pickedChar);
            
            if (queue.Count == 0) isDone = true;
            
            return pickedChar;
        }

        // TODO: Investigate possibility of optimization by not copying the tiles but simply returning the position of the char
        private List<char> PossibleChars(Position2 p) {
            HashSet<Position2> positions = output.PositionsOfSubtilesContaining(p, _dimension);
            HashSet<HashSet<char>> listOfPossibleChars = new HashSet<HashSet<char>>();
            
            foreach (Position2 position in positions) {
                HashSet<char> characters = new HashSet<char>();
                if (!output.IsInside(p + position)) {
                    throw new Exception("FATAL ERROR: everything is broken. THat is physically not possible");
                }

                HashSet<Tile> possibleTiles;
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
                foreach (HashSet<char> charLists in listOfPossibleChars) {
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
            UpdateEntropies(p, _dimension);
        }
        
        private void SetCharAndUpdatePossibilities(Position2 p, char newChar) {
            output.SetChar(p, newChar);
            HashSet<Position2> positions = output.PositionsOfSubtilesContaining(p, _dimension);

            // Using a HashSet instead of a List for removedTiles
            HashSet<Tile> removedTiles = new HashSet<Tile>();

            // Using a for loop instead of a foreach loop for positions
            for (int i = 0; i < positions.Count; i++) {
                Position2 position = positions.ElementAt(i);
                HashSet<Tile> possibleTiles = possibilities[p + position];

                // Using IntersectWith to check if an item is in the HashSet
                possibleTiles.IntersectWith(possibleTiles.Where(t => {
                    char c = t.CharAt(-position);
                    return (c == newChar || c == EMPTY_CHAR || c == ERROR_CHAR || newChar == ERROR_CHAR || newChar == EMPTY_CHAR);
                }));

                // Adding removed tiles to the HashSet
                removedTiles.UnionWith(possibleTiles.Except(removedTiles));

                possibilities[p + position] = possibleTiles;
            }
        }

        
        // private void SetCharAndUpdatePossibilities(Position2 p, char newChar) {
        //     output.SetChar(p, newChar);
        //     HashSet<Position2> positions = output.PositionsOfSubtilesContaining(p, DIMENSION);
        //     List<Tile> removedTiles = new List<Tile>();
        //     
        //     // Iterating over all tiles which will be affected by the placement of newChar at p
        //     foreach (Position2 position in positions) {
        //         HashSet<Tile> possibleTiles = possibilities[p + position];
        //         
        //         // Removing the tiles that are no longer possible
        //         foreach (Tile possibleTile in possibleTiles) {
        //             char c = possibleTile.CharAt(-position);
        //             if (!(c == newChar || c == EMPTY_CHAR || c == ERROR_CHAR) && !(newChar == ERROR_CHAR || newChar == EMPTY_CHAR)) {
        //                 removedTiles.Add(possibleTile);
        //             }
        //
        //            
        //         }
        //         
        //         possibleTiles.RemoveWhere(c => removedTiles.Contains(c));
        //         possibilities[p + position] = possibleTiles;
        //
        //         // TODO: Improve this
        //         removedTiles = new List<Tile>();
        //     }
        // }
        //

        // TODO: Prefer width and height over dimension
        public void UpdateEntropies(Position2 p, int dimension) {
            for (int x = p.x - dimension + 1; x < p.x + dimension - 1; x++) {
                for (int y = p.y - dimension + 1; y < p.y + dimension - 1; y++) {
                    Position2 position = new Position2(x, y);
                    if (output.IsInside(position)) {
                        List<char> possibleChars = PossibleChars(position);
                        // TODO: DEBUG
                        if (possibleChars.Count == 0) {
                            //Debug.Log($"No possible char at {position}");
                        }

                        if (entropies[position] != ENTROPY_COMPLETE) {
                            entropies[position] = possibleChars.Count;
                            queue.Update(position, possibleChars.Count);
                        }
                    }
                }
            }
            
            entropies[p] = ENTROPY_COMPLETE;
            queue.Update(p, ENTROPY_COMPLETE);

        }

        public bool IsDone() => isDone;
        
        public override string ToString() {
            return $"WFC[\n{Utils.ToString(tiles.ToList(), t => $"{t}")}\n]";
        }

        public class Tile
        {
            private char[][] table;
            private HashSet<char> characters;

            private int width;
            private int height;

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
                    throw new ArgumentException("topLeftCol or topLeftRow are invalid: the tile is sticking out of the table!");
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

            public HashSet<char> GetChars() {
                HashSet<char> chars = new();
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        char c = table[y][x];
                        if (!chars.Contains(c)) {
                            chars.Add(c);
                        }
                    }
                }

                return chars;
            }
            
            public char[][] GetTable() => table;

            public bool IsInside(Position2 pos) => IsInside(pos.x, pos.y);
            public bool IsInside(int x, int y) {
                return (0 <= x && x < width && 0 <= y && y < height);
            }

            public HashSet<Position2> PositionsOfSubtilesContaining(Position2 pos, int dimension) {
                HashSet<Position2> positions = new HashSet<Position2>();
                for (int x = -dimension + 1; x <=  0; x++) {
                    for (int y = -dimension + 1; y <= 0; y++) {
                        int newX = pos.x + x;
                        int newY = pos.y + y;
                        if (IsInside(newX + dimension - 1, newY + dimension - 1) && IsInside(newX, newY)) {
                            positions.Add(new Position2(x, y));
                        }
                    }
                }

                return positions;
            }

            public HashSet<Tile> GetAllSubtiles(int dimension) {
                HashSet<Tile> tiles = new HashSet<Tile>();
                for (int row = 0; row <= width - dimension; row++) {
                    for (int col = 0; col <= height - dimension; col++) {
                        tiles.Add(GetSubTile(new Position2(col, row), new Position2(col + dimension - 1, row + dimension - 1), false));
                    }
                }

                return tiles;
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
    }
}