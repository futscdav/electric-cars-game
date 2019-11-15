using UnityEngine;
using System.Collections;

public class DecorationHouse : Building {

	public void Fold() {
		transform.Find("base").GetComponent<MeshRenderer>().enabled = false;
		transform.Find("rooftop").GetComponent<MeshRenderer>().enabled = false;
		transform.Find("basement").GetComponent<MeshRenderer>().enabled = true;
	}

	public void Unfold() {
		transform.Find("base").GetComponent<MeshRenderer>().enabled = true;
		transform.Find("rooftop").GetComponent<MeshRenderer>().enabled = true;
		transform.Find("basement").GetComponent<MeshRenderer>().enabled = false;
	}

}

