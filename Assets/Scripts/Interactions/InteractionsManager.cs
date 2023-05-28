using System.Collections.Generic;
using System.Linq;
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

        public void DestroyBlock(GameObject block) {
            GameManager gameManager = SL.Get<GameManager>();
            string actualName = GetName(block);
            Position3 position = block.transform.position.AsPosition3();
            switch (actualName) {
                case "Window":
                    Surface surface = gameManager.GetSurfaceOn(position);
                    if (surface != null) {
                        foreach (var pos in surface.GetBlocks().Where(p => gameManager.GetBlockbox().BlockAt(p) != Block.Door)) {
                            gameManager.ReplaceBlockAt(pos, Vector3.zero, Block.Building);
                        }
                    }
                    
                    
                    break;
                case "Door":
                    surface = gameManager.GetSurfaceOn(position);
                    Position3 bottom = position;
                    if (gameManager.GetBlockbox().BlockAt(position + Position3.up) == Block.Door) {
                        gameManager.ReplaceBlockAt(position + Position3.up, Vector3.zero, Block.Building);
                    }

                    if (gameManager.GetBlockbox().BlockAt(position + Position3.down) == Block.Door) {
                        gameManager.ReplaceBlockAt(position + Position3.down, Vector3.zero, Block.Building);
                        bottom = position + Position3.down;
                    }
                    gameManager.ReplaceBlockAt(position, Vector3.zero, Block.Building);

                    Surface frontFloor = gameManager.FindSurfaceBelow(bottom + surface.GetNormal());
                    if (frontFloor != null && gameManager.GetBlockbox().GetDoorsLeadingTo(frontFloor.GetBorderPositions()).Count == 0) {
                        gameManager.RemoveAllPropsOn(frontFloor);
                        FloorPainter painter = new FloorPainter(frontFloor, gameManager.GetBlockbox(), gameManager.useLights);
                    }
                    
                    /*
                    var border = surface.GetBorder(BorderType.None);
                    if (border != null) {
                        Position3 pos = border.GetPositions().ToList()[0];
                        var listOfPos = SL.Get<PropManager>().RailingsAt(pos).ToList();
                        if (listOfPos.Count > 0) DestroyProp(listOfPos[0].GetGameObject());
                    }
                    */
                    
                    break;
                case "Building":
                    Position3? doorPos = null;
                    surface = gameManager.GetSurfaceOn(position);
                    
                    // CLicking on facades
                    if (surface != null && surface.IsFacade() && !surface.GetBorderPositions().Contains(position)) {
                        
                        // Removing the doors
                        foreach (Position3 blockPos in surface.GetBlocks()) {
                            if (gameManager.GetBlockbox().BlockAt(blockPos) == Block.Door) {
                                doorPos = blockPos;
                            }
                        }

                        if (doorPos != null) {
                            DestroyBlock(gameManager.BlockGameObjectAt(doorPos.Value));
                        }

                        // Replacing the blocks
                        FacadePainter painter = new FacadePainter(surface, gameManager.GetBlockbox());
                        var blocks = painter.GetBlocks();
                        var shifts = painter.GetShifts();
                        foreach (var (pos, b) in blocks) {
                            gameManager.ReplaceBlockAt(pos, shifts[pos], b);
                        }
                        
                        // Creating a new terrace
                        Position3? newDoorPos = null;
                        if (gameManager.GetBlockbox().GetDoorsLeadingTo(surface.GetBorderPositions()).Count == 1) {
                            foreach (Position3 surfacePos in surface.GetBlocks()) {
                                if (gameManager.GetBlockbox().IsDoor(surfacePos)) newDoorPos = surfacePos;
                            }
                        }


                        if (newDoorPos != null) {
                            Surface floor = gameManager.FindSurfaceBelow(newDoorPos.Value + surface.GetNormal());
                            if (floor == null) {
                                floor = gameManager.FindSurfaceBelow(newDoorPos.Value + Position3.up + surface.GetNormal());
                                if (floor == null) {
                                    floor = gameManager.FindSurfaceBelow(newDoorPos.Value + Position3.down + surface.GetNormal());
                                }
                            }
                            gameManager.RemoveAllPropsOn(floor);

                            FloorPainter floorPainter = new FloorPainter(floor, gameManager.GetBlockbox(), gameManager.useLights);
                        }
                        
                        // Clicking on floors
                    } else if (surface != null && surface.IsFloor()) {
                        Surface floor = gameManager.FindSurfaceBelow(position + Position3.up);
                        gameManager.RemoveAllPropsOn(floor);
                        FloorPainter painter = new FloorPainter(surface, gameManager.GetBlockbox(), gameManager.useLights);
                        
                    }
                    
                    
                    break;
                default:
                    break;
            }
        }

        private string GetName(GameObject gameobject) {
            int openingParenthesisIndex = gameobject.name.IndexOf('(');
            string extractedString = gameobject.name.Substring(0, openingParenthesisIndex);
            return extractedString.Trim();
        }

    }
}