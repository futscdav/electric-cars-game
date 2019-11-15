using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class Connectible : MonoBehaviour {

	public Transform point;
	public Transform disabledObject;
	public Dictionary<Connectible, LineRenderer> Connected;
	public bool isTemporary;
	public bool isConnectedToPowerplant;

	public static Material connectionMaterial;

	[SerializeField]
	public GameObject chordPrefab;
	[SerializeField]
	public Texture noConnectionTexture;
	
	//this is for building new connectibles - Create temporary connections
	public Dictionary<Connectible, LineRenderer> TemporaryConnections;

	void Awake () {
		Connected = new Dictionary<Connectible, LineRenderer>();
		TemporaryConnections = new Dictionary<Connectible, LineRenderer>();
	}

	void Update () {
		//Clearing and reinstating them every frame is fairly expensive (see ConnectibleWeaver)
		//optimisation - dont clear, but reset the linerenderer positions
		//and create events to delete the temporary objects once the user is done with them
		ClearTemporary();
		if (isConnectedToPowerplant && disabledObject != null && disabledObject.GetComponent<Renderer>().enabled) {
			disabledObject.GetComponent<Renderer>().enabled = false;
		} else if (!isConnectedToPowerplant && disabledObject != null && !disabledObject.GetComponent<Renderer>().enabled) {
			disabledObject.GetComponent<Renderer>().enabled = true;
		}
	}

	//Find all connectibles in the LevelManager.properties.PoleRadius range
	//Could use a SphereCast, but its actually less precise than needed
	public static Connectible[] ConnectiblesInRange(Vector3 position) {
		Connectible[] all = FindObjectsOfType<Connectible>();
		List<Connectible> list = new List<Connectible>();
		foreach (Connectible col in all) {
			if (Vector3.Magnitude(col.transform.position - position) <= LevelManager.properties.PoleRadius) {
				list.Add(col);
			}
		}
		return list.ToArray();
	}

	public void Connect(Connectible other) {
		Connect (other, Connected);
	}
	
	public void ConnectTemporary(Connectible other) {
		Connect(other, TemporaryConnections);
	}
	
	public void ClearTemporary() {
		foreach (KeyValuePair<Connectible, LineRenderer> p in TemporaryConnections) {
			if (p.Value == null)
				continue;
			Destroy(p.Value.gameObject);
		}
		TemporaryConnections.Clear();
	}

	private static void SetConnections() {

		//reset all connections
		Connectible[] connectibles = FindObjectsOfType<Connectible>();
		foreach (Connectible con in connectibles) {
			con.isConnectedToPowerplant = false;
		}

		Powerplant[] plants = FindObjectsOfType<Powerplant>();

		foreach (Powerplant plant in plants) {
			Connectible c = plant.GetComponent<Connectible>();
			SetConnectionRec(c);
		}

	}

	private static void SetConnectionRec(Connectible c) {
		c.isConnectedToPowerplant = true;
		foreach (Connectible con in c.Connected.Keys) {
			if (!con.isConnectedToPowerplant) {
				con.isConnectedToPowerplant = true;
				SetConnectionRec(con);
			}
		} 
	}
	
	private void Connect(Connectible other, Dictionary<Connectible, LineRenderer> list) {
		
		if (list.ContainsKey(other) || other == this) {
			return;
		}
		//do some distance checking perhaps? = no, should be handled by the thing calling this code;
		
		//
		GameObject chord = Instantiate(chordPrefab) as GameObject;
		LineRenderer line = SetLine(chord.GetComponent<LineRenderer>(), point.position, other.point.position);
		//make sure to know this
		list.Add(other, line);

		SetConnections();
	}

	public void Disconnect(Connectible other) {
		if (Connected.ContainsKey(other)) {
			LineRenderer r = Connected[other];
			Destroy(r);
			Connected.Remove(other);
			other.Disconnect(this);
			SetConnections();
		}
	}

	LineRenderer SetLine(LineRenderer renderer, Vector3 from, Vector3 to) {
		if (connectionMaterial == null) {
			connectionMaterial = new Material(renderer.material);
			if (connectionMaterial.HasProperty("_Color"))
				connectionMaterial.SetColor("_Color", Color.black);
		}
		renderer.SetVertexCount(2);
		renderer.SetPosition(0, from);
		renderer.SetPosition(1, to);
		renderer.SetWidth(0.02f, 0.02f);
		Color color = Color.green;
		color.a = 0.5f;
		renderer.material = connectionMaterial;
		renderer.SetColors(color, color);

		return renderer;
	}

	public override string ToString ()
	{
		return string.Format ("[Connectible {0} {1}]", gameObject.name, gameObject.GetInstanceID());
	}

}
