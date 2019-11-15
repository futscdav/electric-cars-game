using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	private Vector3 position;
	private Vector3 distance;
	private float min = -15f;
	private float max = -4f;
	private World world;

	public GridOverlay overlay;

	#if UNITY_ANDROID
	public bool moveCamera = true;
	#endif
	
	// Use this for initialization
	void Start () {
		transform.position = new Vector3(0, 0, -10f);
		overlay = GetComponent<GridOverlay>();

		distance = new Vector3(0, 0, -8f);
		//distance = transform.localPosition.z;
	}

	public void MoveTo(Vector3 pos) {
		distance = pos - gameObject.transform.position;
		distance.z = 0;
	}

	void FixedUpdate() {
		if (world == null) {
			world = FindObjectOfType<World>();
			if (world == null) {
				return;
			}
		}
		
		#if UNITY_ANDROID
		{ // android

		//distance = Vector3.zero;
		if (moveCamera) {
			Vector3 moves = ReadTouches();
			distance += moves * (1f/Mathf.Abs(transform.position.z))/5f;
			if (moves.AlmostEqual(Vector3.zero)) {
				//less distance
				distance /= 1.5f;
			}
		}
		}
		#else
		{ // not android
		float multiplier = 2f * 1f/Time.timeScale;
		float horizontal;
		float vertical;
		float scroll;

		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		scroll = Input.GetAxis("Mouse ScrollWheel");


		distance.x -= horizontal * (multiplier) * -1f;
		distance.y -= vertical * (multiplier) * -1f;

		distance.z -= scroll * (multiplier+1) * -1f;
		}
		#endif

		// bind the camera position
		Rect worldBoundingBox = world.GetBoundingBox();
		distance.x = Mathf.Clamp(distance.x, worldBoundingBox.xMin, worldBoundingBox.xMax);
		distance.y = Mathf.Clamp(distance.y, worldBoundingBox.yMin - worldBoundingBox.height, worldBoundingBox.yMin);
		distance.z = Mathf.Clamp(distance.z, min, max);

		#if UNITY_ANDROID
		// android
		position = transform.position + distance;
		position.x = Mathf.Clamp(position.x, worldBoundingBox.xMin, worldBoundingBox.xMax);
		position.y = Mathf.Clamp(position.y, worldBoundingBox.yMin - worldBoundingBox.height - 3, worldBoundingBox.yMin);
		position.z = Mathf.Clamp(position.z, min, max);

		/*Debug.Log(distance.z);
		Debug.Log(position.z);*/

		position = Vector3.Lerp(transform.position, position, Time.deltaTime);
		#else
		// not android
		position = new Vector3(Mathf.Lerp(transform.localPosition.x, distance.x, Time.deltaTime),
		                       Mathf.Lerp(transform.localPosition.y, distance.y, Time.deltaTime),
		                       Mathf.Lerp(transform.localPosition.z, distance.z, Time.deltaTime));
		#endif

		transform.position = position;
		//Debug.Log(position);
	}

	#if UNITY_ANDROID

	private Vector3 initial = Vector3.zero;
	//private Vector2 lastUpdated = Vector3.zero;

	Vector3 ReadTouches() {

		//Debug.Log(Input.touchCount);
		Vector3 result = Vector3.zero;

		//Pan
		if (Input.touchCount == 1) {
			Touch touch = Input.touches[0];
			if (touch.phase == TouchPhase.Began) {
				initial = Camera.main.ScreenPointToRay(touch.position).GetPoint(10);
				initial.z = 0;
				//lastUpdated = touch.position;
			}
			if (touch.phase == TouchPhase.Moved) {
				//negligible movement
				result = touch.deltaPosition * transform.position.z;

				Vector3 other = Camera.main.ScreenPointToRay(touch.position).GetPoint(10);
				other.z = 0;
				transform.position += (initial - other) * 1f/Mathf.Abs(transform.position.z);
				ClampPosition();

				/*if (touch.deltaPosition.magnitude > 5) {
					lastUpdated = touch.position;
				}*/
			}
			result = -touch.deltaPosition.ToVector3() * GetComponent<Camera>().fieldOfView * 30;
			return Vector3.zero;
		}


		return new Vector3(result.x/Screen.width, result.y/Screen.height, result.z);
	}

	void ClampPosition() {
		Vector3 position = transform.position;
		Rect worldBoundingBox = world.GetBoundingBox();
		position.x = Mathf.Clamp(position.x, worldBoundingBox.xMin, worldBoundingBox.xMax);
		position.y = Mathf.Clamp(position.y, worldBoundingBox.yMin - worldBoundingBox.height - 3, worldBoundingBox.yMin);
		position.z = Mathf.Clamp(position.z, min, max);
		transform.position = position;
	}
	#endif


}
