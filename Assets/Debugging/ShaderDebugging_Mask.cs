using UnityEngine;

namespace UnityVolumeRendering
{
	public class ShaderDebugging_Mask : MonoBehaviour
	{
		public GameObject target_Mask;

		private Material material_Mask;
		private ComputeBuffer buffer_Mask;
		public Vector4[] element_Mask;
		private string label_Mask;
		private Renderer render_Mask;

		MeshRenderer meshRenderer_Mask;
		Material mat_Mask;


		void Load()
		{
			buffer_Mask = new ComputeBuffer(10, 16, ComputeBufferType.Default);
			element_Mask = new Vector4[10];
			for(int i=0; i < buffer_Mask.count; i++)
            {
				element_Mask[i] = new Vector4(0, 0, 0, 0);	
			}
			buffer_Mask.SetData(element_Mask);

			label_Mask = string.Empty;
			render_Mask = target_Mask.GetComponent<Renderer>();
			material_Mask = render_Mask.material;
		}

		void Start()
		{			
			Load();
			VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
			if (objects_Mask.Length == 1)
			{
				meshRenderer_Mask = objects_Mask[0].meshRenderer;
				mat_Mask = meshRenderer_Mask.material;
			}
		}

		void Update()
		{
			Graphics.ClearRandomWriteTargets();
			material_Mask.SetPass(0);
			material_Mask.SetBuffer("buffer_Mask", buffer_Mask);			
			Graphics.SetRandomWriteTarget(2, buffer_Mask, false);

			buffer_Mask.GetData(element_Mask);

			label_Mask = (element_Mask != null && render_Mask.isVisible) ? element_Mask[0].ToString() : string.Empty;						
		}

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();

            GUI.color = Color.white;
            style.fontSize = 32;          
        }

        void OnDestroy()
		{
			buffer_Mask.Dispose();		
		}
	}
}