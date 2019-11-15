using UnityEngine;
using System.Collections;

public enum Buildable {
	Powerstation_left,
	Powerstation_up,
	Powerstation_down,
	Powerstation_right,
	Pole,
	Plant,
	None
}

public static class Ext {

	public static int GetCost(this Buildable b) {
		if (LevelManager.properties == null) {
			throw new System.ArgumentException("Level not initialized!");
		}
		switch (b) {
		case Buildable.Powerstation_left : {
			return LevelManager.properties.ChargingLeftCost;
		}
		case Buildable.Powerstation_right : {
			return LevelManager.properties.ChargingRightCost;
		}
		case Buildable.Powerstation_down : {
			return LevelManager.properties.ChargingDownCost;
		}
		case Buildable.Powerstation_up : {
			return LevelManager.properties.ChargingUpCost;
		}
		case Buildable.Pole : {
			return LevelManager.properties.PoleCost;
		}
		case Buildable.Plant : {
			return LevelManager.properties.PowerplantCost;
		}
		}
		return 0;
	}

}
