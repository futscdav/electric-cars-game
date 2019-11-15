using UnityEngine;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SimpleJSON;

public class WorldCreator : MonoBehaviour {

	public World world;

	public WorldCreator(World world) {
		this.world = world;
	}

	//Also deprecated
	public void GenerateWorld(int x, int y) {
		int xStart = -x/2;
		int xEnd = x/2;
		int yStart = y/2;
		int yEnd = -y/2;
		world.roadmap = new RoadMap();
		world.roadmap.GenerateRandomMap(xStart,xEnd,yStart,yEnd);
		
		world.SpawnCars(3);
	}
	
	public Vector2 GetRealWorldCoordinates(int imagX, int imagY) {
		int xStart = -world.WorldSize.First/2;
		int yStart = world.WorldSize.Second/2;
		
		return new Vector2(imagX+ xStart, yStart - imagY);
	}
	
	//Creates the world from the resource asset at path levelName
	public void CreateWorld(string levelName) {
		string[] lines = ReadLines(levelName);
		RoadStub[,] roadstubs = new RoadStub[lines.Length,lines[0].Length];
		BuildingStub[,] buildingstubs = new BuildingStub[lines.Length, lines[0].Length];
		world.terraingrid = new Tile[lines.Length, lines[0].Length];
		
		//Read file line by line and construct the map
		world.WorldSize = new Pair<int, int>(lines[0].Length, lines.GetLength(0));

		for (int y = 0; y < lines.GetLength(0); y++) {
			for (int x = 0; x < lines[0].Length; ++x) {
				switch(lines[y][x]) {
				case '1': {
					roadstubs[y, x] = new RoadStub(x, y);
					break;
				}
				case '2' : {
					roadstubs[y, x] = new RoadStub(x ,y);
					roadstubs[y, x].type = RoadStub.RoadType.ChargingSpot;
					break;
				}
				case 'P' : {
					roadstubs[y, x] = new RoadStub(x, y) ;
					roadstubs[y, x].type = RoadStub.RoadType.ParkingSpot;
					break;
				}
				case 'E' : {
					buildingstubs[y, x] = new BuildingStub(x, y, BuildingStub.BuildingType.PowerPlant);
					world.BuildPowerplant(world.GetRealWorldCoordinates(x, y));
					break;
				}
				case 'B' : {
					buildingstubs[y, x] = new BuildingStub(x, y, BuildingStub.BuildingType.Decoration);
					break;
				}
				}
			}
		}
		world.roadmap = new RoadMap();
		world.roadmap.CreateMap(roadstubs);
		
		world.buildings = new BuildingManager();
		world.buildings.Build(buildingstubs);
	}

	//Creates terrain from resource asset at path terrainFile
	public void CreateTerrain(string terrainFile) {
		string[] lines = ReadLines(terrainFile);
		
		//Read file line by line and construct the map
		//size should be set by CreateWorld
		//world.WorldSize = new Pair<int, int>(lines[0].Length, lines.GetLength(0));

		for (int y = 0; y < lines.GetLength(0); y++) {
			for (int x = 0; x < lines[0].Length; ++x) {
				switch(lines[y][x]) {
				case 'W': {
					world.terraingrid[y, x] = Tile.Water;
					break;
				}
				case 'S' : {
					world.terraingrid[y, x] = Tile.Sidewalk;
					break;
				}
				}
			}
		}

		//assemble the actual terrain
		TerrainAssembler.AssembleTerrain(world.terraingrid, world);
	}
	
	private string[] ReadLines(string source) {
		TextAsset text = Resources.Load(source) as TextAsset;
		return Regex.Split(text.text, "\r\n|\r|\n");
	}

	private string ReadText(string source) {
		TextAsset text = Resources.Load(source) as TextAsset;
		if (text == null) {
			Debug.Log(source + " not found");
		}
		return text.text;
	}

	
	//Spawns cars from the resource asset at path levelName
	public void SpawnCars(string levelName) {
		string file = ReadText(levelName);
		JSONNode objects = JSON.Parse(file);
		JSONArray a = objects["cars"].AsArray;
		for (int i = 0; i < a.Count; ++i) {
			JSONNode thiscar = a[i];
			ParkingSpace parking = FindParkingSpace(thiscar["x"].AsInt, thiscar["y"].AsInt);
			if (parking == null || parking.road == null) {
				Debug.Log("null");
			}
			Car c = CarFactory.CreateCar(parking.road);
			if (thiscar["tank"] != null) {
				c.tankLevel = thiscar["tank"].AsFloat;
			}
			c.plan.Trips = ReadDestinations(thiscar);
			world.vehicles.Add(c);
		}
	}
	
	//Reads and creates the destinations from the json node given
	private List<Trip> ReadDestinations(JSONNode car) {
		JSONArray destinations = car["trips"].AsArray;
		List<Trip> trips = new List<Trip>();
		ParkingSpotRoad lastDest = FindParkingRoad(car["x"].AsInt, car["y"].AsInt);
		for (int i = 0; i < destinations.Count; ++i) {
			JSONNode thisdest = destinations[i];
			ParkingSpotRoad p = FindParkingRoad(thisdest["x"].AsInt, thisdest["y"].AsInt);
			
			Trip trip = new Trip();
			trip.start = lastDest;
			trip.end = p;
			trip.type = Trip.TripType.Park;
			trip.departure = new TimeClass(DateTime.ParseExact(thisdest["departure"], "HHmm", CultureInfo.InvariantCulture));
			trips.Add(trip);
			
			lastDest = p;
		}
		return trips;
	}

	//pretty much equal to world.findnearestroad
	private Road FindRoad(int imagX, int imagY) {
		var real = GetRealWorldCoordinates(imagX, imagY);
		foreach (Road r in world.roadmap.roads) {
			if (r.AsVector().AlmostEqual(real)) {
				return r;
			}
		}
		Debug.LogError("Road " + real + " not found!");
		return null;
	}
	
	private ParkingSpotRoad FindParkingRoad(int imagX, int imagY) {
		var road = FindRoad(imagX, imagY);
		if (!(road is ParkingSpotRoad)) {
			Debug.Log("Wrong coordinates: " + imagX + " " + imagY);
			return null;
		}
		return (ParkingSpotRoad)road;
	}

	//this was an experiment, but it uses too many resources
	public void SpawnSurroundingBuildings(int depth) {
		Rect box = world.GetBoundingBox();
		Rect surrbox = box;
		surrbox.width += 2*depth;
		surrbox.height += 2*depth;
		surrbox.x -= depth;
		surrbox.y += depth;

		float yMax = surrbox.yMin - surrbox.height;

		Debug.Log("Spawning in " + surrbox + " vs " + box);
		//4 sides
		for (int i = (int)surrbox.xMin; i < surrbox.xMax; ++i) {
			for (int j = (int)surrbox.yMin; j > yMax; --j) {
				if (i >= box.x && i <= box.x + box.width && j <= box.y && j >= box.y - box.height) {
					continue;
				}
				BuildingFactory.SpawnDecoration(i, j);
			}
		}
	}

	//This beast was created as an experiment too, but I ended up sticking with it
	//It basically creates textured quad meshes around the world that sort of just
	//look good. Also creates the neat transition (which is a simple tex), this 
	//could be somehow done nativelly, but I have no idea how
	public void SurroundWorldWithWater(float depth) {
		Rect box = world.GetBoundingBox();
		Rect surrbox1 = box;
		surrbox1.center -= Vector2.right * box.width/2;
		float width = depth;
		float height = box.height + 2* depth;
		float offset =  1 + 0.5f;
		Vector3 center = new Vector3(box.xMin - depth/2 - offset, 0, -0.01f);

		Texture2D shoretex = Resources.Load("shoretex") as Texture2D;
		Texture2D watertex = Resources.Load("waterztex") as Texture2D;
		Material watermat = Resources.Load("watermat") as Material;
		Material shoremat = Resources.Load("shoremat") as Material;
		Shader diffuse = Shader.Find("Diffuse");
		Shader transparent = Shader.Find("Transparent/Diffuse");

		CreatePlane(width, height, center, watertex, watermat, diffuse);

		GameObject shore = CreatePlane(height, 1, new Vector3(box.xMin - 1.01f, 0, -0.001f), shoretex, shoremat, transparent, 1, 1);
		shore.transform.Rotate(0, 0, 90);

		center = new Vector3(box.xMax + depth/2 + offset, 0, -0.01f);
		CreatePlane(width, height, center, watertex, watermat, diffuse);
		shore = CreatePlane(height, 1, new Vector3(box.xMax + 1.01f, 0, -0.001f), shoretex, shoremat, transparent, 1, 1);
		shore.transform.Rotate(0, 0, 270);

		width = box.width + 2*depth;
		height = depth;
		center = new Vector3(0, box.yMin + depth/2 + offset, -0.01f);
		CreatePlane(width, height, center, watertex, watermat, diffuse);

		shore = CreatePlane (width, 1, new Vector3(0, box.yMin + 1.01f, -0.001f), shoretex, shoremat, transparent, 1, 1);
		shore.transform.Rotate(0, 0, 180);

		center = new Vector3(0, box.yMin - depth/2 - box.height - offset, -0.01f);
		CreatePlane(width, height, center, watertex, watermat, diffuse);
		shore = Instantiate(shore) as GameObject;
		shore.transform.position = new Vector3(0, box.yMin - box.height - 1.01f, - 0.001f);
		shore.transform.Rotate(0, 0, 180);
	}
	
	private GameObject CreatePlane(float width, float height, Vector3 center, Texture2D texture, Material material, Shader shader, int xtile = 0, int ytile = 0) {
		if (xtile == 0) {
			xtile = (int)width;
		}
		if (ytile == 0) {
			ytile = (int)height;
		}

		Mesh m = new Mesh();
		m.vertices = new Vector3[] {
			new Vector3(-width/2, -height/2, 0.01f),
			new Vector3(width/2, -height/2, 0.01f),
			new Vector3(width/2, height/2, 0.01f),
			new Vector3(-width/2, height/2, 0.01f)
		};
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (0, 1),
			new Vector2(1, 1),
			new Vector2 (1, 0)
		};
		m.triangles = new int[] { 0, 1, 2, 0, 2, 3};
		m.RecalculateNormals();
		
		GameObject plane = new GameObject("Plane");
		MeshFilter filter = plane.AddComponent<MeshFilter>();
		filter.mesh = m;
		MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
		if (material != null)
			renderer.material = material;
		renderer.material.shader = shader;
		if (texture != null)
			renderer.material.mainTexture = texture;
		
		plane.transform.position += center;
		plane.transform.Rotate(180,0,0);
		//renderer.lightmapTilingOffset = new Vector4(width, 0, height, 0);
		renderer.material.mainTextureScale = new Vector2(xtile, ytile);

		return plane;
	}
	
	private ParkingSpace FindParkingSpace(int imagX, int imagY) {
		var real = GetRealWorldCoordinates(imagX, imagY);
		foreach (ParkingSpace p in world.parkingSpots) {
			if (p.road.AsVector().AlmostEqual(real)) {
				return p;
			}
		}
		Debug.LogError("Parking space " + real + " not found!");
		return null;
	}

}
