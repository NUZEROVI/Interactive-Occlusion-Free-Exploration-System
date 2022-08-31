using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class GetCollidedIndex : MonoBehaviour
    {
       
        public bool IsNotColliding = true;
  
        private void OnTriggerEnter(Collider other)
        {
            string str = other.name;
            int index = int.Parse(str.Substring(str.LastIndexOf('-') + 1));
            if (this.transform.parent.GetComponent<colliderController>().childrensIndexs.Contains(index))
            {
               this.transform.parent.GetComponent<colliderController>().isCollided[index] = true;            
            }
            other.isTrigger = false;
            this.GetComponent<Collider>().isTrigger = false;
        }

      
    }

}