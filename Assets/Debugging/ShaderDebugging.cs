using UnityEngine;

namespace UnityVolumeRendering
{
	public class ShaderDebugging : MonoBehaviour
	{
		public GameObject target, target_Mask;

		private Material material, material_Mask;
		private ComputeBuffer buffer, buffer_Mask;
		public Vector4[] element, element_Mask;
		private string label, label_Mask;
		private Renderer render, render_Mask;

		MeshRenderer meshRenderer, meshRenderer_Mask;
		Material mat, mat_Mask;


		void Load()
		{			
			buffer = new ComputeBuffer(10, 16, ComputeBufferType.Default);
			element = new Vector4[10];
			for(int i=0; i <buffer.count; i++)
            {
				element[i] = new Vector4(0, 0, 0, 0);	
			}		
			buffer.SetData(element);

			label = string.Empty;					
		}

		void Load_Mask()
		{
			buffer_Mask = new ComputeBuffer(10, 16, ComputeBufferType.Default);
			element_Mask = new Vector4[10];
			for (int i = 0; i < buffer_Mask.count; i++)
			{
				element_Mask[i] = new Vector4(0, 0, 0, 0);
			}
			buffer_Mask.SetData(element_Mask);

			label_Mask = string.Empty;
		}

		void Start()
		{			
			
			VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();					
			if (objects.Length == 1)
			{
				render = objects[0].GetComponent<Transform>().GetChild(0).GetComponent<Renderer>();					
				material = render.material;
				Load();
				meshRenderer = objects[0].meshRenderer;
				mat = meshRenderer.material;
			}

			VolumeRenderedObject_Mask[] objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
			if (objects_Mask.Length == 1)
			{
				render_Mask = objects_Mask[0].GetComponent<Transform>().GetChild(0).GetComponent<Renderer>();
				material_Mask = render_Mask.material;
				Load_Mask();
				meshRenderer_Mask = objects_Mask[0].meshRenderer;
				mat_Mask = meshRenderer_Mask.material;
			}
		}

		void Update()
		{
			Graphics.ClearRandomWriteTargets();
			material.SetPass(0);
			material_Mask.SetPass(0);

			material.SetBuffer("buffer", buffer);			
			Graphics.SetRandomWriteTarget(1, buffer, false);
			
			material_Mask.SetBuffer("buffer_Mask", buffer_Mask);
			Graphics.SetRandomWriteTarget(2, buffer_Mask, false);

			buffer.GetData(element);
			buffer_Mask.GetData(element_Mask);

			
			label = (element != null && render.isVisible) ? element[0].ToString() : string.Empty;
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
			buffer.Dispose();
			buffer_Mask.Dispose();
		}
	}
}