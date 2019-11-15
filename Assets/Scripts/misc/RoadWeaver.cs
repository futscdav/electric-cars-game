using UnityEngine;
using System.Collections;

public class RoadWeaver : MonoBehaviour
{
	//direction to weave in
	public Side direction;
	public bool enabled = true;
	public bool isWeaving;

	Vector2 offset;
	Vector2 weavingPosition;
	Vector2 storedPosition;
	World world;

	//The imaginary weaved object
	GameObject weaved;

	void Update () {
		//Transform direction (of type Side) to offset (of type Vector2)
		//This could be done when this object is created, right now i don't
		//even think there was a relevant reason not to do that
		CreateOffset();
		if (!enabled) {
			return;
		}
		//If the position is the same as last change, dont do stuff
		if (storedPosition == gameObject.transform.position.ToVector2()) {
			return;
		}
		//if its not the same, destroy it
		if (weaved != null) {
			StopWeaving();
		}
		//Check with validatychecker for world obstacles on the main object
		//(dont weave when there are obstacles anyway)
		if (!GetComponent<RoadValidityChecker>().CheckWorldObstacles()) {
			return;
		}

		//store last position
		storedPosition = gameObject.transform.position.ToVector2();
		//create new adjacent road
		Weave();
		//tell the validitychecker about it
		SetAdjacent();
	}

	public void ForceUpdate() {
		Update();
	}

	public void ForceWeave() {
		Weave();
		SetAdjacent();
	}

	void Weave() {
		GetWorld();
		weavingPosition = gameObject.transform.position.ToVector2() + offset;
		Road r = world.FindNearestRoad(weavingPosition);
		StopWeaving();
		if (r.AsVector().AlmostEqual(weavingPosition)) {
			//create temporary road in place of r
			weaved = CreateWeavedRoad(r);
			if (weaved == null) {
				return;
			}
			weaved.transform.parent = gameObject.transform;
		}
	}

	void SetAdjacent() {
		GetComponent<RoadValidityChecker>().Adjacent = weaved;
	}

	//Check whether there is a valid road next to the gameObject (to check whether the station can be built here)
	//This is called every frame, but it shouldnt be too bad, cause we are only building one station at a time
	bool TryWeaving() {
		GetWorld();
		Vector2 weave = gameObject.transform.position.ToVector2() + offset;
		Road r = world.FindNearestRoad(weave);
		if (r.AsVector().AlmostEqual(weave)) {
			GameObject go = CreateWeavedRoad(r);
			bool ret = go != null;
			Destroy(go);
			return ret;
		}
		return false;
	}

	public bool ShouldWeave() {
		return TryWeaving();
	}

	public void StopWeaving() {
		if (weaved != null) {
			Destroy(weaved);
			weaved = null;
		}
	}

	GameObject CreateWeavedRoad(Road r) {
		Road go = null;
		switch (direction) {
		case Side.left : {
			go = PickRoad(r, false, false, false, true);
			break;
		}
		case Side.right : {
			go = PickRoad(r, false, false, true, false);
			break;
		}
		case Side.up : {
			go = PickRoad(r, false, true, false, false);
			break;
		}
		case Side.down : {
			go = PickRoad(r, true, false, false, false);
			break;
		}
		}
		if (go != null)
			return go.gameObject;
		return null;
	}

	Road PickRoad(Road r, bool up, bool down, bool left, bool right) {
		switch (r.GetType().ToString()) {
		case "NarrowRoad" : {
			if (r.up)
				return RoadFactory.CreateT((int)weavingPosition.x, (int)weavingPosition.y, true, true, left, right);
			else
				return RoadFactory.CreateT((int)weavingPosition.x, (int)weavingPosition.y, up, down, true, true);
		}
		case "CrossTRoad" : {
			return RoadFactory.CreateQCrossroad((int)weavingPosition.x, (int)weavingPosition.y);
		}
		case "EndRoad" : {
			if (r.up) {
				return RoadFactory.CreateTwoSided((int)weavingPosition.x, (int)weavingPosition.y, true, down, left, right);
			} else if (r.down) {
				return RoadFactory.CreateTwoSided((int)weavingPosition.x, (int)weavingPosition.y, up, true, left, right);
			} else if (r.left) {
				return RoadFactory.CreateTwoSided((int)weavingPosition.x, (int)weavingPosition.y, up, down, true, right);
			} else {
				return RoadFactory.CreateTwoSided((int)weavingPosition.x, (int)weavingPosition.y, up, down, left, true);
			}
		}
		}
		return null;
	}

	void GetWorld() {
		if (world == null) {
			world = FindObjectOfType<World>();
		}
	}

	void CreateOffset() {
		if (true) {
			switch (direction) {
			case Side.left : {
				offset = -Vector2.right;
				break;
			}
			case Side.right : {
				offset = Vector2.right;
				break;
			}
			case Side.up : {
				offset = Vector2.up;
				break;
			}
			case Side.down : {
				offset = -Vector2.up;
				break;
			}
			}
		}
	}

}