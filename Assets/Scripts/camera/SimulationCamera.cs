using UnityEngine;
using System.Collections.Generic;

public class SimulationCamera : MonoBehaviour {

	public List<Car> list;

	private List<Car> followableCars = new List<Car>();
	private World world;
	private Vector3 velocity = Vector3.zero;

	public float dampTime = .3f;

	
	int frameCount = 0;
	Car followed = null;

	void Start() {
		world = FindObjectOfType<World>();
	}

	void Update ()
	{
		if (list == null || list.Count == 0) {
			list = FindObjectOfType<World>().vehicles;
			//return;
		}

		UpdateFollowables();

		if (followed == null || !followableCars.Contains(followed)) {
			if (!PickToFollow(followableCars)) {
				return;
			}
		}

		if ((frameCount += world.deltaTime()) > 600) {
			frameCount = 0;
			if (!PickToFollow(followableCars)) {
				return;
			}
		}

		Transform target = followed.transform;

		Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
		Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
		Vector3 destination = transform.position + delta;
		transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);

	}

	void UpdateFollowables() {
		for (int i = followableCars.Count -1; i >= 0; --i) {
			Car c = followableCars[0];
			if (c == null || c.state != Car.CarState.charging || c.state != Car.CarState.driving) {
				followableCars.Remove(c);
			}
		}
		foreach (Car c in list) {
			if (c.state == Car.CarState.charging || c.state == Car.CarState.driving) {
				followableCars.Add(c);
			}
		}
	}
	
	bool PickToFollow(List<Car> list) {
		if (followed != null) {
			followed.HideInfo();
		}

		if (list.Count == 0) {
			return false;
		}

		int index = Random.Range(0, list.Count-1);
		followed = list[index];

		followed.ShowInfo();
		return true;
	}

}

