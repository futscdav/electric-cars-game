using UnityEngine;
using System.Collections;

public class ClickScript : MonoBehaviour {

	private UIController controller;

	private bool last2;
	private bool last1;
	private World world;
	private CameraScript camera;

	void Start () {
		controller = GetComponent<UIController>();
		world = FindObjectOfType<World>();
		camera = FindObjectOfType<CameraScript>();
	}

	//Check for mouse clicks OR releases in case of Mobile
	void Update () {
		bool now = Input.GetMouseButton(0);
		if (true) {
			if (now != last1 && last1 == last2) {
				#if UNITY_ANDROID
				if (now) {
				#endif
				controller.OnClick(Input.mousePosition);
				#if UNITY_ANDROID
				}
				if (!now) {
					controller.OnMouseRelease();
				}
				#endif
				CheckMousedown();
			}
		}
		last2 = last1;
		last1 = now;
	}

	//Do Mouse Checking
	Plane p = new Plane(-Vector3.forward, Vector3.zero);
	void CheckMousedown() {
		
		if (!Input.GetMouseButtonDown(0)) {
			return;
		}
		
		//Enable all cars colliders for the duration of this function
		bool[] colliders = EnableCarsColliders();
		// enable matching in colliders
		bool toRestore = Physics2D.queriesStartInColliders;
		Physics2D.queriesStartInColliders = true;

		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		float dist;
		Vector3 point = Vector3.zero;
		if (p.Raycast(r, out dist)) {
			point = r.GetPoint(dist);
		}

		float leastDistance = float.MaxValue;
		Car closestHit = null;
		RaycastHit2D[] hits = Physics2D.LinecastAll(point, point);
		foreach (RaycastHit2D hit in hits) {
			Car clicked = hit.collider.GetComponent<Car>();
			if (hit.collider != null && clicked != null && clicked.transform.position.Distance(point) < leastDistance) {
				leastDistance = clicked.transform.position.Distance(point);
				closestHit = clicked;
			}
		}

		if (closestHit != null) {
			closestHit.ShowInfo();
		}

		//disable collider if necessary
		DisableCarsColliders(colliders);
		Physics2D.queriesStartInColliders = toRestore;
	}

	void DisableCarsColliders(bool[] states) {
		Car[] cararray = world.vehicles.ToArray();
		for (int i = 0; i < cararray.Length; ++i) {
			cararray[i].collider.enabled = states[i];
			cararray[i].collider.transform.localScale /= 5;
		}
	}

	bool[] EnableCarsColliders() {
		Car[] cararray = world.vehicles.ToArray();
		// Debug.Log("Cars: " + cararray.Length);
		bool[] colliderEnabled = new bool[cararray.Length];
		for (int i = 0; i < cararray.Length; ++i) {
			colliderEnabled[i] = cararray[i].collider.enabled;
			if (!colliderEnabled[i]) {
				//temporarily enable the collider
				cararray[i].collider.enabled = true;
			}
			cararray[i].collider.transform.localScale *= 5;
		}
		return colliderEnabled;
	}
}
