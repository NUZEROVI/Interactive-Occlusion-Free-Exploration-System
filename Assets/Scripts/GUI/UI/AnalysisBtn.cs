using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

namespace UnityVolumeRendering
{
    public class AnalysisBtn : MonoBehaviour
    {
        public Button btn;
        public string filePath;
        public string dataName;
        public float eulerX;
        public string description;
        public string survey_txt;
        public string isoRangeFilePath;
        public string tFFilePath;
        public string segTexturePath;
        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;
        public float[] isoRange = new float[20];
        public Vector4[] isoCluster = new Vector4[20];

        private string fileToImport;

        private int dimX;
        private int dimY;
        private int dimZ;
        private int bytesToSkip = 0;
        private DataContentFormat dataFormat = DataContentFormat.Int16;
        private Endianness endianness = Endianness.LittleEndian;

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
            if (btn == "Analysis")
            {
                //filePath = Application.streamingAssetsPath + "/DataFiles/hydrogen.raw"; 

                //Debug.Log(Path.GetExtension(filePath));

                string extension = Path.GetExtension(filePath);


                fileToImport = filePath;

                if(extension == ".raw")
                {
                    if (Path.GetExtension(fileToImport) == ".ini")
                        fileToImport = fileToImport.Replace(".ini", ".raw");

                    // Try parse ini file (if available)
                    DatasetIniData initData = DatasetIniReader.ParseIniFile(fileToImport + ".ini");
                    if (initData != null)
                    {
                        dimX = initData.dimX;
                        dimY = initData.dimY;
                        dimZ = initData.dimZ;

                        bytesToSkip = initData.bytesToSkip;
                        dataFormat = initData.format;
                        endianness = initData.endianness;
                        ImportRawData();
                        GetIsoRangeAndSetTF();
                        GetIsoRangeAndSetTF_Mask();
                    }
                }               
               
                GameObject.Find("Description").GetComponent<Text>().text = description;
                for (int j = 0; j < 2; j++) // Default 4 Cluster
                {
                    GameObject G1 = GameObject.Find("Glock" + (2 * j)).gameObject;
                    GameObject G2 = GameObject.Find("Glock" + (2 * j + 1)).gameObject;
                  
                    G1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + dataName + "/4 Group/" + (2 * j), typeof(Sprite)) as Sprite;
                    G1.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 1).ToString();
                    G2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/" + dataName + "/4 Group/" + (2 * j + 1), typeof(Sprite)) as Sprite;
                    G2.transform.GetChild(1).GetComponent<Text>().text = (2 * j + 2).ToString();
                    
                }
            }
        }
      
        private void ImportRawData()
        {
            RawDatasetImporter importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
            VolumeDataset dataset = importer.Import();
            dataset.datasetName = dataName;
            
            if (dataset != null)
            {

                if(dimX > 500 || dimY > 500 || dimZ > 500)
                {
                    dataset.DownScaleData();
                }
              
                VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                VolumeRenderedObject_Mask obj_Mask = VolumeObjectFactory_Mask.CreateObject(dataset);
                
                if (eulerX == 180.0f)
                {
                    obj.gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);
                    obj_Mask.gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);
                }
                else if(eulerX == 0.0f)
                {
                    obj.gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.right);
                    obj_Mask.gameObject.transform.rotation *= Quaternion.AngleAxis(-90, Vector3.right);
                }

               
            }
            else
            {
                Debug.LogError("Failed to import raw dataset");
            }          
        }

        void GetIsoRangeAndSetTF()
        {         
            VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
            {
                meshRenderer = objects[0].meshRenderer;
                mat = meshRenderer.material;
            }
            //if(isoRangeFilePath != "")
            //{
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
            // }
            
            //if (tFFilePath != "")
            //{
            TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(tFFilePath);
            if (newTF != null)
            {
                objects[0].transferFunction = newTF;
                //objects[0].SetVisibilityWindow(0.58f, 1.0f);
                objects[0].UpdateMaterialProperties();
                // }
            }


        }


        void GetIsoRangeAndSetTF_Mask()
        {
            VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
            if (objects_Mask.Length == 1)
            {
                meshRenderer_Mask = objects_Mask[0].meshRenderer;
                mat_Mask = meshRenderer_Mask.material;
            }
            //if(isoRangeFilePath != "")
            //{
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

            mat_Mask.SetInt("_isoCount", Count);
            mat_Mask.SetFloatArray("_isoRange", isoRange);
            mat_Mask.SetVectorArray("_isoCluster", isoCluster);
            // }

            //if (tFFilePath != "")
            //{
            TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(tFFilePath);
            if (newTF != null)
            {
                objects_Mask[0].transferFunction = newTF;
                //objects[0].SetVisibilityWindow(0.58f, 1.0f);
                objects_Mask[0].UpdateMaterialProperties();
                // }
            }


        }

    }
}