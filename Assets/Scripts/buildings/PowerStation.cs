using UnityEngine;
using System.Collections;

public class PowerStation : Building {

	public Road road;
	public RoadWaypoint chargingPoint;
	public bool occupied;
	
	public bool Powered {
		get {
			Connectible c = GetComponent<Connectible>();
			if (c.isConnectedToPowerplant) {
				return true;
			}
			return false;
		}
	}

	// Use this for initialization
	void Start () {
		road = transform.parent.GetComponent<Road>();
	}

	public void Free() {
		occupied = false;
	}

	public void Occupy() {
		occupied = true;
	}

	void OnDestroy() {
		//Debug.Log ("PowerStation Destroyed!!!!!!!");
		try {
			FindObjectOfType<World>().powerStations.Remove(this);
		} catch (System.Exception) {

		}
	}

	public Waypoint GetWaypoint() {
		Waypoint w = new Waypoint(chargingPoint.transform.position.ToVector2());
		w.onRoad = road;
		return w;
	}
	
	public void CheckForElectricity() {

	}
}
