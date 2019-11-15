using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CurvedRoad : Road {

	public override Vector2 upperLaneOffset {get {return new Vector2(0f, 0f); }}
	public override Vector2 lowerLaneOffset {get {return new Vector2(0.25f, -0.25f); }}

	public CurvedRoad(int x, int y) : base(x, y) {
	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		List<RoadWaypoint> w = null;

		if (comingfrom.AlmostEqual(-Vector2.up) && goingto.AlmostEqual(Vector2.right)) {
			//w = AsVector() + upperLaneOffset;
			w = GetWaypoints(true);
		}
		else if (comingfrom.AlmostEqual(-Vector2.right) && goingto.AlmostEqual(Vector2.up)) {
			//w = AsVector() + new Vector2(lowerLaneOffset.x, -lowerLaneOffset.y);
			w = GetWaypoints(false);
		}
		else if (comingfrom.AlmostEqual(-Vector2.right) && goingto.AlmostEqual(-Vector2.up)) {
			//w = AsVector() + upperLaneOffset;
			w = GetWaypoints(true);
		}
		else if (comingfrom.AlmostEqual(Vector2.up) && goingto.AlmostEqual(Vector2.right)) {
			//w = AsVector() + lowerLaneOffset;
			w = GetWaypoints(false);
		}
		else if (comingfrom.AlmostEqual(Vector2.up) && goingto.AlmostEqual(-Vector2.right)) {
			//w = AsVector() + new Vector2(-upperLaneOffset.x, upperLaneOffset.y);
			w = GetWaypoints(true);
		}
		else if (comingfrom.AlmostEqual(Vector2.right) && goingto.AlmostEqual(-Vector2.up)) {
			//w = AsVector() + new Vector2(-lowerLaneOffset.x, lowerLaneOffset.y);
			w = GetWaypoints(false);
		}
		else if (comingfrom.AlmostEqual(Vector2.right) && goingto.AlmostEqual(Vector2.up)) {
			//w = AsVector() - upperLaneOffset;
			w = GetWaypoints(true);
		}
		else if (comingfrom.AlmostEqual(-Vector2.up) && goingto.AlmostEqual(-Vector2.right)) {
			//w = AsVector() - lowerLaneOffset;
			w = GetWaypoints(false);
		}
		else if (comingfrom.AlmostEqual(-goingto)) {
			w = GetWaypoints(true);
		}
		else {
			Debug.LogError("Uncaught case! " + comingfrom + " " + goingto);
		}

		return SortWaypoints(w, comingfrom);
	}

	//Find waypoints for given lane - get lane waypoints, then sort them according to their distance
	private List<RoadWaypoint> GetWaypoints(bool upper) {
		RoadWaypoint[] all = GetComponentsInChildren<RoadWaypoint>();	
		List<RoadWaypoint> suitable = new List<RoadWaypoint>();
		foreach (RoadWaypoint point in all) {
			if (point.upperLane == upper) {
				suitable.Add(point);
			}
		}
		return suitable;
	}

	private class Comparer : IComparer<Waypoint> {
		public Vector2 reference;

		public int Compare (Waypoint a, Waypoint b) {
			int result = reference.Distance(a.AsVector()) > reference.Distance(b.AsVector()) ? -1 : 1;
			return result;
		}
	}

	private List<Waypoint> SortWaypoints(List<RoadWaypoint> points, Vector2 comingfrom) {
		List<Waypoint> path = new List<Waypoint>();
		foreach (RoadWaypoint point in points) {
			Waypoint w = new Waypoint();
			w.onRoad = this;
			w.Set(point.transform.position.ToVector2());
			path.Add(w);
		}
		Vector2 reference = AsVector() + comingfrom;
		path.Sort(new Comparer() {reference = reference});
		return path;
	}
}
