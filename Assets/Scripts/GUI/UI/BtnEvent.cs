using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class BtnEvent : MonoBehaviour
    {
        public Button btn;  
  
        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;
        //float viewCamIndex;   

        void Start()
        {
            btn = this.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => BtnClick(btn.name));
            }         
        }

        void BtnClick(string btn)
        {

            VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1) 
            {
                meshRenderer = objects[0].meshRenderer;
                mat = meshRenderer.material;
            }

            VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
            if (objects_Mask.Length == 1)
            {
                meshRenderer_Mask = objects_Mask[0].meshRenderer;
                mat_Mask = meshRenderer_Mask.material;
            }

            if (btn == "RightBtn")
            {

                objects[0].gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.forward);
                objects_Mask[0].gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.forward);
            }
            else if (btn == "LeftBtn")
            {
                objects[0].gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.forward);
                objects_Mask[0].gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.forward);
            }
            else if (btn == "DownBtn")
            {
                objects[0].gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.right);
                objects_Mask[0].gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.right);
            }
            else if (btn == "UpBtn")
            {

                objects[0].gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);
                objects_Mask[0].gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);
            }

            var localVector = objects[0].gameObject.transform.InverseTransformDirection(Vector3.forward);

            float depthNP = Vector3.Dot(localVector.normalized, Camera.main.WorldToScreenPoint(Vector3.up));

            int localDepth = -1;
            if (localVector.x != 0) localDepth = 1; // x
            else if (localVector.y != 0) localDepth = 2; // y
            else if (localVector.z != 0) localDepth = 3; // z

         
            mat.SetFloat("_localDepth", localDepth);
            mat.SetFloat("_depthNP", depthNP);

            mat.SetFloat("RotationX", objects[0].gameObject.transform.rotation.eulerAngles.x);
            mat.SetFloat("RotationY", objects[0].gameObject.transform.rotation.eulerAngles.y);
            mat.SetFloat("RotationZ", objects[0].gameObject.transform.rotation.eulerAngles.z);

            mat_Mask.SetFloat("_localDepth", localDepth);
            mat_Mask.SetFloat("_depthNP", depthNP);

            mat_Mask.SetFloat("RotationX", objects[0].gameObject.transform.rotation.eulerAngles.x);
            mat_Mask.SetFloat("RotationY", objects[0].gameObject.transform.rotation.eulerAngles.y);
            mat_Mask.SetFloat("RotationZ", objects[0].gameObject.transform.rotation.eulerAngles.z);




        }
       
    }
}