using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class ListEvent : MonoBehaviour
    {
        public Button btn;
        public Text nameTxt, describeTxt, surveyTxt;


        private void Start()
        {
            btn = this.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(SetText);
            }
        }

        public void SetText()
        {
            nameTxt = this.transform.Find("Data Name").GetComponent<Text>();
            describeTxt = this.transform.Find("Data Description").GetComponent<Text>();
            surveyTxt = this.transform.Find("Data Description Survey").GetComponent<Text>();
            Sprite dataImg = this.transform.Find("Data Img").GetComponent<Image>().sprite;
            GameObject.Find("Data Title").GetComponent<Text>().text = nameTxt.text;
            GameObject.Find("Data Info").GetComponent<Text>().text = describeTxt.text;
            GameObject.Find("Data survey").GetComponent<Text>().text = surveyTxt.text;
            GameObject.Find("Data Pic").GetComponent<Image>().sprite = dataImg; // Resources.Load(dataImg, typeof(Sprite)) as Sprite;

            string csvPath = Application.streamingAssetsPath + "/dataset.csv";

            if (File.Exists(csvPath))
            {
                string[] lines = File.ReadAllLines(csvPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    if (recordMatches(nameTxt.text, fields, 0))
                    {
                        AnalysisBtn[] analysisBtn = FindObjectsOfType<AnalysisBtn>();
                        analysisBtn[0].filePath = Application.streamingAssetsPath + fields[1]; // data file (raw, bin...)
                        analysisBtn[0].dataName = fields[8];
                        analysisBtn[0].eulerX = float.Parse(fields[11]);
                        analysisBtn[0].description = fields[0] + "\n" + fields[7] + "\n" + "Data Range [" + fields[9] + " , " + fields[10] + "]";
                        analysisBtn[0].survey_txt = fields[12];
                        analysisBtn[0].isoRangeFilePath = Application.streamingAssetsPath + fields[3]; // iso Txt file
                        analysisBtn[0].tFFilePath = Application.streamingAssetsPath + fields[2]; // TF file     
                        analysisBtn[0].segTexturePath = Application.streamingAssetsPath + fields[4]; // Seg Texture
                    }
                }                
            }

          
        }
        public static bool recordMatches(string findObj, string[] record, int posfindNum)
        {
            if (record[posfindNum].Equals(findObj))
            {
                return true;
            }
            return false;
        }      

    }
}