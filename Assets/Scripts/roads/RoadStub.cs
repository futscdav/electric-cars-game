public class RoadStub {
	public int xPos {set; get;}
	public int yPos {set; get;}

	public enum RoadType {
		Regular,
		ChargingSpot,
		ParkingSpot
	}

	public RoadType type = RoadType.Regular;

	public RoadStub (int x, int y) {
		this.xPos = x;
		this.yPos = y;
	}
	public override string ToString() {
		return "(" + xPos + "," + yPos + ")";
	}
}