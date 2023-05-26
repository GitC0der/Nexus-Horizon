using System.Collections.Generic;
using Painting;
using Prepping;
using UnityEngine;

namespace Interactions
{
    public class InteractionsManager : MonoBehaviour
    {
        public void Initialize() {

        }

        public bool DestroyProp(GameObject prop) {
            string actualName = GetName(prop);
            switch (actualName) {
                case "Railing":
                    Surface surface = SL.Get<GameManager>().FindSurfaceBelow(prop.transform.position.AsPosition3() + Position3.up);
                    if (surface != null) {
                        foreach (Position3 pos in surface.GetBlocks()) {
                            //Instantiate(SL.Get<GameManager>().plazaPrefab, pos.AsVector3(), Quaternion.identity);
                            var railings = SL.Get<PropManager>().RailingsAt(pos);
                            foreach (ActualProp railing in railings) {
                                SL.Get<PropManager>().RemoveProp(railing.GetGameObject());
                            }
                        }
                    }
                    
                    /*
                    void SearchNeighbors(Position3 pos) {
                        HashSet<GameObject> discovered = new HashSet<GameObject>();
                        foreach (Position3 neighborPos in NeighborPositions(pos)) {
                            var neighbors = propManager.RailingsAt(neighborPos);
                            foreach (ActualProp neighborProp in neighbors) {
                                if (neighborProp != null && neighborProp.GetGameObject() != null && !discovered.Contains(neighborProp.GetGameObject()) && GetName(neighborProp.GetGameObject()) == "Railing") {
                                    discovered.Add(neighborProp.GetGameObject());
                                    propManager.RemoveProp(neighborProp.GetGameObject());
                                    SearchNeighbors(neighborPos);
                                }
                            }
                            
                        }
                    }
                    SearchNeighbors(prop.transform.position.AsPosition3());
                    */
                    break;
                default:
                    SL.Get<PropManager>().RemoveProp(prop);
                    break;
            }
            return false;
        }

        public HashSet<Position3> NeighborPositions(Position3 position) {
            return new() {
                position + Position3.up,
                position + Position3.down,
                position + Position3.left,
                position + Position3.right,
                position + Position3.forward,
                position + Position3.back
            };
        }

        public bool DestroyBlock(GameObject block) {
            string actualName = GetName(block);
            return false;
        }

        private string GetName(GameObject gameobject) {
            int openingParenthesisIndex = gameobject.name.IndexOf('(');
            string extractedString = gameobject.name.Substring(0, openingParenthesisIndex);
            return extractedString.Trim();
        }

    }
}