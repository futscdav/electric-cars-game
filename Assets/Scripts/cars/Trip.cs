using UnityEngine;
using System.Collections;

public class Trip {

	public enum TripType {
		Charge,
		Park,
		Cruise
	}

	public Road start;
	public Road end;
	public TripType type;
	public TimeClass departure;
	public bool completed;

	public override string ToString ()
	{
		return string.Format ("[Trip from {0} to {1}: departure: {2}]", start, end, departure);
	}

}
