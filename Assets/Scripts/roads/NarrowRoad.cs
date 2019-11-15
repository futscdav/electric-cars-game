using UnityEngine;
using System.Collections.Generic;

public class NarrowRoad : Road {

	public override Vector2 upperLaneOffset {get {return new Vector2(0f, 0.15f);}} 
	public override Vector2 lowerLaneOffset {get {return new Vector2(0f, -0.15f);}}

	public NarrowRoad(int x, int y) : base(x, y) {
	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		//only depends on comingfrom since it must be the same
		Waypoint w = null;
		if (comingfrom.AlmostEqual(Vector2.up)) {
			w = AsVector() + upperLaneOffset.Swap();
		}
		else if (comingfrom.AlmostEqual(-Vector2.up)) {
			w = AsVector() + lowerLaneOffset.Swap();
		}
		else if (comingfrom.AlmostEqual(Vector2.right)) {
			w = AsVector() + lowerLaneOffset;
		} 
		else if (comingfrom.AlmostEqual(-Vector2.right)) {
			w = AsVector() + upperLaneOffset;
		}
		else {
			Debug.LogError("Wrong direction vector " + comingfrom + " " + goingto);
		}
		w.onRoad = this;

		return new List<Waypoint>(new Waypoint[] {w});
	}
}
