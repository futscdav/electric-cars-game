using UnityEngine;
using System.Collections;

public class Powerplant : Building, Deconstructor {

	public static GameObject PowerplantPrefab {
		get {return Resources.Load("Powerplant") as GameObject;}
	}

	public static Powerplant CreatePowerplant() {
		GameObject o = Instantiate(PowerplantPrefab) as GameObject;
		return o.GetComponent<Powerplant>();
	}

	public void UndoChanges() {
		//Free up the tile so it can be used again
		FindObjectOfType<World>().EmptyTile(transform.position.ToVector2());
	}

}
