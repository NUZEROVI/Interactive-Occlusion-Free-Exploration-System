using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class BtnSetPressedColor : MonoBehaviour
    {
        private Button btn;

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
        
            int nums = GameObject.Find("Data Lists").transform.childCount;
            for (int i = 0; i < nums; i++)
            {
                GameObject.Find("Datasets - " + i).GetComponent<Image>().color = this.GetComponent<Button>().colors.normalColor;
            }

            this.GetComponent<Image>().color = this.GetComponent<Button>().colors.pressedColor;
        }
    }
}