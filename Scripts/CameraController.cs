using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	public GameOfLife LifeController;
	public GameObject CameraUpDownGimbal;
	public GameObject MainCamera;
	public float mouseSensitivityx = 10f;
	public float mouseSensitivityy = 5f;
	public float mouseSensitivityz = 1f;
	public bool hideMouse = false;

	// Use this for initialization
	void Start () {
		if (!Application.isEditor || hideMouse)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		Vector3 cameraPosition = LifeController.XYZDimensions;
		cameraPosition = cameraPosition * LifeController.cellSpacing;
		cameraPosition = cameraPosition * 0.5f;
		transform.position = cameraPosition;
	}
	
	// Update is called once per frame
	void Update () {
		float mousex = Input.GetAxis("Mouse X");
		float mousey = Input.GetAxis("Mouse Y");
		float mousez = Input.GetAxis("Mouse ScrollWheel");
		transform.Rotate(transform.up,mousex * Time.deltaTime * mouseSensitivityx);
		CameraUpDownGimbal.transform.Rotate(mousey * Time.deltaTime * mouseSensitivityy,0,0,Space.Self);
		Vector3 cposition = MainCamera.transform.localPosition;
		cposition.z += mousez * mouseSensitivityz;
		MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition,cposition,1f);
		
	}
}
