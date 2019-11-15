using UnityEngine;
using System.Collections;

public class BuildingFactory : MonoBehaviour {

	private static GameObject house;
	private static GameObject Houseprefab {
		get {if (house == null) {
				house = Resources.Load("House") as GameObject;
			} return house;
		}
	}

	public static Building CreateFromStub(BuildingStub stub) {
		GameObject prefab = null;

		switch (stub.type) {
		case BuildingStub.BuildingType.PowerStation : {
			//deprecated
			break;
		}
		case BuildingStub.BuildingType.Decoration : {
			prefab = Houseprefab;
			break;
		}
		}

		if (prefab == null) {
			return null;
		}

		GameObject building = (GameObject) Instantiate(prefab);
		building.transform.position = new Vector3(stub.xPos, stub.yPos, building.transform.position.z);

		World w = World.FindObjectOfType<World>();
		w.ChangeTile(building.transform.position.ToVector2(), Tile.Building);

		return building.GetComponent<Building>();
	}

	//Instantiate the decoration at x,y
	public static Building SpawnDecoration(int x, int y) {
		GameObject prefab = Houseprefab;
		GameObject building = (GameObject) Instantiate(prefab);
		building.transform.position = new Vector3(x, y, building.transform.position.z);
		return building.GetComponent<Building>();
	}

}
