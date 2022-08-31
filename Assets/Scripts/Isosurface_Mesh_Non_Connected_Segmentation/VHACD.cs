using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class VHACD : MonoBehaviour
    {
        [System.Serializable]
        public unsafe struct Parameters
        {
            public void Init()
            {
                m_resolution = 100000;
                m_concavity = 0.001;
                m_planeDownsampling = 4;
                m_convexhullDownsampling = 4;
                m_alpha = 0.05;
                m_beta = 0.05;
                m_pca = 0;
                m_mode = 0; // 0: voxel-based (recommended), 1: tetrahedron-based
                m_maxNumVerticesPerCH = 64;
                m_minVolumePerCH = 0.0001;
                m_callback = null;
                m_logger = null;
                m_convexhullApproximation = 1;
                m_oclAcceleration = 0;
                m_maxConvexHulls = 30;
                m_projectHullVertices = true; // This will project the output convex hull vertices onto the original source mesh to increase the floating point accuracy of the results
            }

            [Tooltip("maximum concavity")]
            [Range(0, 1)]
            public double m_concavity;

            [Tooltip("controls the bias toward clipping along symmetry planes")]
            [Range(0, 1)]
            public double m_alpha;

            [Tooltip("controls the bias toward clipping along revolution axes")]
            [Range(0, 1)]
            public double m_beta;

            [Tooltip("controls the adaptive sampling of the generated convex-hulls")]
            [Range(0, 0.01f)]
            public double m_minVolumePerCH;

            public void* m_callback;
            public void* m_logger;

            [Tooltip("maximum number of voxels generated during the voxelization stage")]
            [Range(10000, 64000000)]
            public uint m_resolution;

            [Tooltip("controls the maximum number of triangles per convex-hull")]
            [Range(4, 1024)]
            public uint m_maxNumVerticesPerCH;

            [Tooltip("controls the granularity of the search for the \"best\" clipping plane")]
            [Range(1, 16)]
            public uint m_planeDownsampling;

            [Tooltip("controls the precision of the convex-hull generation process during the clipping plane selection stage")]
            [Range(1, 16)]
            public uint m_convexhullDownsampling;

            [Tooltip("enable/disable normalizing the mesh before applying the convex decomposition")]
            [Range(0, 1)]
            public uint m_pca;

            [Tooltip("0: voxel-based (recommended), 1: tetrahedron-based")]
            [Range(0, 1)]
            public uint m_mode;

            [Range(0, 1)]
            public uint m_convexhullApproximation;

            [Range(0, 1)]
            public uint m_oclAcceleration;

            public uint m_maxConvexHulls;

            [Tooltip("This will project the output convex hull vertices onto the original source mesh to increase the floating point accuracy of the results")]
            public bool m_projectHullVertices;
        };

        unsafe struct ConvexHull
        {
            public double* m_points;
            public uint* m_triangles;
            public uint m_nPoints;
            public uint m_nTriangles;
            public double m_volume;
            public fixed double m_center[3];
        };

        [DllImport("libvhacd")] static extern unsafe void* CreateVHACD();

        [DllImport("libvhacd")] static extern unsafe void DestroyVHACD(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeFloat(
            void* pVHACD,
            float* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeDouble(
            void* pVHACD,
            double* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")] static extern unsafe uint GetNConvexHulls(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe void GetConvexHull(
            void* pVHACD,
            uint index,
            ConvexHull* ch);

        public Parameters m_parameters;

        public VHACD() { m_parameters.Init(); }

        [ContextMenu("Generate Convex Meshes")]
        public unsafe void GenerateConvexMeshes()
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            var vhacd = CreateVHACD();
            var parameters = m_parameters;

            var verts = mesh.vertices;
            var tris = mesh.triangles;
            fixed (Vector3* pVerts = verts)
            fixed (int* pTris = tris)
            {
                ComputeFloat(
                    vhacd,
                    (float*)pVerts, (uint)verts.Length,
                    (uint*)pTris, (uint)tris.Length / 3,
                    &parameters);
            }


            var numHulls = GetNConvexHulls(vhacd);
            List<GameObject> childrensGameObjeects = new List<GameObject>();
            List<int> childrensIndexs = new List<int>();
            List<bool> isConnected = new List<bool>();

            colliderController colliderObj = this.gameObject.AddComponent<colliderController>();
            
            foreach (var index in Enumerable.Range(0, (int)numHulls))
            {
                ConvexHull hull;
                GetConvexHull(vhacd, (uint)index, &hull);

                var hullMesh = new Mesh();
                var hullVerts = new Vector3[hull.m_nPoints];
                List<Vector3> vert_list = new List<Vector3>();
                fixed (Vector3* pHullVerts = hullVerts)
                {
                    var pComponents = hull.m_points;
                    var pVerts = pHullVerts;

                    for (var pointCount = hull.m_nPoints; pointCount != 0; --pointCount)
                    {
                        pVerts->x = (float)pComponents[0];
                        pVerts->y = (float)pComponents[1];
                        pVerts->z = (float)pComponents[2];

                        vert_list.Add(new Vector3((float)pComponents[0], (float)pComponents[1], (float)pComponents[2]));

                        pVerts += 1;
                        pComponents += 3;
                    }
                }

                hullMesh.SetVertices(hullVerts);

                var indices = new int[hull.m_nTriangles * 3];
                Marshal.Copy((System.IntPtr)hull.m_triangles, indices, 0, indices.Length);
                hullMesh.SetTriangles(indices, 0);

                GameObject children = new GameObject();
                children.name = "compound_collider" + "-" + index;
                children.transform.SetParent(gameObject.transform, false);

                Mesh tmp_m = new Mesh();
                tmp_m.name = GetComponent<MeshFilter>().sharedMesh.name + "_" + index;
                tmp_m.SetVertices(vert_list);
                tmp_m.SetTriangles(indices, 0);
                tmp_m.RecalculateNormals();
                MeshFilter tmp_mf = children.AddComponent<MeshFilter>();
                tmp_mf.sharedMesh = tmp_m;
                MeshRenderer tmp_r = children.AddComponent<MeshRenderer>();
                tmp_r.material.color = Random.ColorHSV(0f, 1f, 0.3f, 1f, 0.3f, 1f);
                Rigidbody tmp_rigid = children.AddComponent<Rigidbody>();
                tmp_rigid.useGravity = false;
                tmp_rigid.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
               
                var col = children.AddComponent<MeshCollider>();
                col.convex = true;
                col.isTrigger = true;   
                col.sharedMesh = tmp_m;


                colliderObj.childrensIndexs.Add(index);
                colliderObj.isCollided.Add(false);
 
                GetCollidedIndex obj = children.AddComponent<GetCollidedIndex>();
                Destroy(obj);
               
            }
            this.transform.GetChild(2).gameObject.AddComponent<colliderDetect>();

            DestroyVHACD(vhacd);                   
        }
    }
}