using UnityEngine;
using System.Collections;

public class TerrainAssembler : MonoBehaviour {

	/* Class to take care of the terrain assembly */

	private static GameObject waterStraight = Resources.Load("SpriteWaterStraight") as GameObject;
	private static GameObject waterCurved = Resources.Load("SpriteWaterCurved") as GameObject;
	private static GameObject waterPond = Resources.Load("SpriteWaterPond") as GameObject;
	private static GameObject sidewalk = Resources.Load("Sidewalk") as GameObject;
	private static GameObject crossing = Resources.Load("Crossing2") as GameObject;

	public static void AssembleTerrain(Tile[,] terrain, World reference) {

		for (int y = 0; y < terrain.GetLength(0); y++) {
			for (int x = 0; x < terrain.GetLength(1); x++) {
				if (terrain[y, x] != Tile.Water && terrain[y ,x] != Tile.Sidewalk) {
					continue;
				}
				bool down  = false;
				bool up    = false;
				bool left  = false;
				bool right = false;
				int numAround = 0;
				
				try {if (terrain[y, x+1] == terrain[y, x]) {
						numAround++; right = true;}}
				catch (System.IndexOutOfRangeException) {}
				try {if (terrain[y, x-1] == terrain[y, x]) {
						numAround++; left = true;}}
				catch (System.IndexOutOfRangeException) {}
				try {if (terrain[y+1, x] == terrain[y, x]) {
						numAround++; down = true;}}
				catch (System.IndexOutOfRangeException) {}
				try {if (terrain[y-1, x] == terrain[y, x]) {
						numAround++; up = true;}}
				catch (System.IndexOutOfRangeException) {}

				GameObject obj = null;

				if (terrain[y, x] == Tile.Sidewalk) {
					obj = SpawnSidewalk(up, down, left, right);
					Vector2 pos = reference.GetRealWorldCoordinates(x, y);
					if (((left && right) || (up && down)) && reference.FindNearestRoad(pos).AsVector().AlmostEqual(pos)) {
						if (!(reference.FindNearestRoad(pos) is ParkingSpotRoad)) {
							GameObject cross = Instantiate(crossing) as GameObject;
							if (up && down) {
								cross.transform.localEulerAngles = Vector3.forward * 90;
							}
							cross.transform.parent = obj.transform;
							cross.transform.localPosition -= Vector3.forward * 0.03f;
						}
					}
				}
				if (terrain[y, x] == Tile.Water) {
					obj = SpawnWater(up, down, left, right);
				}

				obj.transform.position = reference.GetRealWorldCoordinates(x, y).ToVector3();
			}
		}
	}

	private static GameObject SpawnSidewalk(bool up, bool down, bool left, bool right) {
		int numAround = Utils.CountBools(up, down, left, right);

		GameObject sw = null;

		/* 
		 * Without going into much detail, the sidewalk is created by using 1 or 2 objects
		 * that overlap if necessary.
		 * */

		switch (numAround) {
		case 0 : {
			sw = Instantiate(sidewalk) as GameObject;
			break;
		}
		case 1 : {
			sw = Instantiate(sidewalk) as GameObject;
			Vector3 scale = sw.transform.localScale;
			if (left || right) {
				scale.x *= 2;
			} else {
				scale.y *= 2;
			}
			sw.transform.localScale = scale;
			break;
		}
		case 2 : {
			sw = Instantiate(sidewalk) as GameObject;
			Vector3 scale = sw.transform.localScale;
			if (left && right) {
				scale.x *= 2;
			} else if (up && down) {
				scale.y *= 2;
			} else {
				GameObject sw2 = Instantiate(sidewalk) as GameObject;
				sw2.transform.parent = sw.transform;
				sw2.transform.localPosition = Vector3.up;
				if (down) {
					sw2.transform.localPosition -= Vector3.up*2;
				}
				GameObject sw3 = Instantiate(sidewalk) as GameObject;
				sw3.transform.parent = sw.transform;
				sw3.transform.localPosition = -Vector3.right*2;
				if (right) {
					sw3.transform.localPosition += Vector3.right*4;
				}
			}
			sw.transform.localScale = scale;
			break;
		}

		case 3 : {
			GameObject sw2 = null;
			if (up && down) {
				sw = SpawnSidewalk(true, true, false, false);
				if (left) {
					sw2 = SpawnSidewalk(false, false, true, false);
					sw2.transform.parent = sw.transform;
					sw2.transform.localPosition = -Vector3.right * 1.3f;
				} else {
					sw2 = SpawnSidewalk(false, false, false, true);
					sw2.transform.parent = sw.transform;
					sw2.transform.localPosition = Vector3.right * 1.3f;
				}
			} else {
				sw = SpawnSidewalk(false, false, true, true);
				if (up) {
					sw2 = SpawnSidewalk(true, false, false, false);
					sw2.transform.parent = sw.transform;
					sw2.transform.localPosition = Vector3.up;
				} else {
					sw2 = SpawnSidewalk(false, true, false, false);
					sw2.transform.parent = sw.transform;
					sw2.transform.localPosition = -Vector3.up;
				}
			}

			break;
		}
		case 4 : {
			sw = SpawnSidewalk(true, true, false, false);
			GameObject sw2 = SpawnSidewalk(false, false, true, true);
			sw2.transform.parent = sw.transform;
			break;
		}
		}

		return sw;
	}

	private static GameObject SpawnWater(bool up, bool down, bool left, bool right) {
		GameObject water = null;
		int numAround = Utils.CountBools(up, down, left, right);
		
		switch (numAround) {
		case 0 : {
			water = Instantiate(waterPond) as GameObject;
			break;
		} 
		case 1 :
		case 2 : {
			water = CreateWater2(up, down, left, right);
			break;
		}
		default : {
			throw new System.ArgumentException("Unexpected water placement. Water tiles around: " + numAround);
		}
		}
		return water;
	}

	private static GameObject CreateWater2(bool up, bool down, bool left, bool right) {
		if (up && right) {
			return SpawnWaterCurved(0);
		} else if (up && left) {
			return SpawnWaterCurved(90);
		} else if (down && right) {
			return SpawnWaterCurved(270);
		} else if (down && left) {
			return SpawnWaterCurved(180);
		} else if (up || down) {
			return SpawnWaterStraight(90);
		} else if (left || right) {
			return SpawnWaterStraight(0);
		}
		return null;
	}

	private static GameObject SpawnWaterStraight(float z) {
		GameObject water = Instantiate(waterStraight) as GameObject;
		water.transform.Rotate(new Vector3(0, 0, z));
		return water;
	}

	private static GameObject SpawnWaterCurved(float z) {
		GameObject water = Instantiate(waterCurved) as GameObject;
		water.transform.Rotate(new Vector3(0, 0, z));
		return water;
	}

}
