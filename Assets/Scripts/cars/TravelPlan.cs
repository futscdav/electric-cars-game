using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class TravelPlan : MonoBehaviour, Awakeable {

	//Current planned journey
	public LinkedList<Waypoint> Journey;
	//currently running trip
	public Trip ActiveTrip;

	//self-explanatory
	public ParkingSpace parkedAt;
	public PowerStation chargingAt;

	public bool TripInProgress;
	public bool Finished = false;

	protected static World world;
	//the car this plan controls
	protected Car controlledCar;

	//misc
	private Trip suspededTrip;
	private bool goingToCharge;
	private bool charging;
	private State state;

	//All trips in a list (only relevant when there actually is a list)
	public List<Trip> Trips;

	private enum State {
		parked,
		enRoute,
		charging
	}

	void OnDestroy() {
		//Free all the resources
		if (parkedAt != null) {
			parkedAt.Vacate();
		}
		if (chargingAt != null) {
			chargingAt.Free();
		}
	}

	public void RemoveFromGame() {
		//When car is destroyed
		OnDestroy();
	}

	void Start() {
		//find the car
		controlledCar = GetComponent<Car>();

		//create first journey
		CreateJourney();
		//wake me up when its time to depart
		SetWakeUp(ActiveTrip.departure);
	}

	public void WakeUp() {
		//awaken when car is supposed to go
		BeginActiveTrip();
		//Call event to slow time down if its too fast
		Game.Instance.CarWokenUp();
		if (TripInProgress && Journey.Count > 0) {
			SendCarToWaypoint(Journey.First.Value);
		}
	}

	void SetWakeUp(TimeClass when) {
		if (world == null) {
			world = FindObjectOfType<World>();
		}
		//set up the event
		world.time.WakeUpAt(this, when);
	}

	void BeginActiveTrip() {

		switch (state) {
		case State.parked : {
			//move out
			controlledCar.Unpark();
			//Free parking space
			if (parkedAt != null)
				parkedAt.Vacate();
			break;
		}
		case State.enRoute : {
			//nothing has to be done
			break;
		}
		case State.charging : {
			if (charging) {
				//Debug.Log("Suspending trip due to charging");
				return;
			}
			//Free the station
			controlledCar.LeavePowerStation();
			chargingAt.Free();
			break;
		}
		}
		//Set en route
		TripInProgress = true;
		state = State.enRoute;
	}

	void EndActiveTrip() {
		ActiveTrip.completed = true;
		TripInProgress = false;
		controlledCar.GoToState(Car.CarState.waiting);

		switch (ActiveTrip.type) {
		case Trip.TripType.Park : {
			ParkingSpace s = ((ParkingSpotRoad)ActiveTrip.end).GetEmptySpace();
			//Reserve the parking space
			s.Occupy();
			parkedAt = s;
			state = State.parked;

			controlledCar.Park(s);
			break;
		}
		case Trip.TripType.Charge : {
			PowerStationRoad s = world.FindNearestPoweredPowerstation(controlledCar.transform.position.ToVector2());
			if (s == null) {
				Debug.LogError("No power stations! Handle that.");
			}
			PowerStation ps = s.GetEmptyPoweredStation();
			//no empty station available
			//this is actually obsolete.
			if (ps == null) {
				Debug.LogError("All stations full (handled in coroutine.)!");
			} else {
				chargingAt = ps;
				ps.Occupy();
			}

			charging = true;
			state = State.charging;

			controlledCar.Recharge(chargingAt);
			return;
		}
		case Trip.TripType.Cruise : {
			state = State.enRoute;
			break;
		}
		}

		//Create next journey
		CreateJourney();
		if (ActiveTrip == null) {
			Finished = true;
			Game.Instance.OnCarFinished(controlledCar);
			return;
		}
		SetWakeUp(ActiveTrip.departure);
	}

	//Notification method for car to let the plan know that a waypoint has been reached
	public void WaypointReached() {
		RemoveFirstWaypoint();
		if (Journey.Count == 0) {
			EndActiveTrip();
		} else {
			SendCarToWaypoint(Journey.First.Value);
		}
		if (Journey.Count > 0 && ActiveTrip != null)
			BehaveLastWaypoints(Journey);
	}

	void SendCarToWaypoint(Waypoint w) {
		if (w == null) {
			Debug.LogError ("Sending car to null!!");
		}
		controlledCar.GoToWaypoint(w);
	}

	public void ReplanWithRecharge() {
		if (goingToCharge == true) {
			return;
		}
		//Find nearest power station
		PowerStationRoad closest = world.FindNearestPoweredPowerstation(controlledCar.transform.position.ToVector2());
		if (closest == null) {
			return;
		}
		if (closest.transform.position.Distance(transform.position) > ActiveTrip.end.transform.position.Distance(transform.position)) {
			//end is closer than the station
			return;
		}

		Road onWhich = closest;
		//From current waypoint to charge
		suspededTrip = ActiveTrip;
		if (onWhich == null || controlledCar.road == null) {
			Debug.Log (onWhich);
			Debug.Log(controlledCar.road);
		}
		Road carRoad = world.FindNearestRoad(controlledCar.transform.position.ToVector2());
		ActiveTrip = CreateImmediateTrip(carRoad, onWhich, Trip.TripType.Charge);

		CreateJourney(carRoad, onWhich);
		goingToCharge = true;
	}

	public bool WillStopForRecharge() {
		return goingToCharge;
	}

	//Notification method for car
	public void RechargeComplete() {
		//Continue in the plan
		charging = false;
		goingToCharge = false;
		ActiveTrip = CreateImmediateTrip(chargingAt.road, suspededTrip.end, suspededTrip.type);
		suspededTrip.completed = true;
		//Debug.Log ("Continuing after recharge.");
		CreateJourney(chargingAt.road, suspededTrip.end);
		this.WakeUp();
		Invoke("ReenableCollision", 2.0f);
	}

	void ReenableCollision() {
		controlledCar.SetCollisionAvoidance(Car.CollisionAvoidance.dynamicc);
	}

	//last few waypoints may have specific behavior
	private void BehaveLastWaypoints(LinkedList<Waypoint> points) {
		int count = points.Count;

		switch (ActiveTrip.type) {
		case Trip.TripType.Park : {
			ParkingSpotRoad p = (ParkingSpotRoad)points.Last.Value.onRoad;
			ParkingSpace parking = p.GetEmptySpace();
			if (parking == null) {
				Debug.LogError("Unhandled exception: parking full.");
				return;
			}
			Waypoint last = points.Last.Value;
			last.Set(parking.transform.position.ToVector2());
			break;
		}
		case Trip.TripType.Charge : {
			PowerStationRoad p = (PowerStationRoad)points.Last.Value.onRoad;
			if (count < 2) {
				//Set static collision avoidance
				controlledCar.SetCollisionAvoidance(Car.CollisionAvoidance.staticc);
				if (p.GetEmptyPoweredStation() == null) {
					controlledCar.GoToState(Car.CarState.waiting);
					StartCoroutine(PollForStation(p));
				}
			}
			if (count <= 1) {
				Debug.Log("Disabling collision avoidance");
				//set dynamic collision avoidance
				controlledCar.SetCollisionAvoidance(Car.CollisionAvoidance.disabled);
			}
			break;
		}
		}
	}

	//Try to get a free station
	IEnumerator PollForStation(PowerStationRoad p) {
		while (p.GetEmptyPoweredStation() == null) {
			yield return new WaitForSeconds(0.1f);
		}
		PowerStation station = p.GetEmptyPoweredStation();
		station.Occupy();
		chargingAt = station;
		controlledCar.GoToState(Car.CarState.driving);
	}

	private void RemoveFirstWaypoint() {
		if (Journey.Count > 0)
			Journey.RemoveFirst();
	}

	public bool IsFinished() {
		return Finished;
	}

	private void SetActiveTrip() {
		CreateJourney();
	}

	public ICollection<Waypoint> GetPlan() {
		return (ICollection<Waypoint>) Journey;
	}

	public List<Waypoint> RequestPlanList() {
		return new List<Waypoint>(Journey);
	}

	protected Trip CreateImmediateTrip(Road from, Road to, Trip.TripType tripType) {
		Trip p = new Trip();
		p.start = from;
		p.end = to;
		p.type = tripType;
		p.departure = world.time.Now;
		return p;
	}

	void CreateJourney() {
		if (world == null) {
			world = FindObjectOfType<World>();
		}

		ActiveTrip = SelectNextTrip().First();

		if (ActiveTrip == null) {
			Finished = true;
			return;
		}
		SetWakeUp(ActiveTrip.departure);
		CreateJourney(ActiveTrip.start, ActiveTrip.end);
	}

	void CreateJourney(Road from, Road to) {
		List<Road> path = null;
		if (from != null && to != null)
			 path = new List<Road>(Astar.CreateJourney(from, to));
		else
			Debug.LogError("Null ptr to A*");

		Journey = new LinkedList<Waypoint>(Car.CreateWaypoints(path, controlledCar));
	}

	protected virtual IEnumerable<Trip> SelectNextTrip() {

		//Find the earliest trip in the list

		for (int i = 0; i < Trips.Count; ++i) {
			Trip earliest = null;
			foreach (Trip t in Trips) {
				if (t.completed) {
					continue;
				}
				if (earliest == null) {
					earliest = t;
				}
				if (earliest.departure.hour >= t.departure.hour) {
					if (earliest.departure.minute > t.departure.minute) {
						earliest = t;
					}
				}
			}

			yield return earliest;
		}
	}

}
