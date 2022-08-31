using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{

    public class ProcedureMesh : MonoBehaviour
    {
        public float isovalue = 0;
        public VolumeDataset dataset;
        private static float[] CalMinDistance(Collider coll, VolumeDataset dataset)
        {            
            float[] d = new float[dataset.data.Length];
           
            for (int z = 0; z < dataset.dimZ; z++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int x = 0; x < dataset.dimX; x++)
                    {
                        int arr_index = x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY);
                        float minDist = Vector3.Distance(coll.ClosestPointOnBounds(new Vector3(x, y, z)), new Vector3(x, y, z));
                        d[arr_index] = minDist;                                              
                    }
                }
            }
            return d;
        }

        public float[] MakeGrid(float isoVal)
        {
            //MakeGrid
            MarchingCube.grd = new GridPoint[(int)dataset.dimX, (int)dataset.dimY, (int)dataset.dimZ];

            for (int z = 0; z < dataset.dimZ; z++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int x = 0; x < dataset.dimX; x++)
                    {
                        MarchingCube.grd[x, y, z] = new GridPoint();
                        MarchingCube.grd[x, y, z].Position = new Vector3(x, y, z);
                        MarchingCube.grd[x, y, z].On = false;
                    }
                }
            }

            // ReadFile();
            float minValue = dataset.GetMinDataValue();
            float maxValue = dataset.GetMaxDataValue();
            float maxRange = maxValue - minValue;

            float[] b = new float[dataset.data.Length];

            for (int z = 0; z < dataset.dimZ; z++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int x = 0; x < dataset.dimX; x++)
                    {
                        int iData = x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY);

                        b[iData] = (float)(dataset.data[iData] - minValue) / maxRange;

                    }
                }
            }

            isovalue = isoVal / maxRange;
            int arr_index = 0;
            for (int z = 0; z < dataset.dimZ; z++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int x = 0; x < dataset.dimX; x++)
                    {
                        arr_index = (int)(x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY));
                        if (b[arr_index] - isovalue > 0.0)
                        {
                            MarchingCube.grd[x, y, z].On = true;
                        }
                        else
                        {
                            MarchingCube.grd[x, y, z].On = false;
                        }
                    }
                }
            }

            //March()
            Material material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            Mesh mesh = null;
            GameObject IsoObject = this.gameObject;
            //IsoObject.transform.localPosition = new Vector3(-68.0f, -67.0f, 90.0f);
            mesh = MarchingCube.GetMesh(ref IsoObject, ref material);

            MarchingCube.Clear();
            MarchingCube.MarchCubes();

            MarchingCube.SetMesh(ref mesh);
            MeshCollider coll = IsoObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
            return CalMinDistance(coll, dataset); //minimim distance of a point to the surface.

        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }      
    }

}
