using UnityEngine;

namespace UnityVolumeRendering
{
	public class ShaderDebugging : MonoBehaviour
	{
		public GameObject target;

		private Material material;
		private ComputeBuffer buffer;
		public Vector4[] element;
		private string label;
		private Renderer render;

		MeshRenderer meshRenderer;
		Material mat;


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
			render = target.GetComponent<Renderer>();
			material = render.material;
		}

		void Start()
		{			
			Load();		
			VolumeRenderedObject[] objects = FindObjectsOfType<VolumeRenderedObject>();
			if (objects.Length == 1)
			{
				meshRenderer = objects[0].meshRenderer;
				mat = meshRenderer.material;
			}
		}

		void Update()
		{
			Graphics.ClearRandomWriteTargets();
			material.SetPass(0);
			material.SetBuffer("buffer", buffer);			
			Graphics.SetRandomWriteTarget(1, buffer, false);		

			buffer.GetData(element);			

			label = (element != null && render.isVisible) ? element[0].ToString() : string.Empty;						
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
		}
	}
}