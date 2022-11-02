using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
	public class SwitchAnim : MonoBehaviour
	{

		[Header("SWITCH")]
		public bool onSwitch;
		public Button switchObject;

		[Header("ANIMATORS")]
		public Animator switchAnimator;
		public Animator onAnimator;
		public Animator offAnimator;

		[Header("ANIM NAMES")]
		public string switchAnim;
		public string onTransition;
		public string offTransition;

		MeshRenderer meshRenderer_Mask;
		Material mat_Mask;
		VolumeRenderedObject_Mask[] objects_Mask;

		void Start()
		{
			this.switchObject.GetComponent<Button>();
			switchObject.onClick.AddListener(TaskOnClick);

			
		}

		void TaskOnClick()
		{
			switchAnimator.Play(switchAnim);

			objects_Mask = FindObjectsOfType<VolumeRenderedObject_Mask>();
			if (objects_Mask.Length == 1)
			{
				meshRenderer_Mask = objects_Mask[0].meshRenderer;
				mat_Mask = meshRenderer_Mask.material;
			}

			if (onSwitch == true)
			{
				offAnimator.Play(offTransition);	
				mat_Mask.SetInt("_allcomponent_on", 1);
				GameObject.Find("BtnState").GetComponent<Text>().text = (mat_Mask.GetInt("_CurrentWidgetNum") + 1).ToString();
			}

			else
			{
				onAnimator.Play(onTransition);
				mat_Mask.SetInt("_allcomponent_on", 0);
				GameObject.Find("BtnState").GetComponent<Text>().text = "[ ALL ]";
			}
		}
	}
}