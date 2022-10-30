using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

namespace UnityVolumeRendering
{
    public class ToggleEvent : MonoBehaviour
    {
        Toggle m_Toggle;
        GameObject toggleGroupObj;

        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;

        [HideInInspector]
        public TransferFunction transferFunction;

        [HideInInspector]
        public TransferFunction2D transferFunction2D;

        VolumeRenderedObject[] objects;
        VolumeRenderedObject_Mask[] objects_Mask;
        public float[] isoRange = new float[20];
        public Vector4[] isoCluster = new Vector4[20];

        void Start()
        {
            m_Toggle = GetComponent<Toggle>();
            toggleGroupObj = m_Toggle.transform.parent.gameObject;

            if (toggleGroupObj.name == "Cluster Toggle Group") // 4, 6, 8
            {
                toggleGroupObj.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            else if (toggleGroupObj.name == "Model Toggle Group") // Widgets, Structures
            {
                toggleGroupObj.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            else if (toggleGroupObj.name == "Light Toggle Group") // On, Off
            {
                toggleGroupObj.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            else if (toggleGroupObj.name == "Surface Toggle Group") // Full, Partial
            {
                toggleGroupObj.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            
            objects = FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
            {
                meshRenderer = objects[0].meshRenderer;
                mat = meshRenderer.material;
            }

            objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
            if (objects_Mask.Length == 1)
            {
                meshRenderer_Mask = objects_Mask[0].meshRenderer;
                mat_Mask = meshRenderer_Mask.material;
            }

            matSetting();

            m_Toggle.onValueChanged.AddListener(delegate
            {
                matSetting();
            });
           
        }

        void matSetting()
        {

            for (int i = 0; i < toggleGroupObj.transform.childCount; i++)
            {
                if (toggleGroupObj.transform.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    switch (toggleGroupObj.name)
                    {
                        case "Cluster Toggle Group":
                            objects = FindObjectsOfType<VolumeRenderedObject>();
                            string tfPath = "", isoRangeFilePath = "";

                            //
                            if (i == 0) // 4 Clusters
                            {                                                             
                                for (int j = 0; j < 2 + i; j++) // Default 4 Cluster
                                {
                                    GameObject G1 = GameObject.Find("Glock" + (2 * j)).gameObject;
                                    GameObject G2 = GameObject.Find("Glock" + (2 * j + 1)).gameObject;
                                    if (objects.Length == 1)
                                    {
                                        G1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/4 Group/" + (2 * j), typeof(Sprite)) as Sprite;
                                        G1.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 1).ToString();
                                        G2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/4 Group/" + (2 * j + 1), typeof(Sprite)) as Sprite;
                                        G2.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 2).ToString();
                                    }
                                }

                                for (int j = 2; j < 4 + i; j++) // Default 4 Cluster
                                {
                                    GameObject NG1 = GameObject.Find("Glock" + (2 * j)).gameObject;
                                    GameObject NG2 = GameObject.Find("Glock" + (2 * j + 1)).gameObject;
                                    NG1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/bk", typeof(Sprite)) as Sprite;
                                    NG1.transform.GetChild(1).GetComponent<Text>().text = "";
                                    NG2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/bk", typeof(Sprite)) as Sprite;
                                    NG2.transform.GetChild(1).GetComponent<Text>().text = "";
                                }
                                if (objects.Length == 1)
                                {
                                    tfPath = Application.streamingAssetsPath + "/Recall_tf/" + objects[0].dataset.datasetName + "_4.tf";
                                    isoRangeFilePath = Application.streamingAssetsPath + "/Recall_Iso/" + objects[0].dataset.datasetName + "_isoRange_4.txt";
                                    AnalysisBtn.FindObjectsOfType<AnalysisBtn>()[0].segTexturePath = Application.streamingAssetsPath + "/IsoSegCluster/" + objects[0].dataset.datasetName + "_segClusters_4.bin";
                                }
                            }
                            if (i == 1) // 6
                            {                                                               
                                for (int j = 0; j < (2 + i); j++) // Default 4 Cluster
                                {
                                    GameObject G1 = GameObject.Find("Glock" + (2 * j)).gameObject;
                                    GameObject G2 = GameObject.Find("Glock" + (2 * j + 1)).gameObject;
                                    if (objects.Length == 1)
                                    {
                                        G1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/6 Group/" + (2 * j), typeof(Sprite)) as Sprite;
                                        G1.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 1).ToString();
                                        G2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/6 Group/" + (2 * j + 1), typeof(Sprite)) as Sprite;
                                        G2.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 2).ToString();
                                    }
                                }
                              
                                GameObject NG1 = GameObject.Find("Glock" + (2 * 3)).gameObject;
                                GameObject NG2 = GameObject.Find("Glock" + (2 * 3 + 1)).gameObject;
                                NG1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/bk", typeof(Sprite)) as Sprite;
                                NG1.transform.GetChild(1).GetComponent<Text>().text = "";
                                NG2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/bk", typeof(Sprite)) as Sprite;
                                NG2.transform.GetChild(1).GetComponent<Text>().text = "";

                                if (objects.Length == 1)
                                {
                                    tfPath = Application.streamingAssetsPath + "/Recall_tf/" + objects[0].dataset.datasetName + "_6.tf";
                                    isoRangeFilePath = Application.streamingAssetsPath + "/Recall_Iso/" + objects[0].dataset.datasetName + "_isoRange_6.txt";
                                    AnalysisBtn.FindObjectsOfType<AnalysisBtn>()[0].segTexturePath = Application.streamingAssetsPath + "/IsoSegCluster/" + objects[0].dataset.datasetName + "_segClusters_6.bin"; 
                                }
                            }
                            if (i == 2) // 8
                            {                                                               
                                for (int j = 0; j < (2 + i); j++) // Default 4 Cluster
                                {
                                    GameObject G1 = GameObject.Find("Glock" + (2 * j)).gameObject;
                                    GameObject G2 = GameObject.Find("Glock" + (2 * j + 1)).gameObject;
                                    if (objects.Length == 1)
                                    {
                                        G1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/8 Group/" + (2 * j), typeof(Sprite)) as Sprite;
                                        G1.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 1).ToString();
                                        G2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + objects[0].dataset.datasetName + "/8 Group/" + (2 * j + 1), typeof(Sprite)) as Sprite;
                                        G2.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 2).ToString();
                                    }
                                }
                                if (objects.Length == 1)
                                {
                                    tfPath = Application.streamingAssetsPath + "/Recall_tf/" + objects[0].dataset.datasetName + "_8.tf";
                                    isoRangeFilePath = Application.streamingAssetsPath + "/Recall_Iso/" + objects[0].dataset.datasetName + "_isoRange_8.txt";
                                    AnalysisBtn.FindObjectsOfType<AnalysisBtn>()[0].segTexturePath = Application.streamingAssetsPath + "/IsoSegCluster/" + objects[0].dataset.datasetName + "_segClusters_8.bin";
                                }
                            }

                            if (objects.Length == 1)
                            {
                                meshRenderer = objects[0].meshRenderer;
                                mat = meshRenderer.material;

                                VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
                                if (objects_Mask.Length == 1)
                                {
                                    meshRenderer_Mask = objects_Mask[0].meshRenderer;
                                    mat_Mask = meshRenderer_Mask.material;
                                }

                                List<string> fileLines = File.ReadAllLines(isoRangeFilePath).ToList();

                                int range = 0; //dataset.GetMinDataValue();
                                int Count = 0;
                                foreach (string line in fileLines)
                                {
                                    range += int.Parse(line);

                                    isoRange[Count] = range;

                                    if (Count == 0)
                                    {
                                        isoCluster[Count] = new Vector4(0, isoRange[Count] - 1);
                                    }
                                    else
                                    {
                                        isoCluster[Count] = new Vector4(isoRange[Count - 1], isoRange[Count] - 1);
                                    }
                                    Count++;
                                }

                                mat.SetInt("_isoCount", Count);
                                mat.SetFloatArray("_isoRange", isoRange);
                                mat.SetVectorArray("_isoCluster", isoCluster);

                                mat_Mask.SetInt("_isoCount", Count);
                                mat_Mask.SetFloatArray("_isoRange", isoRange);
                                mat_Mask.SetVectorArray("_isoCluster", isoCluster);


                                TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(tfPath);
                                if (newTF != null)
                                    objects[0].transferFunction = newTF;
                                    objects_Mask[0].transferFunction = newTF;
                                objects[0].UpdateMaterialProperties();
                                objects_Mask[0].UpdateMaterialProperties();
                                resetDefault();
                            }

                            break;
                        case "Model Toggle Group":                                                       
                            if (i == 0)
                            {
                                resetDefault();
                                mat.EnableKeyword("DiggingWidget"); 
                                mat.DisableKeyword("ErasingWidget");
                                mat_Mask.EnableKeyword("DiggingWidget");
                                mat_Mask.DisableKeyword("ErasingWidget");
                                SliderEvent[] sliObj = FindObjectsOfType<SliderEvent>();
                                sliObj[0].sliname = "SLICircleSize";
                            }
                            if (i == 1)
                            {
                                resetDefault();
                                mat.EnableKeyword("ErasingWidget");
                                mat.DisableKeyword("DiggingWidget");
                                mat_Mask.EnableKeyword("ErasingWidget");
                                mat_Mask.DisableKeyword("DiggingWidget");
                                SliderEvent[] sliObj = FindObjectsOfType<SliderEvent>();
                                sliObj[0].sliname = "SLISurface";
                            }
                         
                            break;
                        case "Light Toggle Group":
                            objects = FindObjectsOfType<VolumeRenderedObject>();
                            if (objects.Length == 1)
                            {
                                if (i == 0) objects[0].SetLightingEnabled(true);                             
                                if (i == 1) objects[0].SetLightingEnabled(false);
                            }
                            objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
                            if (objects_Mask.Length == 1)
                            {
                                if (i == 0) objects_Mask[0].SetLightingEnabled(true);
                                if (i == 1) objects_Mask[0].SetLightingEnabled(false);
                            }
                            break;
                        case "Surface Toggle Group":
                            if (i == 0)
                            {
                                mat.EnableKeyword("ErasingWidget");
                                mat.DisableKeyword("DiggingWidget");
                                mat_Mask.EnableKeyword("ErasingWidget");
                                mat_Mask.DisableKeyword("DiggingWidget");
                            }
                            if (i == 1)
                            {
                                mat.EnableKeyword("DiggingWidget");
                                mat.DisableKeyword("ErasingWidget");
                                mat_Mask.EnableKeyword("DiggingWidget");
                                mat_Mask.DisableKeyword("ErasingWidget");
                            }
                            break;
                    }
                }                              
            }         
           
        }


        void resetDefault()
        {
            Hover[] obj = FindObjectsOfType<Hover>();
            if(obj.Length == 1)
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