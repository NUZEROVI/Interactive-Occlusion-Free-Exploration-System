using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace UnityVolumeRendering
{
    public class ItemsListCreate : MonoBehaviour
    {
       
        void Start()
        {
            string readFromFilePath = Application.streamingAssetsPath + "/dataset.csv";
           

            if (File.Exists(readFromFilePath))
            {
                string[] lines = File.ReadAllLines(readFromFilePath);
                //Debug.Log(lines.Length);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    GameObject objs = GameObject.Instantiate((GameObject)Resources.Load("Example Mission"));
                    objs.transform.parent = this.transform;
                    Vector3 oriPos = objs.transform.GetComponent<RectTransform>().localPosition;
                    objs.transform.GetComponent<RectTransform>().localPosition = new Vector3(oriPos.x, oriPos.y, 0);                 
                    objs.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load(fields[5], typeof(Sprite)) as Sprite; ;
                    objs.transform.GetChild(2).GetComponent<Text>().text = fields[0]; // Data Name
                    objs.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = fields[6];
                    objs.transform.GetChild(4).GetComponent<Text>().text = fields[7] + "\n" + "Data Range [" + fields[9] + " , " + fields[10] + "]"; // Data Description
                    objs.transform.GetChild(5).GetComponent<Text>().text = fields[12]; // Data Description Survey
                    objs.name = "Datasets - " + i;

                    if (i == 0)
                    {                      
                        GameObject.Find("Data Pic").GetComponent<Image>().sprite = Resources.Load(fields[5], typeof(Sprite)) as Sprite;
                        GameObject.Find("Data Title").GetComponent<Text>().text = fields[0];                 
                        GameObject.Find("Data Info").GetComponent<Text>().text = fields[7] + "\n" + "Data Range [" + fields[9] + " , " + fields[10] + "]";
                        GameObject.Find("Data survey").GetComponent<Text>().text = fields[12];
                        AnalysisBtn[] analysisBtn = FindObjectsOfType<AnalysisBtn>();
                        analysisBtn[0].filePath = Application.streamingAssetsPath + fields[1]; // data file (raw, bin...)
                        analysisBtn[0].dataName = fields[8];
                        analysisBtn[0].description = fields[0] + "\n" + fields[7] + "\n" + "Data Range [" + fields[9] + " , " + fields[10] + "]";
                        analysisBtn[0].survey_txt = fields[12];
                        analysisBtn[0].isoRangeFilePath = Application.streamingAssetsPath + fields[3]; // iso Txt file
                        analysisBtn[0].tFFilePath = Application.streamingAssetsPath + fields[2]; // TF file
                        analysisBtn[0].segTexturePath = Application.streamingAssetsPath + fields[4]; // Seg Texture
                        objs.GetComponent<Image>().color = objs.GetComponent<Button>().colors.pressedColor;
                        
                    }

                 
                }
     

            }
        }
           
    }
}