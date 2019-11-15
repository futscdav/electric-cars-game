using UnityEngine;
using System.Collections.Generic;

public class CrossTRoad : Road {

	public override Vector2 upperLaneOffset {get {return new Vector2(0f, 0.15f);}} 
	public override Vector2 lowerLaneOffset {get {return new Vector2(0f, -0.15f);}}

	public CrossTRoad(int x, int y) : base(x, y) {
		
	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		//reuse the 4 way
		return CrossRoad.CreateCrossroadWaypoint(comingfrom, goingto, this);
	}
}
