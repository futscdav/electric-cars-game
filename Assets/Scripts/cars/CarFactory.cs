using UnityEngine;
using System.Collections.Generic;

public class CarFactory : MonoBehaviour {
	
	public static GameObject car1Prefab;
	public static List<Sprite> possibleTextures;

	private static void LoadTextures() {
		possibleTextures = new List<Sprite>();
		possibleTextures.Add(Resources.Load<Sprite>("Car"));
		possibleTextures.Add(Resources.Load<Sprite>("Car3"));
		possibleTextures.Add(Resources.Load<Sprite>("Car4"));
	}

	private static void FaceCarInDirection(Car c, Vector2 carDirection) {
		c.directionHeading = carDirection;
		Vector3 originalPos = c.transform.position;
		//reposition the car at the origin
		//Note: there is a better way!
		c.transform.position = Vector3.zero;
		//Look at 0,0,0 would be looking left (<-), 0,0,1 is looking up
		c.transform.LookAt(new Vector3(0, 0, 1));
		//only works for singluarities!!
		c.transform.Rotate(new Vector3(0, 0, 180 + 180f*carDirection.y - 270f*carDirection.x));
		c.transform.position = originalPos;
	}

	private static void RotateCarByAngle(Car c, float angle) {
		//Debug.Log("Rotate z=" + c.transform.rotation.z + " by " + angle);
		c.directionHeading = Quaternion.AngleAxis(angle, Vector2.up) * c.directionHeading;
		c.transform.Rotate(0,0,angle);
	}
	
	private static void PlaceCarOnNarrowRoad(Car c, NarrowRoad r) {
		//road orientation, car orientation
		float carZ = c.transform.rotation.eulerAngles.z;
		//road can only be left or up
		bool roadUp = r.up;

		if (roadUp) {
			if (carZ >= 90 && carZ <= 270) {
				//going down
				FaceCarInDirection(c, new Vector2(0, -1));
			} else {
				//going up
				FaceCarInDirection(c, new Vector2(0, +1));
			}
		}
		else {
			if (carZ >= 0 && carZ <= 180) {
				//going left
				FaceCarInDirection(c, new Vector2(-1, 0));
			} else {
				//going right
				FaceCarInDirection(c, new Vector2(+1, 0));
			}
		}
	}

	//not quite fully functional
	private static void PlaceCarOnCurvedRoad(Car c, CurvedRoad r) {
		FaceCarInDirection(c, new Vector2(0.5f, 0.5f));
		RotateCarByAngle(c, r.transform.rotation.eulerAngles.z);
	}

	private static void PlaceCarOnUnknownRoad(Car c, Road r) {
		Debug.LogWarning("Unknown road type.");
	}

	private static void PlaceCarOnRoad(Car c, Road r) {
		c.road = r;
		
		switch (r.GetType().ToString()) {
		case "NarrowRoad" : {
			PlaceCarOnNarrowRoad(c, (NarrowRoad)r);
			break;
		}
		case "CurvedRoad" : {
			PlaceCarOnCurvedRoad(c, (CurvedRoad)r);
			break;
		}
		default: {
			PlaceCarOnUnknownRoad(c, r);
			break;
		}
		}
	}

	private static void CreatePrefab() {
		if (car1Prefab == null) {
			car1Prefab = Resources.Load("Car1") as GameObject;
		}
		if (possibleTextures == null) {
			LoadTextures();
		}
	}

	public static int CurrentSerialNumber = 0;

	private static GameObject CreateNewCar() {
		CreatePrefab();
		GameObject car = (GameObject) Instantiate(car1Prefab);
		//pick a random texture
		car.GetComponent<SpriteRenderer>().sprite = possibleTextures[Random.Range(0, possibleTextures.Count)];
		Car c = (Car)car.GetComponent<Car>();
		c.plan = car.AddComponent<TravelPlan>();
		c.serialNumber = CurrentSerialNumber++;
		return car;
	}

	public static Car CreateCar(ParkingSpotRoad parking) {
		GameObject car = CreateNewCar();
		Car c = (Car)car.GetComponent<Car>();
		PlaceCarOnRoad(c, parking);
		ParkingSpace p = parking.ParkCar(c);
		c.plan.parkedAt = p;
		c.Park(p);
		//rotate car
		Vector2 lookto = parking.neighbourRoads[0].AsVector() - parking.AsVector();
		FaceCarInDirection(c, lookto);
		car.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, 0);
		return c;
	}
	
	public static Car CreateCar(Road road) {
		GameObject car = CreateNewCar();
		car.transform.position = new Vector3(road.xPos, road.yPos, 0);
		PlaceCarOnRoad((Car)car.GetComponent<Car>(), road);
		return (Car)car.GetComponent<Car>();
	}
	
	/**
	 * Default is
	 * */
	public static Car CreateCar(Road road, float zRotation) {
		GameObject car = CreateNewCar();
		car.transform.position = new Vector3(road.xPos, road.yPos, 0);
		car.transform.Rotate(0,0,zRotation);
		PlaceCarOnRoad((Car)car.GetComponent<Car>(), road);
		return (Car)car.GetComponent<Car>();
	}
}


