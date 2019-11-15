using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Road : MonoBehaviour {

	public int xPos;
	public int yPos;

	public bool down = false;
	public bool up = false;
	public bool left = false;
	public bool right = false;

	public List<Road> neighbourRoads;

	//See prefabs for orientation
	public virtual Vector2 upperLaneOffset {get {return new Vector2(0f, 0f); }}
	public virtual Vector2 lowerLaneOffset {get {return new Vector2(0f, 0f); }}

	public Road(int x, int y) {
		xPos = x;
		yPos = y;

		neighbourRoads = new List<Road>();
	}

	public abstract List<Waypoint> CreateWaypoints(Vector2 comingfrom, Vector2 goingto);

	public Vector2 AsVector() {
		return new Vector2(xPos, yPos);
	}

	public override string ToString() {
		return "Road {x=" + xPos + ", y=" + yPos + "}";
	}
}
