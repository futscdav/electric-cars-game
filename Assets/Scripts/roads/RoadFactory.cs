using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class RoadFactory {
	
	private static GameObject narrowPrefab;
	private static GameObject curvedPrefab;
	private static GameObject tcrossPrefab;
	private static GameObject qcrossPrefab;
	private static GameObject endPrefab;
	private static GameObject chargeTurnPrefab;
	public static GameObject chargeSpotPrefab;
	private static GameObject parkingSpotPrefab;
	private static World worldInstance;
	
	private static bool initialized = false;


	private RoadFactory () {
		
	}

	private static void IsNotNull(object thing) {
		if (thing is null) {
			Debug.Log("Null object.");
		}
	}

	private static void InitializePrefabs() {
		if (initialized) return;
		narrowPrefab 		= (GameObject) Resources.Load("RoadNarrow");
		curvedPrefab 		= (GameObject) Resources.Load("RoadCurved");
		tcrossPrefab 		= (GameObject) Resources.Load("RoadTCross");
		qcrossPrefab 		= (GameObject) Resources.Load("RoadQCross");
		endPrefab 			= (GameObject) Resources.Load("RoadEnd");
		chargeTurnPrefab 	= (GameObject) Resources.Load("RoadCharge1");
		chargeSpotPrefab 	= (GameObject) Resources.Load("RoadCharge2");
		parkingSpotPrefab   = (GameObject) Resources.Load("RoadParking");
		IsNotNull(narrowPrefab);
		IsNotNull(curvedPrefab);
		IsNotNull(tcrossPrefab);
		IsNotNull(qcrossPrefab);
		IsNotNull(endPrefab);
		IsNotNull(chargeTurnPrefab);
		IsNotNull(chargeSpotPrefab);
		IsNotNull(parkingSpotPrefab);
		initialized = true;
		Debug.Log("Prefabs initialized");
	}

	private static GameObject SpoofInstantiate(GameObject prefab) {
		InitializePrefabs();
		return (GameObject) Game.Instantiate(prefab);
	}

	public static Road CreateQCrossroad(int x, int y) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(qcrossPrefab);
		road.transform.position = new Vector3(x, y, 0);
		CrossRoad r = road.GetComponent<CrossRoad>();
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		r.up = true;
		r.down = true;
		r.left = true;
		r.right = true;
		return r;
	}

	public static Road CreateTCrossroad(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(tcrossPrefab);
		road.transform.position = new Vector3(x, y, 0);
		road.transform.Rotate(0,0,zRotation);
		CrossTRoad r = road.GetComponent<CrossTRoad>();
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		return r;
	}

	public static Road CreateEnd(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(endPrefab);
		road.transform.position = new Vector3(x, y, 0);
		EndRoad r = road.GetComponent<EndRoad>();
		r.transform.Rotate(0,0,zRotation);
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();

		return r;
	}

	public static Road CreateNarrow(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(narrowPrefab);
		road.transform.position = new Vector3(x, y, 0);
		NarrowRoad r = road.GetComponent<NarrowRoad>();
		r.transform.Rotate(0,0,zRotation);
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		return r;
	}

	/**
	 * Default is down-right
	 * */
	public static Road CreateCurved(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(curvedPrefab);
		road.transform.position = new Vector3(x, y, 0);
		road.transform.Rotate(0,0,zRotation);
		CurvedRoad r = road.GetComponent<CurvedRoad>();
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		return r;
	}

	public static Road CreateChargeTurn(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(chargeTurnPrefab);
		road.transform.position = new Vector3(x, y, 0);
		road.transform.Rotate(0,0,zRotation);
		CrossTRoad r = road.AddComponent<CrossTRoad>();
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		return r;
	}

	public static Road CreateParkingSpot(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(parkingSpotPrefab);
		road.transform.position = new Vector3(x, y, 0);
		road.transform.Rotate(0,0,zRotation);
		ParkingSpotRoad r = road.AddComponent<ParkingSpotRoad>();
		r.xPos = x;
		r.yPos = y;
		r.neighbourRoads = new List<Road>();
		//Register end as parking spot!
		if (worldInstance == null) {
			worldInstance = GameObject.FindObjectOfType<World>();
		}
		
		//Debug.Log("Adding parking spots. " + road.GetComponentsInChildren<ParkingSpace>().Length);
		worldInstance.parkingSpots.AddRange(road.GetComponentsInChildren<ParkingSpace>());
		foreach (ParkingSpace p in road.GetComponentsInChildren<ParkingSpace>()) {
			p.road = r;
		}
		return r;
	}
	
	public static Road CreateChargeSpot(int x, int y, float zRotation) {
		InitializePrefabs();
		GameObject road = (GameObject) SpoofInstantiate(chargeSpotPrefab);
		road.transform.position = new Vector3(x, y, 0);
		road.transform.Rotate(0,0,zRotation);
		PowerStationRoad r = road.AddComponent<PowerStationRoad>();
		r.xPos = x;
		r.yPos = y;

		//Register power stations into the world!
		if (worldInstance == null) {
			worldInstance = GameObject.FindObjectOfType<World>();
		}
		if (worldInstance == null || worldInstance.powerStations == null) {
			throw new ApplicationException("World not instantiated");
		}
		PowerStation[] stations = r.GetComponentsInChildren<PowerStation>();
		worldInstance.powerStations.AddRange(stations);

		r.neighbourRoads = new List<Road>();
		return r;
	}

	public static Road CreateNarrowUpDown(int x, int y) {
		NarrowRoad r = (NarrowRoad) CreateNarrow(x, y, 90);
		r.up = true;
		r.down = true;
		return r;
	}

	public static Road CreateNarrowLeftRight(int x, int y) {
		NarrowRoad r = (NarrowRoad) CreateNarrow(x, y, 0);
		r.left = true;
		r.right = true;
		return r;
	}

	public static Road CreateCurvedLeftUp(int x, int y) {
		CurvedRoad r = (CurvedRoad) CreateCurved(x, y, 180);
		r.left = true;
		r.up = true;
		return r;
	}

	public static Road CreateCurvedLeftDown(int x, int y) {
		CurvedRoad r = (CurvedRoad) CreateCurved(x, y, 270);
		r.left = true;
		r.down = true;
		return r;
	}

	public static Road CreateCurvedRightUp(int x, int y) {
		CurvedRoad r = (CurvedRoad) CreateCurved(x, y, 90);
		r.right = true;
		r.up = true;
		return r;
	}

	public static Road CreateCurvedRightDown(int x, int y) {
		CurvedRoad r = (CurvedRoad) CreateCurved(x, y, 0);
		r.right = true;
		r.down = true;
		return r;
	}

	public static Road CreateUpT(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateTCrossroad(x, y, 180);
		r.up = true;
		r.left = true;
		r.right = true;
		return r;
	}

	public static Road CreateDownT(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateTCrossroad(x, y, 0);
		r.down = true;
		r.left = true;
		r.right = true;
		return r;
	}

	public static Road CreateLeftT(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateTCrossroad(x, y, 270);
		r.left = true;
		r.up = true;
		r.down = true;
		return r;
	}

	public static Road CreateRightT(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateTCrossroad(x, y, 90);
		r.right = true;
		r.up = true;
		r.down = true;
		return r;
	}

	public static Road CreateEndLeft(int x, int y) {
		EndRoad r = (EndRoad) CreateEnd(x, y, 0);
		r.left = true;
		return r;
	}

	public static Road CreateEndRight(int x, int y) {
		EndRoad r = (EndRoad) CreateEnd(x, y, 180);
		r.right = true;
		return r;
	}

	public static Road CreateEndUp(int x, int y) {
		EndRoad r = (EndRoad) CreateEnd(x, y, 270);
		r.up = true;
		return r;
	}

	public static Road CreateEndDown(int x, int y) {
		EndRoad r = (EndRoad) CreateEnd(x, y, 90);
		r.down = true;
		return r;
	}
	
	public static Road CreateChargeSpotLeft(int x, int y) {
		PowerStationRoad r = (PowerStationRoad) CreateChargeSpot(x, y, 0);
		r.left = true;
		return r;
	}

	public static Road CreateChargeSpotRight(int x, int y) {
		PowerStationRoad r = (PowerStationRoad) CreateChargeSpot(x, y, 180);
		r.right = true;
		return r;
	}

	public static Road CreateChargeSpotDown(int x, int y) {
		PowerStationRoad r = (PowerStationRoad) CreateChargeSpot(x, y, 90);
		r.down = true;
		return r;
	}

	public static Road CreateChargeSpotUp(int x, int y) {
		PowerStationRoad r = (PowerStationRoad) CreateChargeSpot(x, y, 270);
		r.up = true;
		return r;
	}

	public static Road CreateParkingSpotUp(int x, int y) {
		ParkingSpotRoad r = (ParkingSpotRoad) CreateParkingSpot(x, y, 0);
		r.transform.position += new Vector3(0, +0.35f, 0);
		r.up = true;
		return r;
	}

	public static Road CreateParkingSpotDown(int x, int y) {
		ParkingSpotRoad r = (ParkingSpotRoad) CreateParkingSpot(x, y, 180);
		r.transform.position += new Vector3(0, -0.35f, 0);
		r.down = true;
		return r;
	}

	public static Road CreateParkingSpotLeft(int x, int y) {
		ParkingSpotRoad r = (ParkingSpotRoad) CreateParkingSpot(x, y, 90);
		r.transform.position += new Vector3(-0.35f, 0, 0);
		r.left = true;
		return r;
	}

	public static Road CreateParkingSpotRight(int x, int y) {
		ParkingSpotRoad r = (ParkingSpotRoad) CreateParkingSpot(x, y, 270);
		r.transform.position += new Vector3(+0.35f, 0, 0);
		r.right = true;
		return r;
	}

	public static Road CreateChargeTurnLeft(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateChargeTurn(x, y, 180);
		r.left = true;
		return r;
	}

	public static Road CreateChargeTurnRight(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateChargeTurn(x, y, 0);
		r.right = true;
		return r;
	}

	public static Road CreateChargeTurnUp(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateChargeTurn(x, y, 90);
		r.up = true;
		return r;
	}

	public static Road CreateChargeTurnDown(int x, int y) {
		CrossTRoad r = (CrossTRoad) CreateChargeTurn(x, y, 270);
		r.down = true;
		return r;
	}

	//bool structors
	private static int CountBools(params bool[] args) {
		int c = 0;
		foreach (bool a in args) {
			if (a)
				c++;
		}
		return c;
	}

	public static Road CreateNarrow(int x, int y, bool up, bool left) {
		if (up && left || !up && !left) {
			throw new System.ArgumentException();
		}
		if (up)
			return CreateNarrowUpDown(x, y);
		else
			return CreateNarrowLeftRight(x, y);
	}

	public static Road CreateCurved(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools (up, down, left, right) != 2) {
			throw new System.ArgumentException();
		}

		if (up && left) {
			return CreateCurvedLeftUp(x, y);
		} else if (up && right) {
			return CreateCurvedRightUp(x, y);
		} else if (down && left) {
			return CreateCurvedLeftDown(x, y);
		} else {
			return CreateCurvedRightDown(x, y);
		}
			
	}

	public static Road CreateTwoSided(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools(up, down, left, right) != 2) {
			throw new ArgumentException();
		}

		if (up && down || left && right) {
			return CreateNarrow(x, y, up, left);
		} else {
			return CreateCurved(x, y, up, down, left, right);
		}
	}

	public static Road CreateT(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools (up, down, left, right) != 3) {
			throw new System.ArgumentException("Bools: " + up + " " + down + " " + left + " " + right);
		}

		if (up && left && right) {
			return CreateUpT(x, y);
		} else if (left && right && down) {
			return CreateDownT(x, y);
		} else if (up && right && down) {
			return CreateRightT(x, y);
		} else {
			return CreateLeftT(x, y);
		}
	}

	public static Road CreateEnd(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools(up, down, left, right) != 1) {
			throw new ArgumentException();
		}

		if (up) {
			return CreateEndUp(x, y);
		} else if (down) {
			return CreateEndDown(x, y);
		} else if (left) {
			return CreateEndLeft(x, y);
		} else {
			return CreateEndRight(x ,y);
		}
	}

	public static Road CreateChargeSpot(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools(up, down, left, right) != 1) {
			throw new ArgumentException();
		}

		if (up) {
			return CreateChargeSpotUp(x, y);
		} else if (down) {
			return CreateChargeSpotDown(x, y);
		} else if (left) {
			return CreateChargeSpotLeft(x, y);
		} else {
			return CreateChargeSpotRight(x, y);
		}
	}

	/*
	 * 
	 * Use this to create a road
	 * 
	 * */
	public static Road CreateGeneralRoad(int x, int y, bool up, bool down, bool left, bool right) {
		int sides = CountBools(up,down,left,right);

		switch (sides) {
		case 0 :
			throw new ArgumentException("Zero bools given!!");
		case 1:
			return CreateEnd(x, y, up, down, left, right);
		case 2 :
			return CreateTwoSided(x, y, up, down, left, right);
		case 3 :
			return CreateT(x, y, up, down, left, right);
		case 4 :
			return CreateQCrossroad(x, y);
		}
		return null;
	}

	public static Road CreateParkingSpot(int x, int y, bool up, bool down, bool left, bool right) {
		if (CountBools(up, down, left, right) != 1) {
			throw new ArgumentException();
		}
		
		if (up) {
			return CreateParkingSpotUp(x, y);
		} else if (down) {
			return CreateParkingSpotDown(x, y);
		} else if (left) {
			return CreateParkingSpotLeft(x, y);
		} else {
			return CreateParkingSpotRight(x, y);
		}
	}
	
}

