using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class BtnSetSizePressedColor : MonoBehaviour
    {
        private Button btn;

        MeshRenderer meshRenderer;
        Material mat;

        void Start()
        {
            btn = this.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SetColor(btn.name));
            }
        }
      
        public void SetColor(string name)
        {


            VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
            {
                meshRenderer = objects[0].meshRenderer;
                mat = meshRenderer.material;
            }
            SliderEvent sliderObj = FindObjectOfType<SliderEvent>();

            int nums = GameObject.Find("SetSizeBtn Group").transform.childCount;
            int btnIndex = -1;
            for (int i = 0; i < nums; i++)
            {
                
                GameObject.Find("CircleNum" + i).GetComponent<Image>().sprite = Resources.Load("Sprites/Borders/Basic/Basic Outline 10px - Stroke 4px", typeof(Sprite)) as Sprite;
                GameObject.Find("CircleNum" + i).transform.GetChild(0).GetComponent<Text>().color = new Color32(111, 124, 138, 60);//new Color32(56, 126, 184, 255);


                if (("CircleNum" + i) == this.name)
                {
                    btnIndex = i;                  
                }

                if (i >= sliderObj.widgetNums)
                {
                    //GameObject.Find("CircleNum" + i).GetComponent<Button>().interactable = false;
                    GameObject.Find("CircleNum" + i).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                    GameObject.Find("CircleNum" + i).GetComponent<Button>().enabled = false;
                }
                else
                {
                    //GameObject.Find("CircleNum" + i).GetComponent<Button>().interactable = false;
                    GameObject.Find("CircleNum" + i).GetComponent<Image>().color = new Color32(56, 126, 184, 75);
                    GameObject.Find("CircleNum" + i).transform.GetChild(0).GetComponent<Text>().color = new Color32(56, 126, 184, 255);
                    GameObject.Find("CircleNum" + i).GetComponent<Button>().enabled = true;
                }
            }

            
            float[] _CircleSize = mat.GetFloatArray("_CircleSize");
            if (this.enabled)
            {
                this.GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                //this.GetComponent<Image>().sprite = Resources.Load("Sprites/Borders/Basic/Basic Filled 10px", typeof(Sprite)) as Sprite;
                this.transform.GetChild(0).GetComponent<Text>().color = new Color32(245, 36, 18, 180);
                
            }


            if (btnIndex != -1)
            {
                sliderObj.sliNumOn = btnIndex;
                sliderObj.GetComponent<Slider>().value = _CircleSize[btnIndex];

                float[] _LensShapeNums = mat.GetFloatArray("_LensIndexs");
                if (_LensShapeNums[btnIndex] == 0.1f)
                {                   
                    GameObject.Find("LensShape" + 0).GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                    GameObject.Find("LensShape" + 1).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                    GameObject.Find("LensShape" + 2).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                }
                else if (_LensShapeNums[btnIndex] == 0.2f)
                {
                    GameObject.Find("LensShape" + 1).GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                    GameObject.Find("LensShape" + 0).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                    GameObject.Find("LensShape" + 2).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                }
                else if (_LensShapeNums[btnIndex] == 0.3f)
                {
                    GameObject.Find("LensShape" + 2).GetComponent<Image>().color = new Color32(245, 36, 18, 180);
                    GameObject.Find("LensShape" + 0).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                    GameObject.Find("LensShape" + 1).GetComponent<Image>().color = new Color32(111, 124, 138, 30);

                }
            }

            mat.SetInt("_CurrentWidgetNum", btnIndex);

        }
    }
}
