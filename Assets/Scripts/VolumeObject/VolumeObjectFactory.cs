using System;
using System.Collections.Generic;
using UnityEngine;
using SeawispHunter.Maths;
using System.IO;

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory
    {
        private static float mutualInfo(float[] d1, float[] d2, VolumeDataset dataset)
        {
            int binCount = (int)Vector3.Distance(new Vector3(0, 0, 0), new Vector3(dataset.dimZ, dataset.dimY, dataset.dimX));

            Tally<float, float> tally
              = new Tally<float, float>(binCount, x => (int)(x),
                                        binCount, y => (int)(y));

            // estimate the joint distribution of X and Y
            for (int z = 0; z < dataset.dimZ; z++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int x = 0; x < dataset.dimX; x++)
                    {
                        int arr_index = x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY);
                        tally.Add(d1[arr_index], d2[arr_index]);
                    }
                }
            }

            // information-theoretic 
            float[] px = tally.probabilityX;
            float[] py = tally.probabilityY;
            float[,] pxy = tally.probabilityXY;
            float Hx = ProbabilityDistribution.Entropy(px, 2);
            float Hy = ProbabilityDistribution.Entropy(py, 2);
            float HY_X = ProbabilityDistribution.ConditionalEntropyYX(pxy, px, 2);
            float Hxy = Hx + HY_X;

            float Ixy = Hx + Hy - Hxy;
            float IheadXY = (2 * Ixy) / (Hx + Hy);

            return IheadXY;

        }

        public static void CreateIsosurfacePair(VolumeDataset dataset)
        {
            string filename = Application.streamingAssetsPath + "/SimilarityMapCSV/" + dataset.datasetName + "_similarityMap.csv";
            TextWriter tw = new StreamWriter(filename, false);
            tw.Close();

            tw = new StreamWriter(filename, true);
            string indexArr = "";

            int isoMin = (int)dataset.GetMinDataValue();
            int isoMax = (int)dataset.GetMaxDataValue();
            int isoRange = (int)(isoMax - isoMin) + 1;

            // 1. Create All iso-surfaces (Mesh) && Cal minimum distance from any point to the surface
            GameObject All_IsoObject = new GameObject("All_Isosurface_" + dataset.datasetName);
            float[,] result = new float[isoRange, isoRange];
            List<float[]> dList = new List<float[]>(); // Store the distances from any point to the isosurface
            for (int i = 0; i < isoRange; i++)
            {
                GameObject IsoObject = new GameObject("Isosurface_" + i + "_" + dataset.datasetName);
                ProcedureMesh valObj = IsoObject.AddComponent<ProcedureMesh>();
                IsoObject.transform.parent = All_IsoObject.transform;
                valObj.dataset = dataset;


                dList.Add(valObj.MakeGrid(i)); // minimum distance from any point to the surface

                if (i == isoRange - 1) indexArr += i;
                else indexArr += i + ",";
            }

            tw.WriteLine(indexArr);

            // 2. Heapmap (similarity map)                    
            for (int i = isoMin; i < isoRange; i++)
            {
                for (int j = i; j < isoRange; j++)
                {
                    // information-theoretic measure of similarity
                    float tmp = mutualInfo(dList[i], dList[j], dataset);
                    if (i != j)
                    {
                        result[i, j] = tmp;
                        result[j, i] = tmp;
                    }
                    else
                    {
                        result[i, j] = tmp;
                    }
                }
            }

            // 3. output to .csv file
            for (int j = isoRange - 1; j > -1; j--)
            {
                string rowArr = "";
                rowArr += j + ",";
                for (int i = 0; i < isoRange; i++)
                {

                    if (i == isoRange - 1)
                        rowArr += result[i, j];
                    else
                        rowArr += result[i, j] + ",";
                }
                tw.WriteLine(rowArr);
            }
            tw.Close();

            All_IsoObject.active = false;

        }

        public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();
      
            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            
            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;
            meshContainer.transform.parent = outerObject.transform;

            int maxDim = Math.Max(dataset.dimX, Math.Max(dataset.dimY, dataset.dimZ));
            outerObject.transform.localScale = new Vector3((float)Math.Round((decimal)dataset.dimX / maxDim, 2), (float)Math.Round((decimal)dataset.dimY / maxDim, 2), (float)Math.Round((decimal)dataset.dimZ / maxDim, 2));
            meshContainer.AddComponent<BoxCollider>();

            meshContainer.AddComponent<Hover>(); // Interactive Control

            GameObject ShaderBuffer = new GameObject("ShaderBuffer");
            ShaderDebugging shaderDebugObj = ShaderBuffer.AddComponent<ShaderDebugging>();
            shaderDebugObj.target = meshContainer;

            outerObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

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
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", null);
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
