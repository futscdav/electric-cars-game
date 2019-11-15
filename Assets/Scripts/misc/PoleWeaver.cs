using UnityEngine;
using System.Collections.Generic;

public class PoleWeaver : MonoBehaviour {

	private Color color = Color.green;
	private float radius = LevelManager.properties.PoleRadius;
	private float scale;
	private Pole pole;
	private ConnectibleWeaver cweaver;

	public Color Color {
		get {return color;}
		set {cylinder.GetComponent<Renderer>().material.color = value; color = value;}
	}

	public float Radius {
		get {return radius;}
		set {radius = value; SetRadius(radius); cweaver.radius = value;}
	}

	private GameObject cylinder;
	
	void Awake () {
		//draw circle around gameObject
		if (cylinder == null) {
			cylinder = Instantiate(Resources.Load("CircleArea") as GameObject) as GameObject;
			cylinder.transform.parent = gameObject.transform;
			scale = cylinder.transform.localScale.x;
			cweaver = gameObject.AddComponent<ConnectibleWeaver>();
			Radius = radius;
		}
		pole = gameObject.GetComponent<Pole>();
	}

	void Update () {

		Vector3 desired = gameObject.transform.position;
		cylinder.transform.position = new Vector3(desired.x, desired.y, 0f);
		color.a = 0.5f;
		cylinder.GetComponent<Renderer>().material.color = color;
	
	}

	private void SetRadius(float radius) {
		cylinder.transform.localScale = new Vector3(scale * radius * 2, scale * radius * 2, 0.01f);
	}
}
