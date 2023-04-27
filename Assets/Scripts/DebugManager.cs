using System;
using System.Collections.Generic;
using Data;
using Painting;
using Prepping;
using UnityEngine;

namespace DefaultNamespace
{
    
    public class DebugManager : MonoBehaviour
    {
        public bool Enabled;

        void Start() {
            if (Enabled) {
                
                WaveFunctionCollapse.TEST_Subtiles();
                WaveFunctionCollapse.TEST_Queue();
                
                var generator = new WaveFunctionCollapse(WaveFunctionCollapse.ExampleTile);
                int slotsCOunt = 1;
                while (!generator.IsDone()) {
                    if (slotsCOunt == 26) {
                        var debug = "dewifwefwiefbwiefbwf";
                    }
                    generator.GenerateNextSlot();
                    ++slotsCOunt;
                }

                Debug.Log($"-------------\n DONE! Here's the result: {generator.OutputToString()}");
            }
        }

        // Update is called once per frame
        void Update() {
            if (Enabled) {
                
            }
        }
    }
}