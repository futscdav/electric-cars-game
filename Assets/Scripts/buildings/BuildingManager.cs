using UnityEngine;
using System.Collections;

public class BuildingManager {

	public Building[,] buildingGrid;

	public BuildingManager() {

	}

	public void Build(BuildingStub[,] stubs) {

		buildingGrid = new Building[stubs.GetLength(0),stubs.GetLength(1)];

		int xStart = -stubs.GetLength(1)/2;
		int yStart = stubs.GetLength(0)/2;
		
		for (int y = 0; y < stubs.GetLength(0); ++y) {
			for (int x= 0; x < stubs.GetLength(1); ++x) {
				if (stubs [y, x] != null) {
					var stub = stubs[y ,x];
					buildingGrid[y, x] = BuildingFactory.CreateFromStub(new BuildingStub(xStart + stub.xPos, 
					                                                                     yStart - stub.yPos,
					                                                                     stub.type));
				}
			}
		} 

	}
}
