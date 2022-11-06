using System;
using System.Collections.Generic;
using UnityEngine;
using SeawispHunter.Maths;
using System.IO;

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory_Mask
    {
        public static VolumeRenderedObject_Mask CreateObject(VolumeDataset dataset)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName + "_Mask");
            VolumeRenderedObject_Mask volObj = outerObject.AddComponent<VolumeRenderedObject_Mask>();
            
            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer_Mask"));       

            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            outerObject.transform.localPosition = new Vector3(1.3f, 0.2f, 0.0f);
            //outerObject.transform.Rotate(0, 20, 0);
            //meshContainer.transform.localPosition = Vector3.zero;
            //meshContainer.transform.localScale = Vector3.one;
           // meshContainer.transform.parent = outerObject.transform;

            int maxDim = Math.Max(dataset.dimX, Math.Max(dataset.dimY, dataset.dimZ));
            outerObject.transform.localScale = new Vector3((float)Math.Round((decimal)dataset.dimX / maxDim, 2), (float)Math.Round((decimal)dataset.dimY / maxDim, 2), (float)Math.Round((decimal)dataset.dimZ / maxDim, 2));
            outerObject.transform.localScale = outerObject.transform.localScale * 0.7f;
            //GameObject ShaderBuffer_mask = new GameObject("ShaderBuffer_mask");
            GameObject ShaderBuffer_mask = GameObject.Find("ShaderBuffer");
            ShaderDebugging shaderDebugObj = ShaderBuffer_mask.GetComponent<ShaderDebugging>();
            //ShaderDebugging_Mask shaderDebugObj = ShaderBuffer_mask.AddComponent<ShaderDebugging_Mask>();
            shaderDebugObj.target_Mask = meshContainer;
            
            outerObject.transform.localRotation = Quaternion.Euler(90.0f, 20.0f, 0.0f);

            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);          

            volObj.meshRenderer = meshRenderer;
            volObj.dataset = dataset;
          
            const int noiseDimX = 512;
            const int noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            volObj.transferFunction = tf;
          
            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            volObj.transferFunction2D = tf2D;          

            meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", dataset.GenerateVolume());
            meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
            meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

            meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");



            //if(dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f)
            //{
            //    float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
            //    volObj.transform.localScale = new Vector3(dataset.scaleX / maxScale, dataset.scaleY / maxScale, dataset.scaleZ / maxScale);
            //}

            meshRenderer.sharedMaterial.SetFloat("bounds", dataset.bounds);
            meshRenderer.sharedMaterial.SetFloat("lowBound", dataset.lowBound);
            meshRenderer.sharedMaterial.SetFloat("RotationX", outerObject.transform.rotation.eulerAngles.x);
            meshRenderer.sharedMaterial.SetFloat("RotationY", outerObject.transform.rotation.eulerAngles.y);
            meshRenderer.sharedMaterial.SetFloat("RotationZ", outerObject.transform.rotation.eulerAngles.z);
            meshRenderer.sharedMaterial.SetFloat("ObjDepthX", (float)Math.Round((decimal)dataset.dimX / maxDim, 2));
            meshRenderer.sharedMaterial.SetFloat("ObjDepthY", (float)Math.Round((decimal)dataset.dimY / maxDim, 2));
            meshRenderer.sharedMaterial.SetFloat("ObjDepthZ", (float)Math.Round((decimal)dataset.dimZ / maxDim, 2));

            return volObj;
        }
    }
}
