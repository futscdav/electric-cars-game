using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class RoadMap {

	public List<Road> roads;
	public Road[,] roadGrid;

	public RoadMap() {
		roads = new List<Road>();
	}

	/* I have a feeling that at some point, whoever dares come here, is on the quest to
		make roads useable when next to each other without connecting to each other. To achieve that,
		have a look at ChooseSprite and ConnectNetwork functions. Of course, first you have to know which
		roads are supposed to be which, which is exactly why RoadStub was created in the first place. 
		In any case, good luck. You will need it. */

	#region deprecated

	private static float crossroadProbability = 0.05f;

	public void GenerateRandomMap(int xStart, int xEnd, int yStart, int yEnd) {
		//generate crossroads
		RoadStub[,] crossroads = GenerateCrossroads(xStart, xEnd, yStart, yEnd);
		//link crossroads together
		crossroads = LinkCrossroads(crossroads);
		//choose correct sprite for each roadpiece and connect them together
		roads = ChooseSprites(crossroads, xStart, xEnd, yStart, yEnd);
	}

	private RoadStub[,] LinkCrossroads(RoadStub[,] crossroads) {
		List<RoadStub> crossroadsList = new List<RoadStub>();
		
		//enumerate crossroads
		for (int i = 0; i < crossroads.GetLength(0); ++i) {
			for (int j = 0; j < crossroads.GetLength(1) ;++j) {
				if (crossroads[i ,j] != null) {
					crossroadsList.Add(crossroads[i, j]);
				}
			}
		}
		
		//create pairs of crossroads
		List<Pair<RoadStub, RoadStub>> crossroadPairs = CreatePairs(crossroadsList);
		
		//connect the pairs
		foreach(Pair<RoadStub, RoadStub> pair in crossroadPairs) {
			ConnectPair(crossroads, pair);
		}
		
		return crossroads;
	}
	
	private void ConnectPair(RoadStub[,] roads, Pair<RoadStub, RoadStub> crossroadPair) {
		//Choose randomly the "manhattan" direction
		bool firstx = Random.Range(0f,1f) < 0.5f;
		
		if (firstx) {
			ConnectManhattan(roads, crossroadPair.First, crossroadPair.Second);
		} else {
			ConnectManhattan(roads, crossroadPair.Second, crossroadPair.First);
		}
	}
	
	private void ConnectManhattan(RoadStub[,] roads, RoadStub first, RoadStub second) {
		//go until first.x == second.x
		int mulx = first.xPos < second.xPos ? 1 : -1;
		int muly = first.yPos < second.yPos ? 1 : -1;
		
		//connect x axis
		int y = first.yPos;
		int x = first.xPos;
		for (; x != second.xPos; x += mulx) {
			if (roads[y, x] == null) {
				roads[y, x] = new RoadStub(x, y);
			}
		}
		//connect y axis
		for (; y != second.yPos; y += muly) {
			if (roads[y, x] == null) {
				roads[y, x] = new RoadStub(x, y);
			}
		}
	}

	private List<Pair<RoadStub, RoadStub>> CreatePairs(List<RoadStub> crossroads) {
		List<Pair<RoadStub, RoadStub>> pairs = new List<Pair<RoadStub, RoadStub>>();
		
		for (int i = 0; i < crossroads.Count-1; ++i) {
			pairs.Add(new Pair<RoadStub, RoadStub>(crossroads[i], crossroads[i+1]));
		}
		pairs.Add(new Pair<RoadStub, RoadStub>(crossroads[crossroads.Count-1], crossroads[0]));
		return pairs;
	}

	private RoadStub[,] GenerateCrossroads(int xStart, int xEnd, int yStart, int yEnd) {
		RoadStub[,] grid = new RoadStub[xEnd-xStart, yStart-yEnd];
		int xDimen = xEnd-xStart;
		int yDimen = yStart-yEnd;
		
		Debug.Log ("Map size: x=" + xDimen + ", y=" + yDimen);
		int crossroads = 0;
		
		/* Generate road stub in a random place.
		 * This does no heavy lifting. The LinkCrossroads method is then
		 * responsible for connecting the roads.
		 */
		for (int i = 0; i < yDimen; ++i) {
			for (int j = 0; j < xDimen; ++j) {
				float roll = Random.Range(0f,1f);
				if (roll < crossroadProbability) {
					//Debug.Log("Crossroad at x=" + (j+xStart) + ", y=" + (yStart-i) + " (roll=" + roll + ")");
					grid[i, j] = new RoadStub(j, i);
					crossroads++;
				}
			}
		}
		///Should check whether there are at least 2-3 crossroads
		if (crossroads < 3)
			return GenerateCrossroads(xStart, xEnd, yStart, yEnd);
		return grid;
	}

	#endregion

	//removing road from the outside, this is called when the user decides to remove an already placed
	//powerstation (has to be removed from the roadmap)
	public void RemoveRoad(Road r) {
		for (int i = 0; i < roads.Count; ++i) {
			if (roads[i].AsVector().AlmostEqual(r.AsVector())) {
				roads.Remove(roads[i]);
				break;
			}
		}
		Vector2 imag = World.FindObjectOfType<World>().GetImagWorldCoordinates(r.xPos, r.yPos);
		roadGrid[(int)imag.y, (int)imag.x] = null;
	}

	//registering road created outside of this class
	public void RegisterRoad(Road r) {
		roads.Add(r);
		Vector2 imag = World.FindObjectOfType<World>().GetImagWorldCoordinates(r.xPos, r.yPos);
		roadGrid[(int)imag.y, (int)imag.x] = r;
	}

	public PowerStationRoad BuildPowerstation(Buildable station, Vector2 position, World reference) {
		Road built = null;
		Vector2 toFix;

		switch(station) {
		case Buildable.Powerstation_left : {
			built = RoadFactory.CreateChargeSpotLeft((int)position.x, (int)position.y);
			toFix = position - Vector2.right;
			break;
		}
		case Buildable.Powerstation_right : {
			built = RoadFactory.CreateChargeSpotRight((int)position.x, (int)position.y);
			toFix = position + Vector2.right;
			break;
		}
		case Buildable.Powerstation_down : {
			built = RoadFactory.CreateChargeSpotDown((int)position.x, (int)position.y);
			toFix = position - Vector2.up;
			break;
		}
		case Buildable.Powerstation_up : {
			built = RoadFactory.CreateChargeSpotUp((int)position.x, (int)position.y);
			toFix = position + Vector2.up;
			break;
		}
		default : {
			throw new System.ArgumentException();
		}
		}
	
		//get grid position
		Vector2 imagpos = reference.GetImagWorldCoordinates((int)position.x, (int)position.y);
		Vector2 imagfix = reference.GetImagWorldCoordinates((int)toFix.x, (int)toFix.y);

		//Add new road, destroy old neighbouring road and create new neighbouring road, then ling everything together again
		roadGrid[(int)imagpos.y, (int)imagpos.x] = built;
		roads.Remove(roadGrid[(int)imagfix.y, (int)imagfix.x]);
		//Debug.Log(roadGrid[(int)imagfix.y, (int)imagfix.x]);
		World.Destroy(roadGrid[(int)imagfix.y, (int)imagfix.x].gameObject);
		roadGrid[(int)imagfix.y, (int)imagfix.x] = CreateRoad((int)imagfix.x, (int)imagfix.y, (int)toFix.x, (int)toFix.y);
		Road fixd = roadGrid[(int)imagfix.y, (int)imagfix.x];

		roadGrid[(int)imagpos.y, (int)imagpos.x] = null;
		CreateRoadNetwork(roadGrid);
		roadGrid[(int)imagpos.y, (int)imagpos.x] = built;
		built.neighbourRoads.Add (fixd);
		fixd.neighbourRoads.Add(built);

		roads.Add(built);
		roads.Add(fixd);

		return built as PowerStationRoad;
	}

	private Road CreateRoad(int gridx, int gridy, int mapx, int mapy) {
		bool down  = false;
		bool up    = false;
		bool left  = false;
		bool right = false;
		int numAround = 0;
		
		try {if (roadGrid[gridy, gridx+1] != null) {
				numAround++; right = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roadGrid[gridy, gridx-1] != null) {
				numAround++; left = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roadGrid[gridy+1, gridx] != null) {
				numAround++; down = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roadGrid[gridy-1, gridx] != null) {
				numAround++; up = true;}}
		catch (System.IndexOutOfRangeException) {}
		
		Road result = null;
		switch (numAround) {
		case 0: {
			throw new System.ArgumentException();
		}
		case 1: {
			throw new System.ArgumentException("1 road around fixee?");
		}
		case 2: {
			result = RoadFactory.CreateTwoSided(mapx, mapy, up, down, left, right);
			break;
		}
		case 3: {
			result = RoadFactory.CreateT(mapx, mapy, up, down, left, right);
			break;
		} 
		case 4: {
			result = RoadFactory.CreateQCrossroad(mapx, mapy);
			break;
		}
		}
		return result;

	}

	public void CreateMap(RoadStub[,] stubs) {
		int xStart = -stubs.GetLength(1)/2;
		int xEnd = -xStart;
		int yStart = stubs.GetLength(0)/2;
		int yEnd = -yStart;
		roads = ChooseSprites(stubs, xStart, xEnd, yStart, yEnd);
	}

	private List<Road> ChooseSprites(RoadStub[,] roads, int xStart, int xEnd, int yStart, int yEnd) {
		List<Road> roadmap = new List<Road>();
		Road[,] roadGrid = new Road[roads.GetLength(0),roads.GetLength(1)];

		//Choose sprites
		for (int y = 0; y < roads.GetLength(0); ++y) {
			for (int x = 0; x < roads.GetLength(1); ++x) {
				if (roads[y, x] != null){
					roadGrid[y, x] = ChooseSprite(x, y, roads, xStart+x, yStart-y);
				}
			}
		}

		//Connect roads in terms of neighbors
		CreateRoadNetwork(roadGrid);

		this.roadGrid = roadGrid;

		//Fold all roads into a list
		for (int y = 0; y < roads.GetLength(0); ++y) {
			for (int x = 0; x < roads.GetLength(1); ++x) {
				if (roadGrid[y, x] != null) 
					roadmap.Add(roadGrid[y, x]);
			}
		}
		return roadmap;
	}

	private void CreateRoadNetwork(Road[,] roads) {
		for (int y = 0; y < roads.GetLength(0); ++y) {
			for (int x = 0; x < roads.GetLength(1); ++x) {
				Road r = roads[y, x];
				if (r == null)
					continue;

				//simply check neighbour cells and add neighbours
				try {if (roads[y, x+1] != null) 
						r.neighbourRoads.Add(roads[y, x+1]);}
				catch (System.IndexOutOfRangeException) {}
				try {if (roads[y, x-1] != null) 
						r.neighbourRoads.Add(roads[y, x-1]);}
				catch (System.IndexOutOfRangeException) {}
				try {if (roads[y+1, x] != null) 
					r.neighbourRoads.Add(roads[y+1, x]);}
				catch (System.IndexOutOfRangeException) {}
				try {if (roads[y-1, x] != null)
					r.neighbourRoads.Add(roads[y-1, x]);}
				catch (System.IndexOutOfRangeException) {}
			}
		}

	}
	
	private Road ChooseSprite(int x, int y, RoadStub[,] roads, int mapX, int mapY) {
		//check surrounding

		bool down  = false;
		bool up    = false;
		bool left  = false;
		bool right = false;
		int numAround = 0;

		try {if (roads[y, x+1] != null) {
				numAround++; right = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roads[y, x-1] != null) {
				numAround++; left = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roads[y+1, x] != null) {
				numAround++; down = true;}}
		catch (System.IndexOutOfRangeException) {}
		try {if (roads[y-1, x] != null) {
				numAround++; up = true;}}
		catch (System.IndexOutOfRangeException) {}

		Road result = null;
		RoadStub stub = roads[y, x];

		//Choose correct road
		//Could just use RoadFactory.CreateGeneralRoad, but at the time of writing this,
		//that function did not exist
		switch (numAround) {
			case 0: {
				throw new System.ArgumentException();
			}
			case 1: {
				if (stub.type == RoadStub.RoadType.ChargingSpot) {
					result = RoadFactory.CreateChargeSpot(mapX, mapY, up, down, left, right);
				} else if (stub.type == RoadStub.RoadType.Regular) {
					result = RoadFactory.CreateEnd(mapX, mapY, up, down, left, right);
				} else if (stub.type == RoadStub.RoadType.ParkingSpot) {
					result = RoadFactory.CreateParkingSpot(mapX, mapY, up, down, left, right);
				}
				break;
			}
			case 2: {
				result = RoadFactory.CreateTwoSided(mapX, mapY, up, down, left, right);
				break;
			}
			case 3: {
				result = RoadFactory.CreateT(mapX, mapY, up, down, left, right);
				break;
			} 
			case 4: {
				result = RoadFactory.CreateQCrossroad(mapX, mapY);
				break;
			}
		}
		return result;
	}
}
