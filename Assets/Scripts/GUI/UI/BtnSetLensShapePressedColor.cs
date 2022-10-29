using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class BtnSetLensShapePressedColor : MonoBehaviour
    {
        private Button btn;

        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;

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

            VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
            if (objects_Mask.Length == 1)
            {
                meshRenderer_Mask = objects_Mask[0].meshRenderer;
                mat_Mask = meshRenderer_Mask.material;
            }

            int nums = GameObject.Find("SetLensShapeBtn Group").transform.childCount;
       
            int btnIndex = -1;
            for (int i = 0; i < nums; i++)
            {

                GameObject.Find("LensShape" + i).GetComponent<Image>().sprite = Resources.Load("Sprites/Borders/Basic/Basic Outline 10px - Stroke 4px", typeof(Sprite)) as Sprite;
               
                if (("LensShape" + i) == this.name)
                {
                    btnIndex = i;
                }
                GameObject.Find("LensShape" + i).GetComponent<Image>().color = new Color32(111, 124, 138, 30);
                //if (i >= sliderObj.widgetNums)
                //{            
                //    GameObject.Find("LensShape" + i).GetComponent<Image>().color = new Color32(111, 124, 138, 30);               
                //}
                //else
                //{               
                //    GameObject.Find("LensShape" + i).GetComponent<Image>().color = new Color32(56, 126, 184, 75);                                      
                //}
            }

           
            float[] _LensIndexs = mat.GetFloatArray("_LensIndexs");
            Hover hover = FindObjectOfType<Hover>();
            int lensNum = mat.GetInt("_CurrentWidgetNum");           
            GameObject.Find("LensShape" + btnIndex).GetComponent<Image>().color = new Color32(245, 36, 18, 180);         
            if (btnIndex == 0)
            {
                hover._LensIndexs[lensNum] = 0.1f;
                _LensIndexs[lensNum] = 0.1f;
            }else if(btnIndex == 1)
            {
                hover._LensIndexs[lensNum] = 0.2f;
                _LensIndexs[lensNum] = 0.2f;
            }else if(btnIndex == 2)
            {
                hover._LensIndexs[lensNum] = 0.3f;
                _LensIndexs[lensNum] = 0.3f;
            }
        
            //_LensShapeNums[lensNum] = btnIndex;
            mat.SetFloatArray("_LensIndexs", _LensIndexs);
            mat_Mask.SetFloatArray("_LensIndexs", _LensIndexs);
            //sliderObj.sliNumOn = btnIndex;
            //sliderObj.GetComponent<Slider>().value = _CircleSize[btnIndex];


            //mat.SetInt("_CurrentWidgetNum", btnIndex);

        }
    }
}
