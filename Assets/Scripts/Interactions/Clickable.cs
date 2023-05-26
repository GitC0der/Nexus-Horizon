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
            GameManager gameManager = ServiceLocator.GetService<GameManager>();
            Instantiate(gameManager.plazaPrefab, position.AsVector3(), Quaternion.identity);
            Debug.Log(name);
            ServiceLocator.GetService<PropManager>().RemoveProp(gameObject);
            //Debug.Log(ServiceLocator.GetService<PropManager>().PropAt(position + Position3.up).GetGameObject().name);
            //ServiceLocator.GetService<PropManager>().RemovePropAt(position + Position3.up);
        }
    }
}