using UnityEngine;
using System.Collections.Generic;
using System;

public class Builder : MonoBehaviour {

	//Time started building - so that there is a grace period between selecting a building and placing it down
	public float TimeStarted;
	//Is currently prepared to build a building
	public bool IsBuilding = false;
	//The type of building being prepared
	public Buildable CurrentBuildable = Buildable.None;

	private static GameObject powerstationPrefab = RoadFactory.chargeSpotPrefab;

	//Collections to keep track of things
	//Action stack - retains the actions made by the user so they can be undone
	//currently only deconstructing is supported
	private Stack<Deconstructible> actionStack;
	//List of all decorations for folding/unfolding when constructing
	private List<DecorationHouse> decorations;

	//Current GO of building
	private GameObject current;

	void Awake() {
		actionStack = new Stack<Deconstructible>();
	}

	public void Prebuild(Buildable buildable) {
		if (current != null) {
			CancelCurrent();
		}
		SetBuilding(buildable);

		//Attach object to mouse and create the effect of being built
		switch (buildable) {
		case Buildable.Powerstation_left : {
			current = CreateChargeSpotLeft();
			AttachToMouse(current);
			AttachWeaver(current, Side.left);
			SetTemporaryConnectibles(current);
			AddConnectibleWeaversToStations(current);
			break;
		}
		case Buildable.Powerstation_right : {
			current = CreateChargeSpotRight();
			AttachToMouse(current);
			AttachWeaver(current, Side.right);
			SetTemporaryConnectibles(current);
			AddConnectibleWeaversToStations(current);
			break;
		}
		case Buildable.Powerstation_up : {
			current = CreateChargeSpotUp();
			AttachToMouse(current);
			AttachWeaver(current, Side.up);
			SetTemporaryConnectibles(current);
			AddConnectibleWeaversToStations(current);
			break;
		}
		case Buildable.Powerstation_down : {
			current = CreateChargeSpotDown();
			AttachToMouse(current);
			AttachWeaver(current, Side.down);
			SetTemporaryConnectibles(current);
			AddConnectibleWeaversToStations(current);
			break;
		}
		case Buildable.Pole : {
			current = Pole.CreatePole().gameObject;
			AttachToMouse(current);
			current.GetComponent<FollowMouse>().discrete = false;
			current.AddComponent<PoleWeaver>();
			//Set temporary connection
			SetTemporaryConnectibles(current);
			break;
		}
		case Buildable.Plant : {
			current = Powerplant.CreatePowerplant().gameObject;
			AttachToMouse(current);
			//reuse of RoadValidityChecker
			current.AddComponent<RoadValidityChecker>();
			current.AddComponent<ConnectibleWeaver>();
			SetTemporaryConnectibles(current);
			break;
		}
		}

		//Fold all houses
		FoldDecorations();
	}

	void FoldDecorations() {
		if (decorations == null) {
			FindDecorations();
		}
		foreach (DecorationHouse house in decorations) {
			house.Fold();
		}
	}

	void UnfoldDecorations() {
		if (decorations == null) {
			FindDecorations();
		}
		foreach (DecorationHouse house in decorations) {
			house.Unfold();
		}
	}

	//Add connectible weavers to stations
	void AddConnectibleWeaversToStations(GameObject obj) {
		Connectible[] cons = obj.GetComponentsInChildren<Connectible>();
		foreach(Connectible c in cons) {
			c.transform.gameObject.AddComponent<ConnectibleWeaver>();
		}
	}

	//Find all connectibles in a GO including children (mainly cause of power stations)
	void SetTemporaryConnectibles(GameObject go) {
		Connectible c = go.GetComponent<Connectible>();
		if (c != null) {
			c.isTemporary = true;
		}
		foreach (Connectible con in go.GetComponentsInChildren<Connectible>()) {
			con.isTemporary = true;
		}
	}

	//Begin building
	void SetBuilding(Buildable b) {
		CurrentBuildable = b;
		IsBuilding = true;
		TimeStarted = Time.realtimeSinceStartup;
	}

	//Add FollowMouse script to GO
	GameObject AttachToMouse(GameObject obj) {
		obj.AddComponent<FollowMouse>();
		return obj;
	}

	//Attach powerstation weaver (Side for adjacent road weaving)
	void AttachWeaver(GameObject obj, Side direction) {
		current.AddComponent<RoadValidityChecker>();
		RoadWeaver w = current.AddComponent<RoadWeaver>();
		w.direction = direction;
	}

	public void BuildCurrent() {

		GameObject building = null;
		Buildable temp = CurrentBuildable;

		switch(CurrentBuildable) {
		case Buildable.Powerstation_down :
		case Buildable.Powerstation_left :
		case Buildable.Powerstation_right :
		case Buildable.Powerstation_up : {
			//Check Validator
			bool valid = current.GetComponent<RoadValidityChecker>().IsValid();
			World w = FindObjectOfType<World>();
			Vector2 newPlacement = Vector2.zero;

			if (valid || PossibleToHelp(CurrentBuildable, current, out newPlacement)) {
				//build
				if (Game.Instance.SubtractResource(CurrentBuildable.GetCost())) {
					Road r = w.roadmap.BuildPowerstation(CurrentBuildable, current.transform.position.ToVector2(), w);
					building = r.gameObject;
					ResetState();
					ConnectConnectibles(LevelManager.properties.PoleRadius, r.GetComponentsInChildren<Connectible>());
				} else {
					Game.Instance.Warn(LocaleManager.locale.InsufficientResources);
					ResetState();
				}
			} else {
				//do nothing
				Game.Instance.Warn(LocaleManager.locale.InvalidPlacement);
			} 
			break;
		}
		case Buildable.Pole : {
			//check validity
			if (Game.Instance.SubtractResource(CurrentBuildable.GetCost())) {

				Pole p = Pole.BuildPole(current.transform.position.ToVector2());
				//Destroy the dummy first, else it will try to connect it.
				building = p.gameObject;
				ResetState();
				ConnectConnectibles(LevelManager.properties.PoleRadius, p.GetComponent<Connectible>());
			} else {
				Game.Instance.Warn(LocaleManager.locale.InsufficientResources);
				ResetState();
			}
			break;
		}
		case Buildable.Plant : {
			bool valid = current.GetComponent<RoadValidityChecker>().IsValid();
			if (valid) {
				if (Game.Instance.SubtractResource(CurrentBuildable.GetCost())) {
					Powerplant p = FindObjectOfType<World>().BuildPowerplant(current.transform.position.ToVector2());
					//Destroy the dummy first, else it will try to connect it.
					building = p.gameObject;
					ResetState();
					ConnectConnectibles(LevelManager.properties.PoleRadius, p.GetComponent<Connectible>());
				} else {
					Game.Instance.Warn(LocaleManager.locale.InsufficientResources);
					ResetState();
				}
			} else {
				Game.Instance.Warn(LocaleManager.locale.InvalidPlacement);
			}
			break;
		}
		}

		//Add to action stack if successful
		if (building != null) {
			Deconstructible d = building.AddComponent<Deconstructible>();
			d.building = temp;
			actionStack.Push(d);
		}
		UnfoldDecorations();

	}

	//Try to help the user build a building by trying to displace it slightly
	private bool PossibleToHelp(Buildable buildable, GameObject obj, out Vector2 newPosition) {
		Vector2 direction = Vector2.zero;
		switch (buildable) {
		case Buildable.Powerstation_down : {
			direction = -Vector2.up;
			break;
		}
		case Buildable.Powerstation_left : {
			direction = -Vector2.right;
			break;
		}
		case Buildable.Powerstation_right : {
			direction = Vector2.right;
			break;
		}
		case Buildable.Powerstation_up : {
			direction = Vector2.up;
			break;
		}
		default : {
			throw new System.ArgumentException();
		}
		}

		Vector2 pos = obj.transform.position.ToVector2();
		RoadValidityChecker checker = obj.GetComponent<RoadValidityChecker>();

		int maxIterations = 3;

		for (int i = 1; i <= maxIterations; ++i) {
			obj.transform.position += (direction * i).ToVector3();
			checker.CheckValidity();
			if (checker.IsValid()) {
				newPosition = obj.transform.position.ToVector2();
				return true;
			}
		}

		newPosition = Vector2.zero;
		return false;
	}

	public void UndoAction() {
		//some may have been manually deleted
		while (actionStack.Count > 0 && actionStack.Peek() == null) {
			actionStack.Pop();
		}
		if (actionStack.Count > 0) {
			actionStack.Pop().Deconstruct();
		}
	}

	public static void ConnectConnectibles(float radius, params Connectible[] toConnect) {

		foreach (Connectible con in toConnect) {
			foreach (Connectible p in Connectible.ConnectiblesInRange(con.transform.position)) {
				if (!p.isTemporary && p.gameObject != con.gameObject) {
					if (p.transform.parent != null && p.transform.parent == con.transform.parent) {
						continue;
					}
					//BEWARE - LINE RENDERER DUPLICITY
					con.Connect(p);
					p.Connect(con);
				}
			}
		}
	}

	private void ResetState() {
		Destroy(current);
		current = null;
		IsBuilding = false;
		CurrentBuildable = Buildable.None;
	}

	public void CancelCurrent() {
		Destroy(current);
		current = null;
		IsBuilding = false;
		CurrentBuildable = Buildable.None;

		//Unfold all houses
		UnfoldDecorations();
	}

	void FindDecorations() {
		DecorationHouse[] houses = FindObjectsOfType<DecorationHouse>();
		decorations = new List<DecorationHouse>(houses);
	}

	public static GameObject CreatePole() {
		return Pole.CreatePole().gameObject;
	}

	public static GameObject CreateChargeSpotLeft() {
		powerstationPrefab = RoadFactory.chargeSpotPrefab;
		GameObject r = (GameObject) Instantiate(powerstationPrefab);
		return r;
	}
	
	public static GameObject CreateChargeSpotRight() {
		powerstationPrefab = RoadFactory.chargeSpotPrefab;
		GameObject r = (GameObject) Instantiate(powerstationPrefab);
		r.transform.Rotate(new Vector3(0,0,180));
		return r;
	}
	
	public static GameObject CreateChargeSpotDown() {
		powerstationPrefab = RoadFactory.chargeSpotPrefab;
		GameObject r = (GameObject) Instantiate(powerstationPrefab);
		r.transform.Rotate(new Vector3(0,0,90));
		return r;
	}
	
	public static GameObject CreateChargeSpotUp() {
		powerstationPrefab = RoadFactory.chargeSpotPrefab;
		GameObject r = (GameObject) Instantiate(powerstationPrefab);
		r.transform.Rotate(new Vector3(0,0,270));
		return r;
	}

}
