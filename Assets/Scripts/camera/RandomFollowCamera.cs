using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RandomFollowCamera : MonoBehaviour
{
	public List<GameObject> list;
	private Vector3 distance;
	// Use this for initialization
	void Start ()
	{

	}

	int frameCount = 0;
	GameObject followed = null;
	float timer = 0;

	// Update is called once per frame
	void Update ()
	{
		if (list == null || list.Count == 0) {
			return;
		}
		if (followed == null) {
			PickToFollow();
		}
		if ((timer += Time.deltaTime) > 6) {
			timer = 0;
			PickToFollow();
		}

		MoveTo (followed.transform.position);

		transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, distance.x, Time.deltaTime),
		                                     Mathf.Lerp(transform.localPosition.y, distance.y, Time.deltaTime),
		                                      gameObject.transform.position.z);
	}

	void PickToFollow() {
		if (followed != null) {
			followed.GetComponent<Car>().DrawJourney = false;
		}
		followed = list[Random.Range(0, list.Count-1)];
		Car c = followed.GetComponent<Car>();
		c.DrawJourney = true;
	}

	void MoveTo(Vector3 pos) {
		distance = pos - gameObject.transform.position;
	}
}

