using UnityEngine;
using System.Collections;

public abstract class LevelScript : MonoBehaviour {

	public virtual void OnConstruction() {}
	public virtual void OnSimulation() {}
	public virtual void OnGameOver() {}
	public virtual void OnGameWon() {}
	public virtual void OnLevelLoaded() {}

}

