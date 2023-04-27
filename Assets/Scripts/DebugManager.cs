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
                
                var generator = new WaveFunctionCollapse(WaveFunctionCollapse.ExampleTile);
                while (!generator.IsDone()) {
                    generator.GenerateNextSlot();
                }

                Debug.Log(generator.OutputToString());
            }
        }

        // Update is called once per frame
        void Update() {
            if (Enabled) {
                
            }
        }
    }
}