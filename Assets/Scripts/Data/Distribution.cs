using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Prepping;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data
{

    // TODO: Rename this into DistributionS, and create class Distribution to avoid boilerplate
    public static class Distribution
    {
        
        private static Dictionary<Block, double> Building_Above = new Dictionary<Block, double>() {
            { Block.Building, 20 },
            { Block.Park, 1 },
            { Block.Void, 0 }
        };

        private static Dictionary<Block, double> Building_Next = new Dictionary<Block, double>() {
            { Block.Building, 50 },
            { Block.Park, 1 },
            { Block.Void, 10 }
        };

        private static Dictionary<Block, double> Building_Below = new Dictionary<Block, double>() {
            { Block.Building, 20 },
            { Block.Park, 0 },
            { Block.Void, 2 }
        };

        private static Dictionary<Block, double> Park_Above = new Dictionary<Block, double>() {
            { Block.Building, 0 },
            { Block.Park, 0 },
            { Block.Void, 1 }
        };

        private static Dictionary<Block, double> Park_Next = new Dictionary<Block, double>() {
            { Block.Building, 1 },
            { Block.Park, 10 },
            { Block.Void, 2 }
        };

        private static Dictionary<Block, double> Park_Below = new Dictionary<Block, double>() {
            { Block.Building, 1 },
            { Block.Park, 0 },
            { Block.Void, 0 }
        };

        private static Dictionary<Block, double> Void_Above = new Dictionary<Block, double>() {
            { Block.Building, 2 },
            { Block.Park, 0 },
            { Block.Void, 50 }
        };

        private static Dictionary<Block, double> Void_Next = new Dictionary<Block, double>() {
            { Block.Building, 2 },
            { Block.Park, 1 },
            { Block.Void, 20 }
        };

        private static Dictionary<Block, double> Void_Below = new Dictionary<Block, double>() {
            { Block.Building, 1 },
            { Block.Park, 1 },
            { Block.Void, 50 }
        };
        
        private static Dictionary<Block, Dictionary<Block, double>> nextDistributions =
            new Dictionary<Block, Dictionary<Block, double>>() {
                { Block.Building, Building_Next },
                { Block.Park, Park_Next },
                { Block.Void, Void_Next }
            };
        
        private static Dictionary<Block, Dictionary<Block, double>> aboveDistributions =
            new Dictionary<Block, Dictionary<Block, double>>() {
                { Block.Building, Building_Above },
                { Block.Park, Park_Above },
                { Block.Void, Void_Above }
            };
        
        private static Dictionary<Block, Dictionary<Block, double>> belowDistributions =
            new Dictionary<Block, Dictionary<Block, double>>() {
                { Block.Building, Building_Below },
                { Block.Park, Park_Below },
                { Block.Void, Void_Below }
            };

        public static string ToString(Dictionary<Block, double> distribution) {
            return $"Distribution[{DebugUtils.ToString(distribution, block => $"{block}", value => $"{value}")}]";
        }
        
        private static List<Block> BlocksOrder = new List<Block>() { Block.Building, Block.Park, Block.Void };

        private static Dictionary<Block, double> MixDistributions(List<Dictionary<Block, double>> distributions) {
            if (distributions.Count == 0) {
                throw new ArgumentException("ERROR: MixDistributions was called with an empty list of distributions");
            } else if (distributions.Count == 1) {
                return distributions[0];
            }
            Dictionary<Block, double> distribution = distributions[0];
            for (int i = 1; i < distributions.Count - 1; i++) {
                distribution = MixDistributions(distribution, distributions[i]);
            }

            return distribution;
        }
        
        // TODO: Set to private
        public static Dictionary<Block, double> MixDistributions(Dictionary<Block, double> distr1, Dictionary<Block, double> distr2) {
            List<Block> blocksOrder = BlocksOrder;
            Dictionary<Block, double> newDistr = new Dictionary<Block, double>();

            double sum1 = 0;
            foreach (var pair in distr1) {
                sum1 += pair.Value;
            }
            double sum2 = 0;
            foreach (var pair in distr1) {
                sum2 += pair.Value;
            }
            
            for (int i = 0; i < distr1.Count; i++) {
                Block currentBlock = blocksOrder[i];
                double value1;
                double value2;
                distr1.TryGetValue(currentBlock, out value1);
                distr2.TryGetValue(currentBlock, out value2);
                newDistr.Add(currentBlock, value1 / sum1 * value2 / sum2);
            }

            return newDistr;

        }

        private static Block PickBlock(Dictionary<Block, double> distribution) {
            if (distribution.Count == 0) {
                throw new Exception("Fatal ERROR: distribution is empty!");
            }
            double totalCoefficients = distribution.Values.Sum();
            double epsilon = 1e-4;
            if (-epsilon <= totalCoefficients && totalCoefficients <= epsilon) {
                throw new Exception("Fatal ERROR: all blocks have probability 0");
            }
            
            double randomValue = Random.value * totalCoefficients;
    
            foreach (var pair in distribution)
            {
                randomValue -= pair.Value;
                if (randomValue <= 0)
                {
                    return pair.Key;
                }
            }

            // This should never happen, but return null to indicate an error just in case.
            throw new Exception("ERROR: went beyond possibile probability");
        }

        public static Block PickBlock(Dictionary<Position3, Block> neighbors, Position3 currentPos) {
            List<Dictionary<Block, double>> distributions = new List<Dictionary<Block, double>>();
            foreach (var (position, block) in neighbors) {
                distributions.Add(SelectDistribution(block, position, currentPos));
            }

            if (distributions.Count == 0) {
                Func<Dictionary<Block, double>, string> formatter = distr =>
                    DebugUtils.ToString(distr, block => $"{block}", value => $"{value}");
                Debug.Log($"{DebugUtils.ToString(distributions, formatter)} |||| {currentPos}");
                throw new Exception("ERROR: no distribution was found");
            }

            return PickBlock(MixDistributions(distributions));
        }

        private static Dictionary<Block, double> SelectDistribution(Block previousBlock, Position3 previousPos, Position3 current) {
            Position3 direction = current - previousPos;
            Dictionary<Block, double> distr = new Dictionary<Block, double>();
            if (direction == Position3.left || direction == Position3.right || direction == -Position3.left || direction == -Position3.right ||
            direction == Position3.forward || direction == Position3.back || direction == -Position3.forward || direction == -Position3.back) {
                nextDistributions.TryGetValue(previousBlock, out distr);
                Debug.Log($"Next distribution was chosen");
            } else if (direction == Position3.up) {
                Debug.Log($"Above distribution was chosen");
                aboveDistributions.TryGetValue(previousBlock, out distr);
            } else if (direction == Position3.down) {
                Debug.Log($"Below distribution was chosen");
                belowDistributions.TryGetValue(previousBlock, out distr);
            } else {
                throw new Exception("ERROR: direction vector is broken");
            }
            
            Debug.Log($"Distribution is {ToString(distr)}");

            return distr;
        }
    }
}