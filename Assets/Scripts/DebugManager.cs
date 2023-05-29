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

                var generator = new WaveFunctionCollapse(WaveFunctionCollapse.Facade1, 30, 50, new Position2(1,1), 'X', 3);
                int slotsCOunt = 1;
                while (!generator.IsDone()) {
                    if (slotsCOunt == 16) {
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