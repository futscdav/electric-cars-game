using UnityEngine;
using System.Collections;
using System;
using System.Globalization;

public class TimeClass {

	//Class for representation of in-game time

	public int hour;
	public int minute;
	public int second;

	//empty (midnight constructor)
	public TimeClass() {}

	//copyctor
	public TimeClass(TimeClass other) {
		hour = other.hour;
		minute = other.minute;
		second = other.second;
	}

	//standard C# time class constructor
	public TimeClass(DateTime t) {
		hour = t.Hour;
		minute = t.Minute;
		second = t.Second;
	}

	//string constructor (useable for json initialization
	public TimeClass(string s) : this(DateTime.ParseExact(s, "HHmm", CultureInfo.InvariantCulture)) {}

	#region operators

	public static bool operator <(TimeClass time1, TimeClass time2) {
		if (time1.hour != time2.hour) {
			return time1.hour < time2.hour;
		}
		if (time1.minute != time2.minute) {
			return time1.minute < time2.minute;
		}
		if (time1.second != time2.second) {
			return time1.second < time2.second;
		}
		//the same
		return false;
	}

	public static bool operator >(TimeClass time1, TimeClass time2) {
		bool less = time1 < time2;
		bool more = time2 < time1;
		return more&&!less;
	}

	public static bool operator ==(TimeClass time1, TimeClass time2) {
		bool less = time1 < time2;
		bool more = time2 > time2;
		bool equal = !less & !more;
		return equal;
	}

	public static bool operator >=(TimeClass time1, TimeClass time2) {
		return (time1>time2)||(time1==time2);
	}

	public static bool operator <=(TimeClass time1, TimeClass time2) {
		return (time1<time2)||(time1==time2);
	}

	public static bool operator !=(TimeClass time1, TimeClass time2) {
		return !(time1==time2);
	}

	#endregion

	public void AddSeconds(int seconds) {
		second += seconds;
		Normalize();
	}

	public void AddMinutes(int minutes) {
		minute += minutes;
		Normalize();
	}

	public void AddHours(int hours) {
		hour += hours;
		Normalize();
	}

	//Set to base 60 time
	private void Normalize() {
		int carry = second / 60;
		minute += carry;
		second %= 60;
		carry = minute / 60;
		hour += carry;
		minute %= 60;
		hour %= 24;
	}

	public override string ToString () {
		return string.Format ("{0,2:d2}:{1,2:d2}:{2,2:d2}", hour, minute, second);
	}

}
