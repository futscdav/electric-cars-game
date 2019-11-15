using UnityEngine;
using System.Collections;
using SimpleJSON;

public class LevelProperties {

	public int MaxResource;
	public TimeClass DayStart;
	public TimeClass DayEnd;

	public bool ChargingLeft;
	public bool ChargingRight;
	public bool ChargingDown;
	public bool ChargingUp;
	public bool Poles;
	public bool Powerplants;

	public int ChargingLeftCost;
	public int ChargingRightCost;
	public int ChargingDownCost;
	public int ChargingUpCost;
	public int PoleCost;
	public int PowerplantCost;

	public float PoleRadius;

	public string ScriptName;

	public static LevelProperties Parse(string json) {
		JSONNode obj = JSON.Parse(json);

		LevelProperties properties = new LevelProperties();
		properties.MaxResource = obj["max_resource"].AsInt;
		properties.DayStart = new TimeClass(obj["day_start"]);

		if (obj["day_end"] != null) {
			properties.DayEnd = new TimeClass(obj["day_end"]);
		}

		if (obj["script"] != null) {
			properties.ScriptName = obj["script"];
		}

		properties.ChargingLeft = obj["charging_left"].AsBool;
		properties.ChargingRight = obj["charging_right"].AsBool;
		properties.ChargingDown = obj["charging_down"].AsBool;
		properties.ChargingUp = obj["charging_up"].AsBool;
		properties.Poles = obj["poles"].AsBool;
		properties.Powerplants = obj["powerplants"].AsBool;

		if (properties.ChargingLeft) {
			properties.ChargingLeftCost = obj["charging_left_cost"].AsInt;
		}
		if (properties.ChargingRight) {
			properties.ChargingRightCost = obj["charging_right_cost"].AsInt;
		}
		if (properties.ChargingDown) {
			properties.ChargingDownCost = obj["charging_down_cost"].AsInt;
		}
		if (properties.ChargingUp) {
			properties.ChargingUpCost = obj["charging_up_cost"].AsInt;
		}
		if (properties.Poles) {
			properties.PoleCost = obj["pole_cost"].AsInt;
			properties.PoleRadius = obj["pole_radius"].AsFloat;
		}
		if (properties.Powerplants) {
			properties.PowerplantCost = obj["powerplant_cost"].AsInt;
		}

		return properties;
	}

}
