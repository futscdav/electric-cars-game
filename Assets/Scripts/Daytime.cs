using UnityEngine;
using System.Collections.Generic;

public class Daytime : MonoBehaviour {

	/* Class that takes care of the daytime + waking up Awakeables that are registered to be woken up */

	public TimeClass time = new TimeClass();
	public LinkedList<Pair<TimeClass, Awakeable>> wakeUpQueue = new LinkedList<Pair<TimeClass, Awakeable>>();
	public TimeClass Now {
		get {return new TimeClass(time);}
	}
		
	void FixedUpdate() {
		//Check first thing in the queue
		if (wakeUpQueue.Count < 1) {
			return;
		}
		TimeClass earliest = wakeUpQueue.First.Value.First;
		while (time >= earliest) {
			//Debug.Log(time + ": waking up " + wakeUpQueue.First.Value.Second);
			wakeUpQueue.First.Value.Second.WakeUp();
			wakeUpQueue.RemoveFirst();
			if (wakeUpQueue.Count > 0) {
				earliest = wakeUpQueue.First.Value.First;
			} else {
				return;
			}
		}
	}

	public void WakeUpAt(Awakeable a, TimeClass when) {
		SortIn(new Pair<TimeClass, Awakeable> (when, a));
	}

	void SortIn(Pair<TimeClass, Awakeable> item) {
		LinkedListNode<Pair<TimeClass, Awakeable>> node = wakeUpQueue.First;
		while (node != null) {
			TimeClass nodetime = node.Value.First;
			if (item.First < nodetime) {
				wakeUpQueue.AddBefore(node, new LinkedListNode<Pair<TimeClass, Awakeable>>(item));
				return;
			}
			node = node.Next;
		}
		wakeUpQueue.AddLast(new LinkedListNode<Pair<TimeClass, Awakeable>>(item));
	}

	public override string ToString() {
		return string.Format("{0,2:d2}:{1,2:d2}:{2,2:d2}", time.hour, time.minute, time.second);
	}

}
