using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityVolumeRendering
{
    public class ForRemoteRotate : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [Header("Settings")]
        public float RotateSpeed = 5f;

        bool isClick = false;
        private Transform localTrans, tmpTrans;
        MeshRenderer meshRenderer;
        Material mat;


        void Start()
        {
            
            VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
            {
                meshRenderer = objects[0].meshRenderer;
                mat = meshRenderer.material;
            }

            Hover[] subObj = FindObjectsOfType<Hover>();
            if(subObj.Length == 1)
            {
                localTrans = subObj[0].GetComponent<Transform>();
            }            

        }

        public void OnDrag(PointerEventData data)
        {
            float rotX = Input.GetAxis("Mouse X") * RotateSpeed;

            float rotY = Input.GetAxis("Mouse Y") * RotateSpeed;

            Camera camera = Camera.main;

            Vector3 right = Vector3.Cross(camera.transform.up, localTrans.position - camera.transform.position);

            Vector3 up = Vector3.Cross(localTrans.position - camera.transform.position, right);

            localTrans.rotation = Quaternion.AngleAxis(-rotX, up) * localTrans.rotation;

            localTrans.rotation = Quaternion.AngleAxis(rotY, right) * localTrans.rotation;

            localTrans.localScale = new Vector3(mat.GetFloat("ObjDepthX"), mat.GetFloat("ObjDepthY"), mat.GetFloat("ObjDepthZ"));
            LimitRot();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            localTrans.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            localTrans.parent.localScale = new Vector3(mat.GetFloat("ObjDepthX"), mat.GetFloat("ObjDepthY"), mat.GetFloat("ObjDepthZ"));
            localTrans.localScale = Vector3.one;
        }       

        public static float ConvertToAngle180(float input)
        {
            while (input > 360)
            {
                input = input - 360;
            }
            while (input < -360)
            {
                input = input + 360;
            }
            if (input > 240) //120
            {
                input = input - 360;
            }
            if (input < -240) //120
                input = 360 + input;
            return input;
        }

        void LimitRot()
        {
            float minRotation = -60;
            float maxRotation = 60;
            Vector3 currentRotation = localTrans.localRotation.eulerAngles;
            currentRotation.x = ConvertToAngle180(currentRotation.x);
            currentRotation.x = Mathf.Clamp(currentRotation.x, minRotation, maxRotation);
            currentRotation.y = ConvertToAngle180(currentRotation.y);
            currentRotation.y = Mathf.Clamp(currentRotation.y, minRotation, maxRotation);
            currentRotation.z = ConvertToAngle180(currentRotation.z);
            currentRotation.z = Mathf.Clamp(currentRotation.z, minRotation, maxRotation);
            localTrans.localRotation = Quaternion.Euler(currentRotation);
            localTrans.parent.localScale = Vector3.one;
        }

    }
}
