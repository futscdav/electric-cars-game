using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Astar {

	private static float RoadDistance(Road a, Road b) {
		//euclidean distance - this is the evaluate function
		return Mathf.Sqrt((a.xPos-b.xPos)*(a.xPos-b.xPos)+(a.yPos-b.yPos)*(a.yPos-b.yPos));
	}

	//Class to compare the goodness of two roads (wrt the destination)
	private class RoadComparer : Comparer<Road> {

		public Road destination;

		public override int Compare(Road a, Road b) {
			float aDist = RoadDistance(destination, a);
			float bDist = RoadDistance(destination, b);
			if (aDist < bDist) {
				return -1;
			}
			return 1;
		}
	}

	//Recreate the path using the cameFrom dictionary
	private static LinkedList<Road> CreatePath(Road start, Dictionary<Road, Road> cameFrom, Road destination) {
		List<Road> path = new List<Road>();
		path.Add(destination);
		while (destination != start) {
			path.Add(cameFrom[destination]);
			destination = cameFrom[destination];
		}
		path.Reverse();

		return new LinkedList<Road>(path);
	}

	//Astar implementation
	public static LinkedList<Road> CreateJourney(Road start, Road destination) {
		HashSet<Road> closedList = new HashSet<Road>();
		RoadComparer comp = new RoadComparer();
		comp.destination = destination;
		Heap<Road> openList = new MinHeap<Road>(comp);
		Dictionary<Road, Road> cameFrom = new Dictionary<Road, Road>();
		Dictionary<Road, float> nodeScore = new Dictionary<Road, float>();
		
		openList.Add(start);
		nodeScore[start] = 0f;

		while (openList.Count != 0) {
			Road currentBest = openList.ExtractDominating();

			//goal
			if (Mathf.Approximately(0f, RoadDistance(destination, currentBest))) {
				return CreatePath(start, cameFrom, destination);
			}

			closedList.Add(currentBest);

			//expand nodes going from current best
			List<Road> successors = currentBest.neighbourRoads;
			foreach (Road successor in successors) {
				if (closedList.Contains(successor)) {
					continue;
				}

				float tentative = nodeScore[currentBest] + RoadDistance(currentBest, successor);

				float oldBestScore;
				if (!nodeScore.TryGetValue(successor, out oldBestScore)) {
					oldBestScore = float.MaxValue;
				}
				if (!openList.SlowContains(successor) || tentative < oldBestScore) {
					cameFrom[successor] = currentBest;
					nodeScore[successor] = tentative;
					if (!openList.SlowContains(successor)) {
						openList.Add(successor);
					}
				}
			}
		}
		return null;
	}
}
