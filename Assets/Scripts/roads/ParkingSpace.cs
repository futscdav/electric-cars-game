using UnityEngine;
using System.Collections;

public class ParkingSpace : MonoBehaviour {

	public ParkingSpotRoad road;
	public bool occupied = false;
	
	void Awake () {
		road = gameObject.GetComponentInParent<Road>() as ParkingSpotRoad;
		//Debug.Log(road);
	}

	public bool IsOccupied() {
		return occupied;
	}

	public void Occupy() {
		occupied = true;
	}

	public void Vacate() {
		occupied = false;
	}

	public override string ToString () {
		return string.Format ("[ParkingSpace on {0}]", road);
	}
}
