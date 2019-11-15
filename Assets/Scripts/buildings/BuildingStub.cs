using System;

public class BuildingStub {		

	public enum BuildingType {
		PowerPlant,
		PowerStation,
		Decoration
	}

	public int xPos;
	public int yPos;
	public BuildingType type;

	public BuildingStub () {
	
	}

	public BuildingStub(int x, int y, BuildingType type) {
		xPos = x;
		yPos = y;
		this.type = type;
	}
}


