using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WriteToCSVFile :MonoBehaviour
{
    void Start()
    {
        // name, filePath, tfPath, isoRangeTxtPath, img, date
        addDataRecord("Hydrogen", "/DataFiles/Hydrogen.raw", "/Recall_tf/Rainbow.tf", "/Recall_Iso/isoRange.txt", "Sprites/RemoveBG/Hydrogen", "25.04.2022 - 14:00", "128 X 128 X 128");
        addDataRecord("Head", "/DataFiles//Head.raw", "/Recall_tf/Head8_Final.tf", "/Recall_Iso/head_isoRange.txt", "Sprites/RemoveBG/Head", "25.04.2022 - 18:00", "512 X 512 X 512");
        addDataRecord("C60", "/DataFiles/C60.raw", "/Recall_tf/C604_Final.tf", "/Recall_Iso/c60_isoRange_4.txt", "Sprites/RemoveBG/Orange", "25.04.2022 - 19:00", "64 X 64 X 64");
        addDataRecord("Orange", "/DataFiles/Orange.raw", "/Recall_tf/Para_rainbow_orange_inverse.tf", "/Recall_Iso/orange_isoRange.txt", "Sprites/RemoveBG/Orange", "25.04.2022 - 19:00", "256 X 256 X 24");
        addDataRecord("Foot", "/DataFiles/Foot.raw", "/Recall_tf/Para_rainbow_orange.tf", "/Recall_Iso/foot_isoRange.txt", "Sprites/RemoveBG/Foot", "25.04.2022 - 19:00", "143 X 256 X 183");      
        addDataRecord("lsabel(pf21)", "/DataFiles/lsabel (pf21).bin", "/Recall_tf/Para_rainbow_orange.tf", "/Recall_Iso/lsabelpf21_isoRange_4.txt", "Sprites/RemoveBG/lsabel", "25.04.2022 - 20:00", "500 X 500 X 100");
        //ReadCSVFile("1", "dataset.csv", 1);
        //"C:/Users/user/Desktop/UnityVolumeRendering(0409GUI_Lab)/UnityVolumeRendering(0409)/"
        //readRecord("dataset.csv", "hydrogen", 1);
    }


    public static bool recordMatches(string findObj, string[] record, int posfindNum)
    {
        if (record[posfindNum].Equals(findObj))
        {
            return true;
        }
        return false;
    }

    

    public static void addDataRecord(string name, string data_path, string tf_path, string iso_path, string img_path, string up_date, string descibe)
    {

        bool isMatch = false;       
        string csvPath = Application.streamingAssetsPath + "/dataset.csv";

        if (File.Exists(csvPath))
        {
            string[] lines = File.ReadAllLines(csvPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(',');
                if (recordMatches(name, fields, 0))
                {
                    isMatch = true;
                }
            }
           // Debug.Log(isMatch);
        }
       
        if (isMatch == false)
        {
            StreamWriter file = new StreamWriter(@csvPath, true);
            file.WriteLine(name + "," + data_path + "," + tf_path + "," + iso_path + "," + img_path + "," + up_date + "," + descibe);
            file.Close();
        }       
        
    }

    //public static bool readRecord(string csvPath, string findObj, int posfindNum)
    //{
    //    posfindNum--;

    //    string[] lines = File.ReadAllLines(csvPath);
    //    for (int i = 0; i < lines.Length; i++)
    //    {
    //        string[] fields = lines[i].Split(',');
    //        if (recordMatches(findObj, fields, posfindNum))
    //        {
    //            //Debug.Log("Record found");               
    //            return true;
    //        }
    //    }
    //    //Debug.Log("not found");
    //    return false;
    //}


    //void ReadCSVFile(string searchNum, string filepath, int u)
    //{
    //    StreamReader str = new StreamReader(filepath);
    //    bool endOfFile = false;
    //    while (!endOfFile)
    //    {
    //        string data_string = str.ReadLine();
    //        if(data_string == null)
    //        {
    //            endOfFile = true;
    //            break;
    //        }

    //        var vals = data_string.Split (',');
    //        //for(int i=0; i<vals.Length; i++)
    //        //{
    //        //    Debug.Log("vals: " + i.ToString() + " " + vals[0].ToString());
    //        //}
    //        Debug.Log(vals[0].ToString() + " " + vals[1].ToString());
    //    }
    //}
}
