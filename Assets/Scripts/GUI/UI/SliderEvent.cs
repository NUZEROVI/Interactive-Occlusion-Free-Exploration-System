using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class SliderEvent : MonoBehaviour
    {
        [SerializeField] public VolumeRenderedObject volume;
        //[SerializeField] protected Slider sliderXMin, sliderXMax, sliderYMin, sliderYMax, sliderZMin, sliderZMax;
        //[SerializeField] protected Transform axis;
        public Slider slider;
        public string sliname;
        public int widgetNums = 0;
        public int sliNumOn;
        MeshRenderer meshRenderer, meshRenderer_Mask;
        Material mat, mat_Mask;

        void Start()
        {
            slider = GetComponent<Slider>();

            if (slider != null)
            {
                slider.onValueChanged.AddListener(delegate { ValueChange(slider.value); });
//                Debug.Log(slider.name);
            }

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
        }

        public void ValueChange(float v)
        {            
            if (sliname == "SLICircleSize")
            {
                float[] _CircleSize = mat.GetFloatArray("_CircleSize");
                Hover hover = FindObjectOfType<Hover>();
                hover._CircleSize[sliNumOn] = v;
                _CircleSize[sliNumOn] = v;
                mat.SetFloatArray("_CircleSize", _CircleSize);
                mat_Mask.SetFloatArray("_CircleSize", _CircleSize);
                //mat.SetFloat("_cySize", v);
            }

            if (sliname == "SLISurface")
            {
                // mat.SetFloat("_s", v);
                float[] _CircleSize = mat.GetFloatArray("_CircleSize");
                Hover hover = FindObjectOfType<Hover>();
                hover._CircleSize[sliNumOn] = v;
                _CircleSize[sliNumOn] = v;
                mat.SetFloatArray("_CircleSize", _CircleSize);
                mat_Mask.SetFloatArray("_CircleSize", _CircleSize);
            }


        }
    }
}
