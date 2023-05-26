using System;
using Unity.Profiling;
using Random = UnityEngine.Random;

namespace Prepping
{
    
    
    public enum Block
    {
        //NULL,
        Void,
        Building,
        Park,
        Walkway,
        Window,
        Door,
        Utilities,
        Plaza
        
        
    }

    public static class Blocks
    {
        public static Block RandomBlock(bool noVoid)
        {
            // Get the number of values in the enum
            int count = Enum.GetValues(typeof(Block)).Length;

            // Generate a random index between 0 and the number of values
            int index = noVoid ? Random.Range(1, count) : Random.Range(0, count);

            // Convert the index to an enum value and return it
            return (Block)Enum.ToObject(typeof(Block), index);
        }
    }
}