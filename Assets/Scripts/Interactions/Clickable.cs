using System;
using Painting;
using Prepping;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Interactions
{
    public class Clickable : MonoBehaviour
    {
        private void OnMouseDown() {
            Position3 position = new Position3(transform.position);
            GameManager gameManager = SL.Get<GameManager>();
            //Instantiate(gameManager.plazaPrefab, position.AsVector3(), Quaternion.identity);
            Debug.Log(name);
            GameObject parent = FindHighestParent(gameObject);
            
            //Debug.Log(ServiceLocator.GetService<PropManager>().PropAt(position + Position3.up).GetGameObject().name);
            //ServiceLocator.GetService<PropManager>().RemovePropAt(position + Position3.up);
            Debug.Log($"Object is {parent.name}");

            switch (parent.transform.parent.gameObject.name) {
                case "Prop Holder":
                    //ServiceLocator.GetService<PropManager>().RemoveProp(parent);
                    SL.Get<InteractionsManager>().DestroyProp(parent);
                    break;
                case "Cube Holder":
                    //Destroy(parent);
                    SL.Get<InteractionsManager>().DestroyBlock(parent);
                    break;
            }
        }
        
        private GameObject FindHighestParent(GameObject childObject) {
            var parent = childObject.transform.parent;
            if (parent != null) {
                if (parent.gameObject != null && (parent.gameObject.name is "Prop Holder" or "Cube Holder")) {
                    return childObject;
                } else if (parent.gameObject != null) {
                    return FindHighestParent(parent.gameObject);
                }
            }
            

            return null; // No parent found that is a child of "Prefabs"
        }
    }
}