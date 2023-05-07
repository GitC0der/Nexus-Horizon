namespace Prepping.Generators
{
    public abstract class IGenerator
    {
        protected Blockbox blockbox;

        public IGenerator(Blockbox blockbox) {
            this.blockbox = blockbox;
        }
        
        /// <summary>
        ///     Generate a block at a specified location. If a block is already present, nothing will happen
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Block GenerateBlockAt(Position3 position) => GenerateBlock(position, false);

        /// <summary>
        ///     Force the generation of a block at a specified location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Block ForceGenerateBlockAt(Position3 position) => GenerateBlock(position, true);

        protected abstract Block GenerateBlock(Position3 position, bool doForce);
        
        /// <summary>
        ///     Generate a new block in a free space according to internal generation algorithm
        /// </summary>
        /// <returns></returns>
        public Block GenerateNextBlock() => GenerateBlockAt(SetPreviousPosition(GetNextPosition()));
        
        public abstract bool IsDone();
        
        /// <summary>
        ///     Get the position of the previously placed block
        /// </summary>
        /// <returns></returns>

        public abstract Position3 GetPreviousPosition();
        
        /// <summary>
        ///     Get the position of the next block
        /// </summary>
        /// <returns></returns>
        public abstract Position3 GetNextPosition();

        /// <summary>
        ///     Set the previous position to a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected abstract Position3 SetPreviousPosition(Position3 position);
    }
}