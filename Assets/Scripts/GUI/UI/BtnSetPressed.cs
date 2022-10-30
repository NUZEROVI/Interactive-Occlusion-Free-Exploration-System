using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class BtnSetPressed : MonoBehaviour
    {
        private Button btn;
        private MeshRenderer meshRenderer, meshRenderer_Mask;
        private Material mat, mat_Mask;
        private VolumeRenderedObject[] objects;
        private VolumeRenderedObject_Mask[] objects_Mask;
        void Start()
        {
            btn = this.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => BtnPressed(btn.name));
            }
           
        }
        public void BtnPressed(string name)
        {
           
            if(name == "ExitBtn")
            {
                #if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
            }

            if(name == "ResetBtn")
            {
                objects = FindObjectsOfType<VolumeRenderedObject>();
                objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
                if (objects_Mask.Length == 1)
                {
                    meshRenderer_Mask = objects_Mask[0].meshRenderer;
                    mat_Mask = meshRenderer_Mask.material;
                }

                if (objects.Length == 1)
                {
                    meshRenderer = objects[0].meshRenderer;
                    mat = meshRenderer.material;

                    Hover[] obj = FindObjectsOfType<Hover>();
                    if (obj.Length == 1)
                    {
                        obj[0]._CircleSize = new float[10];
                        obj[0]._LensIndexs = new float[10];
                        obj[0]._WidgetPos = new Vector4[10];
                        obj[0]._WidgetRecorder = new Vector4[10];
                        obj[0]._WidgetNums = 0;
                        obj[0]._RecordNums = 0;
                        obj[0].rotMatrixArr = new Matrix4x4[10];
                        obj[0].rotMatrixArrInverse = new Matrix4x4[10];
                        mat.SetInt("_WidgetNums", obj[0]._WidgetNums);
                        mat.SetInt("_RecordNums", obj[0]._RecordNums);
                        mat.SetVectorArray("_WidgetPos", obj[0]._WidgetPos);
                        mat.SetVectorArray("_WidgetRecorder", obj[0]._WidgetRecorder);
                        mat.SetMatrixArray("_RotateMatrix", obj[0].rotMatrixArr);
                        mat.SetMatrixArray("_RotateMatrixInverse", obj[0].rotMatrixArrInverse);
                        mat.SetInt("_CurrentWidgetNum", 0);
                        mat.SetFloatArray("_CircleSize", obj[0]._CircleSize);
                        mat.SetFloatArray("_LensIndexs", obj[0]._LensIndexs);
                                                                  

                        mat_Mask.SetInt("_WidgetNums", obj[0]._WidgetNums);
                        mat_Mask.SetInt("_RecordNums", obj[0]._RecordNums);
                        mat_Mask.SetVectorArray("_WidgetPos", obj[0]._WidgetPos);
                        mat_Mask.SetVectorArray("_WidgetRecorder", obj[0]._WidgetRecorder);
                        mat_Mask.SetMatrixArray("_RotateMatrix", obj[0].rotMatrixArr);
                        mat_Mask.SetMatrixArray("_RotateMatrixInverse", obj[0].rotMatrixArrInverse);
                        mat_Mask.SetInt("_CurrentWidgetNum", 0);
                        mat_Mask.SetFloatArray("_CircleSize", obj[0]._CircleSize);
                        mat_Mask.SetFloatArray("_LensIndexs", obj[0]._LensIndexs);
                        obj[0].SetColor(0);
                        int nums = GameObject.Find("SetSizeBtn Group").transform.childCount;
                        for (int i = 0; i < nums; i++)
                        {
                            GameObject.Find("CircleNum" + i).GetComponent<Image>().sprite = Resources.Load("Sprites/Borders/Basic/Basic Outline 10px - Stroke 4px", typeof(Sprite)) as Sprite;
                            GameObject.Find("CircleNum" + i).transform.GetChild(0).GetComponent<Text>().color = new Color32(111, 124, 138, 60);//new Color32(56, 126, 184, 255);
                            GameObject.Find("CircleNum" + i).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                            GameObject.Find("CircleNum" + i).GetComponent<Button>().enabled = false;
                        }
                    }

                    
                }
              
            }          
        }
    }
}