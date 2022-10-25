using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class ClusterItemsListCreate : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //string readFromFilePath = Application.streamingAssetsPath + "/dataset.csv";

            //for (int i = 0; i < 2; i++) // Default 4 Cluster
            //{
            //    GameObject twoClusters = GameObject.Find("Header" + i).gameObject;
            //    GameObject G1 = GameObject.Instantiate((GameObject)Resources.Load("Glock (1)"));
            //    G1.transform.SetParent(twoClusters.transform, false);
            //    G1.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/HydronAtom/4 Group/" + (i * (i + 1)) , typeof(Sprite)) as Sprite; ;

            //    GameObject G2 = GameObject.Instantiate((GameObject)Resources.Load("Glock (2)"));
            //    G2.transform.SetParent(twoClusters.transform, false);
            //    G2.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Sprites/Clusters/HydronAtom/4 Group/" + (i * (i + 1) + 1), typeof(Sprite)) as Sprite; ;
            //}
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}