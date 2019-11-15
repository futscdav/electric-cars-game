using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConnectibleWeaver : MonoBehaviour {

	public float radius = LevelManager.properties.PoleRadius;
	public List<Connectible> myConnectibles;

	void Awake() {
		myConnectibles = FindMyConnectibles();
	}

	void Update () {

		Connectible[] connectiblesInRange = Connectible.ConnectiblesInRange(transform.position);
		
		foreach (Connectible c in connectiblesInRange) {
			foreach (Connectible mine in myConnectibles) {
				if (c.transform.parent != null && mine.transform.parent == c.transform.parent) {
					continue;
				}
				//Temporarily connect them
				c.ConnectTemporary(mine);
			}
		}
	}

	List<Connectible> FindMyConnectibles() {
		List<Connectible> list = new List<Connectible>();
		list.AddRange(GetComponents<Connectible>());
		list.AddRange(GetComponentsInChildren<Connectible>());
		return list.Distinct().ToList();
	}

}
