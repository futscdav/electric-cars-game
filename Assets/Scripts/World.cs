using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Globalization;
using System;

public class World : MonoBehaviour {

	//creator object
	public WorldCreator creator;
	//building manager - currently has no real function
	public BuildingManager buildings;
	//all vehicles in the current world
	public List<Car> vehicles = new List<Car>();
	//daytime class - takes care about everything time related (except light)
	public Daytime time;
	//how fast the world goes
	public int timescale = 1;

	//Roadmap - contains information about the roads in this world
	public RoadMap roadmap;
	//terrain data - enum grid of terrain information in this world
	public Tile[,] terraingrid;

	//list of all power stations in this world
	public List<PowerStation> powerStations;
	//list of all parking spots in this world
	public List<ParkingSpace> parkingSpots;

	//world size in ints
	public Pair<int, int> WorldSize;
	//reference to the sunlight (Directional Light in the scene)
	public Light sun;
	//is light directed by time?
	bool lightByTime = true;

	//Returns inclusive box of bounds on the world
	//this is slightly tricky to use correctly, see other uses of the box (find references)
	public Rect GetBoundingBox() {
		Rect bounding = new Rect(-WorldSize.First/2, WorldSize.Second/2, WorldSize.First -1 , WorldSize.Second -1);
		return bounding;
	}
	
	public bool IsInsideWorld(Vector2 point) {
		Rect box = GetBoundingBox();
		if (point.x < box.xMin || point.x > box.xMax) {
			return false;
		}
		if (point.y > box.yMin || point.y < box.yMin - box.height) {
			return false;
		}
		return true;
	}

	public void Init() {
		time = gameObject.AddComponent<Daytime>();
		powerStations = new List<PowerStation>();
		parkingSpots = new List<ParkingSpace>();
		creator = gameObject.AddComponent<WorldCreator>();
		creator.world = this;
	}

	public Powerplant BuildPowerplant(Vector2 where) {
		//fill up a tile
		Vector2 imag = GetImagWorldCoordinates((int)where.x, (int)where.y);
		terraingrid[(int)imag.y, (int)imag.x] = Tile.Other;

		GameObject o = Instantiate(Powerplant.PowerplantPrefab) as GameObject;
		o.transform.position = new Vector3(where.x, where.y, o.transform.position.z);
		Powerplant p = o.GetComponent<Powerplant>();
		return p;
	}

	//Used to deconstruct powerplants
	public void EmptyTile(Vector2 where) {
		Vector2 imag = GetImagWorldCoordinates((int)where.x, (int)where.y);
		terraingrid[(int)imag.y, (int)imag.x] = Tile.Empty;
	}

	public void ChangeTile(Vector2 where, Tile type) {
		Vector2 imag = GetImagWorldCoordinates((int)where.x, (int)where.y);
		terraingrid[(int)imag.y, (int)imag.x] = type;
	}

	//Find the closes powerstation that actually has power and returns the road it's on, otherwise null is returned
	public PowerStationRoad FindNearestPoweredPowerstation(Vector2 position) {
		float leastDistance = Mathf.Infinity;
		PowerStation closest = null;
		foreach (PowerStation p in powerStations) {
			float thisdist = position.Distance(p.transform.position.ToVector2());
			//Do not return unpowered stations
			if (thisdist < leastDistance && p.Powered) {
				leastDistance = thisdist;
				closest = p;
			}
		}
		if (closest == null)
			return null;
		return (PowerStationRoad)closest.road;
	}

	//nearest road to a point in 2d space
	public Road FindNearestRoad(Vector2 position) {
		float leastDistance = Mathf.Infinity;
		Road closest = null;
		foreach (Road p in roadmap.roads) {
			float thisdist = position.Distance(p.transform.position.ToVector2());
			if (thisdist < leastDistance) {
				leastDistance = thisdist;
				closest = p;
			}
		}
		return closest;
	}

	//Deprecated (originally used for testing - would still work with a revision)
	public void SpawnCars(int howmany) {
		if (vehicles == null)
			vehicles = new List<Car>();
		//map.roads.Count
		int toSpawn = howmany;
		int spawned = 0;
		float prob = 0.02f;
		for (int i = 0; spawned < toSpawn; i++) {
			if (AllParkingOccupied()) {
				return;
			}
			foreach (ParkingSpace parking in parkingSpots) {
				if (UnityEngine.Random.Range(0f, 1f) < prob && !parking.occupied) {
					Car c = CarFactory.CreateCar(parking.road);
					vehicles.Add(c);
					spawned++;
				}
			}
		}
	}
	
	bool AllParkingOccupied() {
		foreach (ParkingSpace parking in parkingSpots) {
			if (parking.occupied == false) {
				return false;
			}
		}
		return true;
	}

	void Start() {
		if (LevelManager.properties is null) {
			Debug.Log("Level properties are not loaded.");
			SetTime(new TimeClass("0800"));
		} else {
			SetTime(LevelManager.properties.DayStart);
		}
		
		Game.Instance.WorldReady();
	}

	void FixedUpdate () {
		//add time
		if (Game.Instance.phase == Game.Phase.Simulation)
			time.time.AddSeconds(1 * timescale);

	}

	//time since last fixed update - obviously, that is exactly timescale
	public int deltaTime() {
		if (Game.Instance.phase == Game.Phase.Simulation)
			return 1 * timescale;
		return 0;
	}

	public void SetTime(TimeClass t) {
		time.time = new TimeClass(t);
	}

	#region light

	public void SetDaylight() {
		lightByTime = false;
		if (sun == null) {
			sun = FindObjectOfType<Light>();
		}
		sun.intensity = 0.63f;
	}

	public void SetLightByTime() {
		lightByTime = true;
	}

	void Update() {
		//Change light
		if (lightByTime)
			SetLight(time.time);
	}

	void SetLight(TimeClass time) {
		if (sun == null) {
			sun = FindObjectOfType<Light>();
		}
		float daylight = 0.63f;
		float nightlight = 0.3f;
		//dawn
		if (time.hour >= 6 && time.hour < 7) {
			float point = time.hour + ((float)time.minute)/60;
			sun.intensity = nightlight + ((point-6) * (daylight - nightlight));
		//sunset
		} else if (time.hour >= 19 && time.hour < 20) {
			float point = time.hour + ((float)time.minute)/60;
			sun.intensity = daylight - ((point-19) * (daylight - nightlight));
		//daytime
		} else if (time.hour >= 7 && time.hour <= 19) {
			sun.intensity = daylight;
		//nighttime
		} else {
			sun.intensity = nightlight;
		}
	}

	#endregion

	public void CreateTerrain(string terrainFile) {
		creator.CreateTerrain(terrainFile);
	}

	public void CreateWorld(string worldFile) {
		creator.CreateWorld(worldFile);
	}

	public void SpawnCars(string carsFile) {
		creator.SpawnCars(carsFile);
	}

	public void SurroundWorld() {
		creator.SurroundWorldWithWater(50);
	}

	//convert grid coords to world coords
	public Vector2 GetRealWorldCoordinates(int imagX, int imagY) {
		int xStart = -WorldSize.First/2;
		int yStart = WorldSize.Second/2;
		
		return new Vector2(imagX+ xStart, yStart - imagY);
	}

	//convert world coords to grid coords
	public Vector2 GetImagWorldCoordinates(int realX, int realY) {
		int xStart = -WorldSize.First/2;
		int yStart = WorldSize.Second/2;
		
		return new Vector2(realX - xStart, yStart - realY);
	}

}
