using UnityEngine;
using System.Collections;

public class Level1Script : LevelScript {

	public override void OnConstruction() {
		Game.Instance.StartDay();
	}

	public override void OnSimulation() {
		FindObjectOfType<UserUI>().enabled = false;
	}

	public override void OnLevelLoaded() {
		Connectible[] allConnectibles = FindObjectsOfType<Connectible>();
		Builder.ConnectConnectibles(LevelManager.properties.PoleRadius, allConnectibles);
	}

}
