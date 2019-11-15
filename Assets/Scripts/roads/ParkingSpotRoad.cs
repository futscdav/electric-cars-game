using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParkingSpotRoad : Road {

	public ParkingSpotRoad(int x, int y) : base(x ,y) {

	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		Waypoint w = new Waypoint();
		w.Set(transform.position.ToVector2());
		w.onRoad = this;
		return new List<Waypoint>(new Waypoint[] {w});
	}

	public ParkingSpace GetEmptySpace() {
		//store the value at some point, effectivity
		ParkingSpace[] spots = GetComponentsInChildren<ParkingSpace>();
		foreach (ParkingSpace p in spots) {
			if (!p.occupied) {
				return p;
			}
		}
		return null;
	}

	public ParkingSpace ParkCar(Car c) {
		ParkingSpace[] spots = GetComponentsInChildren<ParkingSpace>();
		foreach (ParkingSpace p in spots) {
			if (!p.occupied) {
				p.occupied = true;
				return p;
			}
		}
		Debug.LogError("Cannot park car due to fully occupied road");
		return null;
	}

}
