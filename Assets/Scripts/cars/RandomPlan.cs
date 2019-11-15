using UnityEngine;
using System.Collections.Generic;

public class RandomPlan : TravelPlan {

	private Road at;

	/*
	 * Travel plan which creates random destionations indefinitely
	 *	*/

	protected override IEnumerable<Trip> SelectNextTrip() {
		if (at == null) {
			at = controlledCar.road;
		}

		if (world == null || world.roadmap == null) {
			Debug.LogError("car world null??");
		}

		List<Road> roads = world.roadmap.roads;
		float prob = 1f/roads.Count;

		while (true) {
			//find a suitable destination
			Road destination = null;

			while(true)
				for (int i = 0; i < world.roadmap.roads.Count; ++i) {
					int dist = Mathf.Abs(roads[i].xPos-at.xPos)+Mathf.Abs(roads[i].yPos-at.yPos);
					if (Random.Range(0f, 1f) < prob && dist > 4) {
						destination = roads[i];
						goto destfound;
					}
				}
			destfound:
			Trip newTrip = CreateImmediateTrip(at, destination, Trip.TripType.Cruise);
			newTrip.type = Trip.TripType.Cruise;
			at = destination;
			yield return newTrip;
		}
	}
}

