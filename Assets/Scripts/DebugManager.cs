using System.Collections.Generic;
using Data;
using Prepping;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    
    public class DebugManager : MonoBehaviour
    {
        public bool Enabled;

        // Update is called once per frame
        void Update() {
            if (Enabled) {
                Dictionary<Block, double> distr1 = new Dictionary<Block, double>() {
                    { Block.Building, 2 },
                    { Block.Park, 5 },
                    { Block.Void, 0 }
                };
                Dictionary<Block, double> distr2 = new Dictionary<Block, double>() {
                    { Block.Building, 7 },
                    { Block.Park, 3 },
                    { Block.Void, 1 }
                };

                Dictionary<Block, double> distribution = Distribution.MixDistributions(distr1, distr2);
                List<double> l = new List<double>();
                l.Add(3.4);

            }
        }
    }
}