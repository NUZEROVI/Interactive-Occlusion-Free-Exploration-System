using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityVolumeRendering
{
    public class colliderDetect : MonoBehaviour
    {        
        List<GameObject> sourceGameObjects = new List<GameObject>();
        List<int> sourceChildIndexs = new List<int>();
        List<MeshFilter> sourceMeshFilters = new List<MeshFilter>();

        int num = 0; 
    
        // 1. Find connected object
        void OnCollisionEnter(Collision collision)
        {            
            string str = collision.collider.name;  
 
            if (!sourceGameObjects.Contains(GameObject.Find(collision.collider.name).gameObject))
            {
                sourceGameObjects.Add(GameObject.Find(collision.collider.name).gameObject);
            }


            if (!sourceChildIndexs.Contains(int.Parse(str.Substring(str.LastIndexOf('-') + 1))))
            {
                sourceChildIndexs.Add(int.Parse(str.Substring(str.LastIndexOf('-') + 1)));          
            }


            if (!sourceMeshFilters.Contains(GameObject.Find(collision.collider.name).GetComponent<MeshFilter>()))
            {
                sourceMeshFilters.Add(GameObject.Find(collision.collider.name).GetComponent<MeshFilter>());
            }
          
            GameObject.Find(collision.collider.name).GetComponent<MeshCollider>().isTrigger = true;
        }

        // 2. Combine connected objects
        private void OnCollisionExit(Collision collision)
        {
            num++;           

            if(sourceMeshFilters.Count == num)
            {
                string str = this.name;
                // add self object
                sourceGameObjects.Add(this.gameObject);
                sourceMeshFilters.Add(this.GetComponent<MeshFilter>());
                //sourceChildIndexs.Add(int.Parse(str.Substring(str.LastIndexOf('-') + 1)));
              

                // Combine all connected objects
                GameObject combineObject = new GameObject("compound_collider" + "-" + int.Parse(str.Substring(str.LastIndexOf('-') + 1)));
                combineObject.transform.parent = this.transform.parent;
             
                var combine = new CombineInstance[sourceMeshFilters.Count];
                for (var i = 0; i < sourceMeshFilters.Count; i++)
                {
                    combine[i].mesh = sourceMeshFilters[i].sharedMesh;
                    combine[i].transform = sourceMeshFilters[i].transform.localToWorldMatrix;
                }
                var mesh = new Mesh();
                mesh.CombineMeshes(combine);
                combineObject.AddComponent<MeshFilter>().mesh = mesh;
                MeshRenderer renderer = combineObject.AddComponent<MeshRenderer>();
                renderer.material.color = Random.ColorHSV(0f, 1f, 0.3f, 1f, 0.3f, 1f);
                MeshCollider tmp_c = combineObject.AddComponent<MeshCollider>();
                tmp_c.convex = true;
                tmp_c.isTrigger = false;
                Rigidbody tmp_rigid = combineObject.AddComponent<Rigidbody>();
                tmp_rigid.useGravity = false;
                tmp_rigid.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

                //Destroy Original objects
                for (int i = 0; i < sourceGameObjects.Count; i++)
                {                   
                    Destroy(sourceGameObjects[i]);                  
                }

                for (int i = 0; i < sourceChildIndexs.Count; i++)
                {            
                    this.transform.parent.GetComponent<colliderController>().childrensIndexs.Remove(sourceChildIndexs[i]);
                }

                // continue detect and combine
                combineObject.AddComponent<colliderDetect>();

            }           
        }     
    }
}