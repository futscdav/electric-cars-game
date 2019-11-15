using UnityEngine;
using System.Collections;

public class Waypoint {

	//world coordinates
	public float x;
	public float y;
	public Road onRoad;

	public Vector2 AsVector() {
		return new Vector2(x, y);
	}

	public void Set(Vector2 vec) {
		x = vec.x;
		y = vec.y;
	}

	public Waypoint() {

	}

	public Waypoint(Vector2 vec) {
		Set (vec);
	}

	public static implicit operator Waypoint(Vector2 vec) {
		Waypoint w = new Waypoint();
		w.x = vec.x;
		w.y = vec.y;
		return w;
	}

	public override string ToString () {
		return string.Format ("[Waypoint {0},{1}]", x, y);
	}

}
