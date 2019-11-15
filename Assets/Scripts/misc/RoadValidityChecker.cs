using UnityEngine;
using System.Collections;

public class RoadValidityChecker : MonoBehaviour {

	//Original and current alpha & color values for changing the color back & forth
	private float origAplha;
	private float currAlpha;
	private Color origColor;
	private Color currColor;

	private World world;

	//The road next to this thing, only valid for building a powerstation
	private GameObject adjacentRoad;
	//In case of a powerplant, this should be null.
	//a more elegant solution would use some sort of inheritance
	private RoadWeaver weaver;
	//Stores current validity
	bool valid = true;

	public GameObject Adjacent {
		get {return adjacentRoad;} 
		set {
			adjacentRoad = value;
			ChangeColor(gameObject, currColor);
			ReduceOpacity(gameObject, currAlpha);
		}
	}

	void Start () {
		//Store the original values
		origAplha = gameObject.GetComponent<Renderer>().material.color.a;
		origColor = gameObject.GetComponent<Renderer>().material.color;
		//Change color
		ChangeColor(gameObject, Color.green);
		//Start by reducing opacity
		ReduceOpacity(gameObject, 0.75f);
		world = FindObjectOfType<World>();
	
	}

	void ChangeColor(GameObject what, Color color) {
		currColor = color;
		if (what == null) {
			return;
		}
		what.GetComponent<Renderer>().material.color = color;
		//Debug.Log(what);
		for (int i = 0; i < what.transform.childCount; ++i) {
			if (what.transform.GetChild(i).gameObject.GetComponent<Renderer>())
				ChangeColor(what.transform.GetChild(i).gameObject, color);
		}
	}

	void ReduceOpacity(GameObject what, float newalpha) {
		currAlpha = newalpha;
		if (what == null) {
			return;
		}
		Color old = what.GetComponent<Renderer>().material.color;
		old.a = newalpha;
		what.GetComponent<Renderer>().material.color = old;
		for (int i = 0; i < what.transform.childCount; ++i) {
			if (what.transform.GetChild(i).gameObject.GetComponent<Renderer>())
				ReduceOpacity(what.transform.GetChild(i).gameObject, newalpha);
		}
	}

	void Update () {
		SetOrdering();
		CheckValidity();
	}

	//Set sprite sorting order (so that it appears above other roads/cars/whatever
	void SetOrdering() {
		if (gameObject.GetComponent<SpriteRenderer>() != null) {
			gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
		}
		if (adjacentRoad != null && adjacentRoad.GetComponent<SpriteRenderer>() != null) {
			adjacentRoad.GetComponent<SpriteRenderer>().sortingOrder = 4;
		}
	}

	void SetValid() {
		if (valid) {
			return;
		}
		valid = true;
		if (weaver != null) {
			//force the weaver to show the adjacent road
			weaver.enabled = true;
			weaver.ForceWeave();
		}
		//Set green color
		ChangeColor(gameObject, Color.green);
		ReduceOpacity(gameObject, 0.75f);
	}

	void SetInvalid() {
		if (!valid) {
			return;
		}
		valid = false;
		if (weaver != null) {
			//Force the weaver to hide anything it's showing
			weaver.enabled = false;
			weaver.StopWeaving();
		}
		//Set color to red
		ChangeColor(gameObject, Color.red);
		ReduceOpacity(gameObject, 0.75f);
	}

	//Check for world objects that would prohibit the construction
	public bool CheckWorldObstacles() {
		Vector2 pos = gameObject.transform.position.ToVector2();
		//array coordinates
		Vector2 gridpos = world.GetImagWorldCoordinates((int)pos.x ,(int)pos.y);

		//Allow building on sidewalks and empty tiles
		//Debug.Log(gridpos);
		if (!((int)gridpos.y < 0 || (int)gridpos.y >= world.terraingrid.GetLength(0) ||
			(int)gridpos.x < 0 || (int)gridpos.x >= world.terraingrid.GetLength(1))) {
			if (world.terraingrid[(int)gridpos.y,(int)gridpos.x] != Tile.Empty &&
				world.terraingrid[(int)gridpos.y,(int)gridpos.x] != Tile.Sidewalk) {
				return false;
			}
		}

		//On a road (there is a functional road right beneath this object)
		Road r = world.FindNearestRoad(pos);
		if (r.AsVector().AlmostEqual(pos)) {
			return false;
		}
		return true;
	}

	public bool IsValid() {
		return valid;
	}

	public void CheckValidity() {

		//Find weaver (might not exist)
		if (weaver == null) {
			weaver = GetComponent<RoadWeaver>();
		}

		//Check for terrain
		if (!CheckWorldObstacles()) {
			SetInvalid();
			return;
		}

		//Ask the weaver whether there is a valid road next to this station
		if (weaver != null && !weaver.ShouldWeave()) {
			//Weaver didnt detect any
			SetInvalid();
			return;
		}

		//Otherwise, it's ok
		SetValid();
	}
}

