using UnityEngine;
using System.Collections.Generic;

public class CrossRoad : Road {

	public override Vector2 upperLaneOffset {get {return new Vector2(0f, 0.15f);}} 
	public override Vector2 lowerLaneOffset {get {return new Vector2(0f, -0.15f);}}

	public static Vector2 offset = new Vector2(0.15f, 0.15f);

	public CrossRoad(int x, int y) : base(x, y) {

	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		return CreateCrossroadWaypoint(comingfrom, goingto, this);
	}

	public static List<Waypoint> CreateCrossroadWaypoint(Vector2 comingfrom, Vector2 goingto, Road road) {
		Waypoint w = null;

		/**
		 *  #  |  #
		 *  _1_|_2_
		 *   4 | 3 
		 *  #  |  #
		 */
		//waypoint 1
		if (comingfrom.AlmostEqual(-Vector2.right) && goingto.AlmostEqual(-Vector2.up) ||
		    comingfrom.AlmostEqual(-Vector2.right) && goingto.AlmostEqual(-Vector2.right) ||
		    comingfrom.AlmostEqual(-Vector2.up) && goingto.AlmostEqual(-Vector2.right)) {
			w = new Waypoint( road.AsVector() + new Vector2(-offset.x, offset.y));
		}
		//waypoint 2
		else if (comingfrom.AlmostEqual(Vector2.up) && goingto.AlmostEqual(-Vector2.right) ||
		         comingfrom.AlmostEqual(Vector2.up) && goingto.AlmostEqual(Vector2.up) ||
		         comingfrom.AlmostEqual(-Vector2.right) && goingto.AlmostEqual(Vector2.up)) {
			w = new Waypoint( road.AsVector() + new Vector2(offset.x, offset.y));
		}
		//waypoint 3
		else if (comingfrom.AlmostEqual(-Vector2.up) && goingto.AlmostEqual(-Vector2.up) ||
		         comingfrom.AlmostEqual(-Vector2.up) && goingto.AlmostEqual(Vector2.right) ||
		         comingfrom.AlmostEqual(Vector2.right) && goingto.AlmostEqual(-Vector2.up)) {
			w = new Waypoint(  road.AsVector() + new Vector2(-offset.x, -offset.y));
		}
		//waypoint 4
		else if (comingfrom.AlmostEqual(Vector2.right) && goingto.AlmostEqual(Vector2.right) ||
		         comingfrom.AlmostEqual(Vector2.up) && goingto.AlmostEqual(Vector2.right) ||
		         comingfrom.AlmostEqual(Vector2.right) && goingto.AlmostEqual(Vector2.up)) {
			w = new Waypoint(  road.AsVector() + new Vector2(offset.x, -offset.y));
		}
		else {
			Debug.LogError("Uncaught case! " + comingfrom + " " + goingto);
		}

		w.onRoad = road;
		return new List<Waypoint>(new Waypoint[] {w});
	}
}
