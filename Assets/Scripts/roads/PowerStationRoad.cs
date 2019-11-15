using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerStationRoad : Road, Deconstructor {

	public PowerStationRoad(int x, int y) : base(x ,y) {

	}

	public List<PowerStation> stations;

	void Start() {
		stations = new List<PowerStation>();
		stations.AddRange(GetComponentsInChildren<PowerStation>());
	}

	public PowerStation GetEmptyPoweredStation() {
		foreach (PowerStation p in stations) {
			if (p.occupied == false && p.Powered) {
				return p;
			}
		}
		return null;
	}

	public override List<Waypoint> CreateWaypoints (Vector2 comingfrom, Vector2 goingto) {
		return new List<Waypoint>(new Waypoint[] {new Waypoint() {x = this.xPos, y = this.yPos, onRoad = this}});
	}

	//Remove the road connections and change back the road next to this one;
	public void UndoChanges() {

		//the road next to this is the only one connected
		Road adjacent = neighbourRoads[0];

		List<Road> connections = adjacent.neighbourRoads;
		connections.Remove(this);
		Vector2 position = adjacent.AsVector();

		bool up = adjacent.up, down = adjacent.down, left = adjacent.left, right = adjacent.right;
		if (this.up) {
			down = false;
		}
		if (this.down) {
			up = false;
		}
		if (this.left) {
			right = false;
		}
		if (this.right) {
			left = false;
		}

		Road restored = RoadFactory.CreateGeneralRoad((int)position.x, (int)position.y, up, down, left, right);
		restored.neighbourRoads = connections;
		foreach (Road r in connections) {
			r.neighbourRoads.Remove(adjacent);
			r.neighbourRoads.Add(restored);
		}

		//remove itself from roadmap
		FindObjectOfType<World>().roadmap.RemoveRoad(this);
		FindObjectOfType<World>().roadmap.RemoveRoad(adjacent);
		FindObjectOfType<World>().roadmap.RegisterRoad(restored);
		//remove old one
		Destroy(adjacent.gameObject);

	}

}
