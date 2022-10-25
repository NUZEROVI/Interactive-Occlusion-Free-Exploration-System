using UnityEngine;
using System.Collections;

public class LUI_MenuCamControl : MonoBehaviour {

	[Header("OBJECTS")]
	public Transform currentMount;
	public Camera Cam;

	[Header("SETTINGS")]
	[Tooltip("Set 1.1 for instant fly")]
	[Range(0.01f,1.1f)]public float speed = 1.0f;
	public float zoom = 1.0f;

	private Vector3 lastPosition;
	public bool isEnter = false;

	void Start ()
	{
		lastPosition = transform.position;
	}

	void Update ()
	{
        transform.position = Vector3.Lerp(transform.position, currentMount.position, speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, currentMount.rotation, speed);
		Cam.fieldOfView = 60 + zoom;
        lastPosition = transform.position;

        if (Input.anyKey && isEnter == false)
        {
			setMount(GameObject.Find("Missions Mount").transform);
        }
    }

	public void setMount (Transform newMount)
	{
		currentMount = newMount;
		if(newMount.name == "Armory Weapon Mount")
        {
			GameObject modelToggle = GameObject.Find("/Portrait/Armory Weapon Panel/Model Toggle Group");
			modelToggle.SetActive(true);
			GameObject Functions =  GameObject.Find("/Portrait/Armory Weapon Panel/Functions");
			Functions.SetActive(true);
			GameObject rotBtn = GameObject.Find("/Portrait/Armory Weapon Panel/Arrow Rotate/RotateBtn"); // For remote use
			rotBtn.SetActive(true);
			isEnter = true;

		}
        else
        {
			GameObject modelToggle = GameObject.Find("/Portrait/Armory Weapon Panel/Model Toggle Group");
			modelToggle.SetActive(false);
			GameObject Functions = GameObject.Find("/Portrait/Armory Weapon Panel/Functions");
			Functions.SetActive(false);
			GameObject rotBtn = GameObject.Find("/Portrait/Armory Weapon Panel/Arrow Rotate/RotateBtn"); // For remote use
			rotBtn.SetActive(false);
		}
	}
}