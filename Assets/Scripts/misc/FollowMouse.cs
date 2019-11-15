using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour
{
	private CameraScript camera;
	private Plane p = new Plane(Vector3.forward, Vector3.zero);
	private World world;

	//Should only go for integer values?
	public bool discrete = true;

	void Start () {
		world = FindObjectOfType<World>();
	}

	void Update () {
		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		float dist;
		Vector3 pos = Vector3.zero;
		if (p.Raycast(r, out dist)) {
			pos = r.GetPoint(dist);
		}
		pos.z = gameObject.transform.position.z;
		//find closest integer pos
		if (discrete) {
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
		}
		if (world.IsInsideWorld(pos))
			gameObject.transform.position = pos;
		else
			gameObject.transform.position = ClampToWorld(pos);
	}

	Vector2 ClampToWorld(Vector2 vector) {
		Rect worldBox = world.GetBoundingBox();
		return new Vector2(Mathf.Clamp(vector.x, worldBox.xMin, worldBox.xMax), 
		                   Mathf.Clamp(vector.y, worldBox.yMin - worldBox.height, worldBox.yMin));
	}
}

