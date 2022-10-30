using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
namespace UnityVolumeRendering
{
    public class Hover : MonoBehaviour
    {
        [Header("Settings")]
        public float RotateSpeed = 5f;
        public float MouseZDist = 10f;

        [Header("Limits Rot")]
        public float maxYRot = 60f;
        public float minYRot = -60f;

        private Transform localTrans, localMaskTrans;

        public Texture2D defaultTexture;
        public Texture2D exitTexture;
        public CursorMode curMode = CursorMode.Auto;
        public Vector2 hotSpot = Vector2.zero;

        private Vector3 ObjectPoint;

        public Matrix4x4[] camToWorld;
        public Vector4[] camPos;

        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;

        public float[] _CircleSize = new float[10];
        public float[] _LensIndexs = new float[10];
        public Vector4[] _WidgetPos = new Vector4[10];
        public Vector4[] _WidgetRecorder = new Vector4[10];
        public int _WidgetNums = 0;
        public int _RecordNums = 0;
        public Matrix4x4[] rotMatrixArr = new Matrix4x4[10];
        public Matrix4x4[] rotMatrixArrInverse = new Matrix4x4[10];

        public bool partialStop = false, fullStop = false;
        private float partialProgrss = 0.0f, fullProgress = 1.0f;
        public float FillSpeed = 1.0f;

        private Vector3 mousePos;
        private Camera myMainCam;

        void Start()
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

            localTrans = GetComponent<Transform>();            
            localMaskTrans = objects_Mask[0].GetComponent<Transform>().GetChild(0).GetComponent<Transform>();
            myMainCam = Camera.main;

            camToWorld = new Matrix4x4[7];
            camPos = new Vector4[7];
            GameObject camZ_Inverse = GameObject.Find("CameraZ_Inverse");
            camPos[0] = camZ_Inverse.transform.position; camToWorld[0] = Matrix4x4.TRS(camZ_Inverse.transform.position, camZ_Inverse.transform.rotation, Vector3.one).inverse;
            camPos[3] = Camera.main.transform.position; camToWorld[3] = Matrix4x4.TRS(Camera.main.transform.position, Camera.main.transform.rotation, Vector3.one).inverse;
            GameObject camX = GameObject.Find("CameraX");
            camPos[1] = camX.transform.position; camToWorld[1] = Matrix4x4.TRS(camX.transform.position, camX.transform.rotation, Vector3.one).inverse;
            GameObject camY = GameObject.Find("CameraY");
            camPos[2] = camY.transform.position; camToWorld[2] = Matrix4x4.TRS(camY.transform.position, camY.transform.rotation, Vector3.one).inverse;
            GameObject camX_Inverse = GameObject.Find("CameraX_Inverse");
            camPos[4] = camX_Inverse.transform.position; camToWorld[4] = Matrix4x4.TRS(camX_Inverse.transform.position, camX_Inverse.transform.rotation, Vector3.one).inverse;
            GameObject camY_Inverse = GameObject.Find("CameraY_Inverse");
            camPos[5] = camY_Inverse.transform.position; camToWorld[5] = Matrix4x4.TRS(camY_Inverse.transform.position, camY_Inverse.transform.rotation, Vector3.one).inverse;
            camPos[6] = camZ_Inverse.transform.position; camToWorld[6] = Matrix4x4.TRS(camZ_Inverse.transform.position, camZ_Inverse.transform.rotation, Vector3.one).inverse;

            Vector3 localVector = objects[0].gameObject.transform.InverseTransformDirection(Vector3.forward);
            int localDepth = -1;
            if (localVector.x != 0) localDepth = 1; // x
            else if (localVector.y != 0) localDepth = 2; // y
            else if (localVector.z != 0) localDepth = 3; // z

            float depthNP = Vector3.Dot(localVector.normalized, Camera.main.WorldToScreenPoint(Vector3.up));

            mat.SetMatrixArray("_CamToWorld", camToWorld);
            mat.SetVectorArray("_camLocalPos", camPos);
            mat.SetFloat("_localDepth", localDepth);
            mat.SetFloat("_depthNP", depthNP);
            mat.SetFloat("_maxDataVal", objects[0].dataset.GetMaxDataValue());
            mat.EnableKeyword("MOUSEDOWN_ON");
            mat.SetFloat("_viewCamIndex", 2);

            mat_Mask.SetMatrixArray("_CamToWorld", camToWorld);
            mat_Mask.SetVectorArray("_camLocalPos", camPos);
            mat_Mask.SetFloat("_localDepth", localDepth);
            mat_Mask.SetFloat("_depthNP", depthNP);
            mat_Mask.SetFloat("_maxDataVal", objects[0].dataset.GetMaxDataValue());
            mat_Mask.EnableKeyword("MOUSEDOWN_ON");
            mat_Mask.SetFloat("_viewCamIndex", 2);

        }
        void Update()
        {

            // For Full Mode
            if (_WidgetNums > 0)
            {
                if (fullProgress <= 0)
                {
                    fullStop = false;
                    fullProgress = 1.0f;
                }

                if (fullStop == true)
                {
                    fullProgress -= FillSpeed * Time.deltaTime;
                    _CircleSize[_WidgetNums - 1] = fullProgress;
                    mat.SetFloatArray("_CircleSize", _CircleSize);
                    mat_Mask.SetFloatArray("_CircleSize", _CircleSize);
                }
            }

           
            if (Input.GetMouseButton(1))
            {                
                float rotX = Input.GetAxis("Mouse X") * RotateSpeed;

                float rotY = Input.GetAxis("Mouse Y") * RotateSpeed;

                Camera camera = Camera.main;

                Vector3 right = Vector3.Cross(camera.transform.up, localTrans.position - camera.transform.position);

                Vector3 up = Vector3.Cross(localTrans.position - camera.transform.position, right);

                localTrans.rotation = Quaternion.AngleAxis(-rotX, up) * localTrans.rotation;

                localTrans.rotation = Quaternion.AngleAxis(rotY, right) * localTrans.rotation;
            
                localTrans.localScale = new Vector3(mat.GetFloat("ObjDepthX"), mat.GetFloat("ObjDepthY"), mat.GetFloat("ObjDepthZ"));
                localMaskTrans.localScale = new Vector3(mat.GetFloat("ObjDepthX"), mat.GetFloat("ObjDepthY"), mat.GetFloat("ObjDepthZ"));
                LimitRot();
                LimitRot_Mask();
            }
            if (Input.GetMouseButtonUp(1))
            {
                localTrans.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                localTrans.parent.localScale = new Vector3(mat.GetFloat("ObjDepthX"), mat.GetFloat("ObjDepthY"), mat.GetFloat("ObjDepthZ"));
                localTrans.localScale = Vector3.one;

                localMaskTrans.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                localMaskTrans.parent.localScale = new Vector3(mat_Mask.GetFloat("ObjDepthX"), mat_Mask.GetFloat("ObjDepthY"), mat_Mask.GetFloat("ObjDepthZ")) * 0.7f;                
                localMaskTrans.localScale = Vector3.one;
            }
    
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

        void LimitRot_Mask()
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

            localMaskTrans.localRotation = Quaternion.Euler(currentRotation);
            localMaskTrans.parent.localScale = Vector3.one * 0.7f;
        }


        void OnMouseDown()
        {
            if (_WidgetNums < 10)
            {
                // Widget on
                if (GameObject.Find("Model Toggle Group").transform.GetChild(0).GetComponent<Toggle>().isOn)
                {
                    PartialOn();
                }
                else // structure on
                {
                    FullOn();
                }
            }
        }

        void PartialOn()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ObjectPoint = hit.point;

                ObjectPoint.z = transform.localPosition.z - (transform.localScale.z / 2);//this.transform.localScale.z / 2;              
                ObjectPoint.x = float.Parse(ObjectPoint.x.ToString("F1"));
                ObjectPoint.y = float.Parse(ObjectPoint.y.ToString("F1"));

                Vector4 pos = new Vector4(ObjectPoint.x, ObjectPoint.y, ObjectPoint.z, 0);

                VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
                Vector3 localVector = objects[0].gameObject.transform.InverseTransformDirection(Vector3.forward);

                int localDepth = -1;
                if (localVector.x != 0) localDepth = 1; // x
                else if (localVector.y != 0) localDepth = 2; // y
                else if (localVector.z != 0) localDepth = 3; // z
                float depthNP = Vector3.Dot(localVector.normalized, Camera.main.WorldToScreenPoint(Vector3.up));

                pos.w = localDepth;
                SliderEvent sliderObj = FindObjectOfType<SliderEvent>();
                if (_WidgetNums == 0) //init
                {
                    _WidgetPos[_WidgetNums] = pos;
                    _WidgetRecorder[_RecordNums] = new Vector4(localDepth, _WidgetNums, 0, 0);
                    if (depthNP > 0) _WidgetRecorder[_RecordNums].z++;
                    if (depthNP < 0) _WidgetRecorder[_RecordNums].w++;
                    rotMatrixArr[_WidgetNums].SetTRS(localTrans.parent.position, localTrans.parent.rotation, localTrans.parent.localScale);
                    rotMatrixArrInverse[_WidgetNums] = rotMatrixArr[_WidgetNums].inverse;

                    _CircleSize[_WidgetNums] = 0.1f;
                    _LensIndexs[_WidgetNums] = 0.1f;

                    sliderObj.sliNumOn = _WidgetNums;
                    sliderObj.GetComponent<Slider>().value = 0.1f; // Defalu value

                    _WidgetNums++; _RecordNums++;
                }
                else
                {
                    bool isNew = true;
                    for (int i = 0; i < _WidgetNums; i++)
                    {
                        if (_WidgetPos[i].w == pos.w && _WidgetPos[i].x == pos.x && _WidgetPos[i].y == pos.y)
                        {
                            if (depthNP > 0) _WidgetRecorder[i].z++;
                            if (depthNP < 0) _WidgetRecorder[i].w++;
                            isNew = false;
                        }
                    }

                    if (isNew)
                    {
                        _WidgetPos[_WidgetNums] = pos;
                        _WidgetRecorder[_RecordNums] = new Vector4(localDepth, _WidgetNums, 0, 0);
                        if (depthNP > 0) _WidgetRecorder[_RecordNums].z++;
                        if (depthNP < 0) _WidgetRecorder[_RecordNums].w++;
                        rotMatrixArr[_WidgetNums].SetTRS(localTrans.parent.position, localTrans.parent.rotation, localTrans.parent.localScale);
                        rotMatrixArrInverse[_WidgetNums] = rotMatrixArr[_WidgetNums].inverse;

                        _CircleSize[_WidgetNums] = 0.1f;
                        _LensIndexs[_WidgetNums] = 0.1f;

                        sliderObj.sliNumOn = _WidgetNums;
                        sliderObj.GetComponent<Slider>().value = 0.1f; // Defalu value

                        _WidgetNums++; _RecordNums++;
                    }
                }



                sliderObj.widgetNums = _WidgetNums;


                //mat.SetFloatArray("_CircleSize", _CircleSize);

                SetColor(_WidgetNums);
               
                GameObject.Find("CircleNum" + (_WidgetNums - 1)).GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                GameObject.Find("CircleNum" + (_WidgetNums - 1)).transform.GetChild(0).GetComponent<Text>().color = new Color32(245, 36, 18, 180);
                GameObject.Find("LensShape0").GetComponent<Image>().color = new Color32(245, 36, 18, 180);



                mat.SetFloatArray("_CircleSize", _CircleSize);         
                mat.SetFloatArray("_LensIndexs", _LensIndexs);
                mat.SetInt("_WidgetNums", _WidgetNums);
                mat.SetInt("_RecordNums", _RecordNums);
                mat.SetVectorArray("_WidgetPos", _WidgetPos);
                mat.SetVectorArray("_WidgetRecorder", _WidgetRecorder);
                mat.SetMatrixArray("_RotateMatrix", rotMatrixArr);
                mat.SetMatrixArray("_RotateMatrixInverse", rotMatrixArrInverse);
                mat.SetInt("_CurrentWidgetNum", _WidgetNums - 1);

                mat_Mask.SetFloatArray("_CircleSize", _CircleSize);
                mat_Mask.SetFloatArray("_LensIndexs", _LensIndexs);
                mat_Mask.SetInt("_WidgetNums", _WidgetNums);
                mat_Mask.SetInt("_RecordNums", _RecordNums);
                mat_Mask.SetVectorArray("_WidgetPos", _WidgetPos);
                mat_Mask.SetVectorArray("_WidgetRecorder", _WidgetRecorder);
                mat_Mask.SetMatrixArray("_RotateMatrix", rotMatrixArr);
                mat_Mask.SetMatrixArray("_RotateMatrixInverse", rotMatrixArrInverse);
                mat_Mask.SetInt("_CurrentWidgetNum", _WidgetNums - 1);

                //partialStop = true;
                //_CircleSize[_WidgetNums - 1] = 0.1f;
                //mat.SetFloatArray("_CircleSize", _CircleSize);
            }
        }

        void FullOn()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ObjectPoint = hit.point;

                ObjectPoint.z = transform.localPosition.z - (transform.localScale.z / 2);//this.transform.localScale.z / 2;              

                Vector4 pos = new Vector4(ObjectPoint.x, ObjectPoint.y, ObjectPoint.z, 0);

                VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
                Vector3 localVector = objects[0].gameObject.transform.InverseTransformDirection(Vector3.forward);

                int localDepth = -1;
                if (localVector.x != 0) localDepth = 1; // x
                else if (localVector.y != 0) localDepth = 2; // y
                else if (localVector.z != 0) localDepth = 3; // z
                float depthNP = Vector3.Dot(localVector.normalized, Camera.main.WorldToScreenPoint(Vector3.up));

                pos.w = localDepth;
                SliderEvent sliderObj = FindObjectOfType<SliderEvent>();
                if (_WidgetNums == 0) //init
                {
                    _WidgetPos[_WidgetNums] = pos;
                    _WidgetRecorder[_RecordNums] = new Vector4(localDepth, _WidgetNums, 0, 0);
                    if (depthNP > 0) _WidgetRecorder[_RecordNums].z++;
                    if (depthNP < 0) _WidgetRecorder[_RecordNums].w++;
                    rotMatrixArr[_WidgetNums].SetTRS(localTrans.parent.position, localTrans.parent.rotation, localTrans.parent.localScale);
                    rotMatrixArrInverse[_WidgetNums] = rotMatrixArr[_WidgetNums].inverse;

                    _CircleSize[_WidgetNums] = 0.1f;
                    _LensIndexs[_WidgetNums] = 0.1f;                   

                    sliderObj.sliNumOn = _WidgetNums;
                    sliderObj.GetComponent<Slider>().value = 0.1f; // Defalu value

                    _WidgetNums++; _RecordNums++;
                }
                else
                {
                    bool isNew = true;
                    for (int i = 0; i < _WidgetNums; i++)
                    {
                        if (_WidgetPos[i].w == pos.w && _WidgetPos[i].x == pos.x && _WidgetPos[i].y == pos.y)
                        {
                            if (depthNP > 0) _WidgetRecorder[i].z++;
                            if (depthNP < 0) _WidgetRecorder[i].w++;
                            isNew = false;
                        }
                    }

                    if (isNew)
                    {
                        _WidgetPos[_WidgetNums] = pos;
                        _WidgetRecorder[_RecordNums] = new Vector4(localDepth, _WidgetNums, 0, 0);
                        if (depthNP > 0) _WidgetRecorder[_RecordNums].z++;
                        if (depthNP < 0) _WidgetRecorder[_RecordNums].w++;
                        rotMatrixArr[_WidgetNums].SetTRS(localTrans.parent.position, localTrans.parent.rotation, localTrans.parent.localScale);
                        rotMatrixArrInverse[_WidgetNums] = rotMatrixArr[_WidgetNums].inverse;

                        _CircleSize[_WidgetNums] = 0.1f;
                        _LensIndexs[_WidgetNums] = 0.1f;
                       
                        sliderObj.sliNumOn = _WidgetNums;
                        sliderObj.GetComponent<Slider>().value = 0.1f; // Defalu value

                        _WidgetNums++; _RecordNums++;
                    }
                }



                sliderObj.widgetNums = _WidgetNums;


                //mat.SetFloatArray("_CircleSize", _CircleSize);

                SetColor(_WidgetNums);
                GameObject.Find("CircleNum" + (_WidgetNums - 1)).GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                GameObject.Find("CircleNum" + (_WidgetNums - 1)).transform.GetChild(0).GetComponent<Text>().color = new Color32(245, 36, 18, 180);
                GameObject.Find("LensShape0").GetComponent<Image>().color = new Color32(245, 36, 18, 180);

                //mat.SetFloatArray("_CircleSize", _CircleSize);
                mat.SetInt("_WidgetNums", _WidgetNums);
                mat.SetInt("_RecordNums", _RecordNums);
                mat.SetVectorArray("_WidgetPos", _WidgetPos);
                mat.SetVectorArray("_WidgetRecorder", _WidgetRecorder);
                mat.SetMatrixArray("_RotateMatrix", rotMatrixArr);
                mat.SetMatrixArray("_RotateMatrixInverse", rotMatrixArrInverse);
                mat.SetInt("_CurrentWidgetNum", _WidgetNums - 1);

                mat_Mask.SetInt("_WidgetNums", _WidgetNums);
                mat_Mask.SetInt("_RecordNums", _RecordNums);
                mat_Mask.SetVectorArray("_WidgetPos", _WidgetPos);
                mat_Mask.SetVectorArray("_WidgetRecorder", _WidgetRecorder);
                mat_Mask.SetMatrixArray("_RotateMatrix", rotMatrixArr);
                mat_Mask.SetMatrixArray("_RotateMatrixInverse", rotMatrixArrInverse);
                mat_Mask.SetInt("_CurrentWidgetNum", _WidgetNums - 1);


                fullStop = true;            
                _CircleSize[_WidgetNums - 1] = 0.0f;
                mat.SetFloatArray("_CircleSize", _CircleSize);
                mat.SetFloatArray("_LensIndexs", _LensIndexs);

                mat_Mask.SetFloatArray("_CircleSize", _CircleSize);
                mat_Mask.SetFloatArray("_LensIndexs", _LensIndexs);
            }
        }
      
        public void SetColor(int num)
        {

            for (int i = 0; i < num; i++)
            {
                GameObject.Find("CircleNum" + i).GetComponent<Image>().color = new Color32(56, 126, 184, 75);
                GameObject.Find("CircleNum" + i).transform.GetChild(0).GetComponent<Text>().color = new Color32(56, 126, 184, 255);
                GameObject.Find("CircleNum" + i).GetComponent<Button>().enabled = true;
            }

            for (int i = 0; i < 3; i++)
            {
                GameObject.Find("LensShape" + i).GetComponent<Image>().color  = new Color32(111, 124, 138, 30);
                // GameObject.Find("LensShape" + i).GetComponent<Button>().enabled = true;
            }

        }


        private void OnMouseEnter()
        {
            Cursor.SetCursor(defaultTexture, hotSpot, curMode);
        }


    }
}
