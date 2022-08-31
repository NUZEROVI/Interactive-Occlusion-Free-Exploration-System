// Put in Editor directory.
using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class GenerateVolumeTexture : EditorWindow
    {
        Transform Cage;
        int size = 128;
        string filename = "dataset_segClusters_4.bin";

        [MenuItem("Assets/Generate Volume Texture")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(GenerateVolumeTexture));
        }

        void SaveFloatArrayToFile(float[] x, string path)
        {
            byte[] a = new byte[x.Length * 4];
            System.Buffer.BlockCopy(x, 0, a, 0, a.Length);
            System.IO.File.WriteAllBytes(path, a);
        }

        void OnGUI()
        {
            Cage = EditorGUILayout.ObjectField("Cage", Cage, typeof(Transform), true) as Transform;
            size = EditorGUILayout.IntField("Volume dimension:", size);
            filename = EditorGUILayout.TextField("File name: ", filename);
            if (GUILayout.Button("Make")) GenerateVolume();
        }

        bool IsPointInsideCollider(Vector3 C, Vector3 P)
        {
            return !Physics.Linecast(C, P);
        }
        bool IsInside(MeshCollider c, Vector3 point)
        {
            Vector3 closest = c.ClosestPoint(point);
            // Because closest=point if point is inside - not clear from docs I feel
            return closest == point;
        }
        public static bool IsPointWithinCollider(MeshCollider collider, Vector3 point)
        {
            return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }

        public bool IsInCollider(MeshCollider other, Vector3 point)
        {
            Vector3 from = (Vector3.up * 5000f);
            Vector3 dir = (point - from).normalized;
            float dist = Vector3.Distance(from, point);
            //fwd      
            int hit_count = Cast_Till(from, point, other);
            //back
            dir = (from - point).normalized;
            hit_count += Cast_Till(point, point + (dir * dist), other);

            if (hit_count % 2 == 1)
            {
                return (true);
            }
            return (false);
        }

        int Cast_Till(Vector3 from, Vector3 to, MeshCollider other)
        {
            int counter = 0;
            Vector3 dir = (to - from).normalized;
            float dist = Vector3.Distance(from, to);
            bool Break = false;
            while (!Break)
            {
                Break = true;
                RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
                for (int tt = 0; tt < hit.Length; tt++)
                {
                    if (hit[tt].collider == other)
                    {
                        counter++;
                        from = hit[tt].point + dir.normalized * .001f;
                        dist = Vector3.Distance(from, to);
                        Break = false;
                        break;
                    }
                }
            }
            return (counter);
        }       

        void GenerateVolume()
        {
            int objs = Cage.childCount; // 4 Clusters
            float[] voxels = new float[size * size * size];
            for (int d = 0; d < voxels.Length; d++)
            {
                voxels[d] = 0.0f;
            }

            for (int j = 0; j < objs; j++)
            {
                Cage.GetChild(j).gameObject.SetActive(false);  // 4 Clusters disable       
            }


            float s = 1.0f / size;
            Vector3 p = Cage.position;

            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = p - new Vector3(0.5f, 0.5f, 0.5f);
            point.transform.localScale = new Vector3(s, s, s);
            point.GetComponent<SphereCollider>().radius = s * 0.5f;

            for (int j = 0; j < objs; j++)
            {
                Cage.GetChild(j).gameObject.SetActive(true);  // 4 Clusters disable            

                int SICs = Cage.GetChild(j).childCount; // Same Isosurface Cluster indexs (SICs)               
                float stepAcc = 0.1f;
                float[] densities = new float[SICs];

                for (int i = 0; i < SICs; i++)
                {
                    densities[i] = stepAcc;
                    stepAcc += 0.1f;
                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(false);  // Same Isosurface Cluster disable
                }

                for (int i = 0; i < SICs; i++)
                {

                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(true);
                    int w = 0;
                    Vector3 o = new Vector3(p.x - 0.5f, p.y - 0.5f, p.z - 0.5f);

                    for (int z = 0; z < size; ++z)
                    {
                        for (int y = 0; y < size; ++y)
                        {
                            for (int x = 0; x < size; ++x, ++w)
                            {
                                Vector3 pos = new Vector3(s * x + o.x, s * y + o.y, s * z + o.z);
                                point.transform.position = pos;

                                if (IsInCollider(Cage.GetChild(j).GetChild(i).GetComponent<MeshCollider>(), pos))
                                {                                   
                                    voxels[w] = densities[i];
                                }

                                if (Physics.CheckSphere(pos, s * 0.5f))
                                {                                   
                                    voxels[w] = densities[i];
                                }
                            }
                        }
                    }
                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(false);
                }
                Cage.GetChild(j).gameObject.SetActive(false);
            }

            DestroyImmediate(point);
            SaveFloatArrayToFile(voxels, Application.dataPath + "/StreamingAssets/IsoSegCluster/" + filename);
        }
      
        void GenerateVolumeOtherDimExample() // Foot dataset
        {
            int objs = Cage.childCount; // 4 Clusters
            float[] voxels = new float[143 * 256 * 183];
            for (int d = 0; d < voxels.Length; d++)
            {
                voxels[d] = 0.0f;
            }

            for (int j = 0; j < objs; j++)
            {
                Cage.GetChild(j).gameObject.SetActive(false);  // 4 Clusters disable       
            }

            float s = 1.0f / size;
            float s1 = 1.0f / 256;
            float s2 = s1 / 2.285714285714286f; // 256/112
            Vector3 p = Cage.position;

            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = p - new Vector3(0.279296875f, 0.5f, 0.357421875f);
            point.transform.localScale = new Vector3(1.79020979020979f, s1, 1.398907103825137f); // 1/(183/256)
            point.GetComponent<SphereCollider>().radius = s * 0.5f;

            for (int j = 0; j < objs; j++)
            {
                Cage.GetChild(j).gameObject.SetActive(true);  // 4 Clusters disable            

                int SICs = Cage.GetChild(j).childCount; // Same Isosurface Cluster indexs (SICs)
              
                float stepAcc = 1.0f;
                float[] densities = new float[SICs];

                for (int i = 0; i < SICs; i++)
                {
                    densities[i] = stepAcc;
                    stepAcc += 1.0f;
                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(false);  // Same Isosurface Cluster disable
                }

                for (int i = 0; i < SICs; i++)
                {

                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(true);
                    int w = 0;
                    Vector3 o = new Vector3(p.x - 0.279296875f, p.y - 0.5f, p.z - 0.357421875f);

                    for (int z = 0; z < 183; ++z)
                    {
                        for (int y = 0; y < 256; ++y)
                        {
                            for (int x = 0; x < 143; ++x, ++w)
                            {
                                Vector3 pos = new Vector3(s * x + o.x, s * y + o.y, s * z + o.z);
                                point.transform.position = pos;

                                if (IsInCollider(Cage.GetChild(j).GetChild(i).GetComponent<MeshCollider>(), pos))
                                {                                    
                                    voxels[w] = densities[i];
                                }

                                if (Physics.CheckSphere(pos, s * 0.5f))
                                {                                 
                                    voxels[w] = densities[i];
                                }
                            }
                        }
                    }
                    Cage.GetChild(j).GetChild(i).gameObject.SetActive(false);
                }
                Cage.GetChild(j).gameObject.SetActive(false);
            }

            DestroyImmediate(point);
            SaveFloatArrayToFile(voxels, Application.dataPath + "/StreamingAssets/IsoSegCluster/" + filename);

        }
    }
}


//public static float[] VoxelizeMesh(MeshFilter meshFilter, int size, Transform Cage)
//{

//    float[] voxels = new float[size * size * size];


//    if (!meshFilter.TryGetComponent(out MeshCollider meshCollider))
//    {
//        meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
//    }

//    if (!meshFilter.TryGetComponent(out VoxelizedMesh voxelizedMesh))
//    {
//        voxelizedMesh = meshFilter.gameObject.AddComponent<VoxelizedMesh>();
//    }

//    Bounds bounds = meshCollider.bounds;
//    Vector3 minExtents = bounds.center - bounds.extents;
//    float halfSize = voxelizedMesh.HalfSize;
//    Vector3 count = bounds.extents / halfSize;
//    Vector3 hsize = bounds.extents / size;
//    voxelizedMesh.GridPoints.Clear();
//    voxelizedMesh.LocalOrigin = voxelizedMesh.transform.InverseTransformPoint(minExtents);

//    int i = 0;
//    float s = hsize.x;
//    Vector3 p = Cage.position;
//    Vector3 o = new Vector3(p.x - 0.5f, p.y - 0.5f, p.z - 0.5f);
//    for (int x = 0; x < size; ++x)
//    {
//        for (int z = 0; z < size; ++z)
//        {
//            for (int y = 0; y < size; ++y, ++i)
//            {
//                Vector3 pos = voxelizedMesh.PointToPosition(new Vector3Int(x, y, z));



//                if (Physics.CheckSphere(new Vector3(s * x + o.x, s * y + o.y, s * z + o.z), s * 0.5f))
//                    voxels[i] = 0.9f;
//                else
//                    voxels[i] = 0.0f;

//            }
//        }
//    }
//    return voxels;
//    //SaveFloatArrayToFile(voxels, Application.dataPath + "/StreamingAssets/" + filename);
//}

//if (IsInside(Cage.GetChild(j).GetComponent<MeshCollider>(), pos))
//{
//    //Debug.Log(Cage.GetChild(j).gameObject.name);//2097152
//    voxels[i] = densities[j];
//}

//if (Cage.GetChild(j).GetComponent<MeshCollider>().sharedMesh.bounds.Contains(pos))
//{
//    voxels[i] = densities[j];
//}

//if (IsPointWithinCollider(Cage.GetChild(j).GetComponent<MeshCollider>(), pos))
//{
//    //Debug.Log(Cage.GetChild(j).gameObject.name); //1,553,102
//    voxels[i] = densities[j];//densities[j];
//}

//if (this.IsPointInsideCollider(Cage.GetChild(j).transform.position, pos)) //+ new Vector3(0.5f, 0.5f, 0.5f)
//{
//    //Debug.Log(Cage.GetChild(j).gameObject.name); //1,553,102
//    voxels[i] = densities[j];
//}

//if(j == 0)
//{

//if(IsInside(Cage.GetChild(j).GetComponent<Collider>(), pos)){
//    Debug.Log(Cage.GetChild(j).gameObject.name);//2097152
//    voxels[i] = density;
//}
//if (m_Bounds.Contains(Cage.GetChild(j).transform.TransformPoint(pos)))
//{

//}
//}


//Collider[] hitColliders = Physics.OverlapSphere(pos, s * 0.5f);
//foreach (Collider hitCollider in hitColliders)
//{
//    if (hitCollider.bounds.Contains(pos))
//    {

//    }
//    //Debug.Log(hitCollider.gameObject.name); 21253, 1884
//}
//if (hitColliders.Length > 0)
//{

//    voxels[i] = density;
//}

//Collider[] hitColliders = Physics.OverlapSphere(center, radius);



//if (ExampleClass.ExplosionDamage(pos, s * 0.5f))
//{
//    Debug.Log(Cage.GetChild(j).gameObject.name);
//    voxels[i] = density;
//}