using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pole : MonoBehaviour {

	[SerializeField]
	public GameObject polePrefab;
	

	public static GameObject PolePrefab {
		get {return Resources.Load("pole") as GameObject;}
	}

	public static Pole BuildPole(Vector2 where) {
		Pole newpole = CreatePole();
		newpole.gameObject.transform.position = new Vector3(where.x, where.y, newpole.gameObject.transform.position.z);
		return newpole;
	}

	public static Pole CreatePole() {
		var pole = Instantiate(PolePrefab) as GameObject;
		pole.transform.position = new Vector3(pole.transform.position.x, pole.transform.position.y, -0.5f);
		return pole.GetComponent<Pole>();
	}
}
