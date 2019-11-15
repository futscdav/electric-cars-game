using UnityEngine;
using System.Collections;

public class CameraZoomPinch : MonoBehaviour
{
	public int speed = 4;
	public Camera selectedCamera;
	public float MINSCALE = 2.0F;
	public float MAXSCALE = 5.0F;
	public float varianceInDistances = 5.0F;
	private float touchDelta = 0.0F;
	private Vector2 prevDist = new Vector2(0,0);
	private Vector2 curDist = new Vector2(0,0);
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(1).phase == TouchPhase.Began) {
			
		}
		
		if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) {
			curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
			prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
			touchDelta = curDist.magnitude - prevDist.magnitude;

			Vector3 magicVector = Vector3.forward * 0.16f;
			float magicFloat = magicVector.z;
			if ((touchDelta < 0)) {
				selectedCamera.transform.position = selectedCamera.transform.position - magicVector;
				//selectedCamera.fieldOfView = Mathf.Clamp(selectedCamera.fieldOfView + (1 * speed),15,90);
			}
			
			if ((touchDelta > 0)) {
				selectedCamera.transform.position = selectedCamera.transform.position + magicVector;
				//selectedCamera.fieldOfView = Mathf.Clamp(selectedCamera.fieldOfView - (1 * speed),15,90);
			}

			Vector3 pos = selectedCamera.transform.position;
			pos.z = Mathf.Clamp(pos.z, -15, -1);
			selectedCamera.transform.position = pos;
		}      
	}
}