using UnityEngine;
using System.Collections.Generic;
using System;
using SimpleJSON;

public class TutorialPresenter : MonoBehaviour {

	//Currently presented tutorials
	//in theory, there could be multiple shown at the same time
	//but as it so happens, i have never seen more than 1 at a time, so i have no idea
	Dictionary<Rect, Pair<Texture2D, Action>> presentingTuts = new Dictionary<Rect, Pair<Texture2D, Action>>();

	//Prepares tutorials from json
	public void Prepare(TextAsset jsonAsset) {
		if (jsonAsset == null) {
			return;
		}

		JSONNode node = JSON.Parse(jsonAsset.text);
		if (node != null) {
			JSONArray tutorials = node["tutorials"].AsArray;
			World w = FindObjectOfType<World>();
			foreach (JSONNode n in tutorials) {
				Tutorial t = new Tutorial(n);
				t.presenter = this;

				//Register the awakeable to be awaken at the designated time
				w.time.WakeUpAt(t, new TimeClass(n["time"]));
			}
		}
	}

	public void Present(Texture2D resource, Rect where, Action action = null, bool fullscreen = true) {

		if (!enabled) {
			return;
		}

		//Pause game if simulating
		if (Game.Instance.phase == Game.Phase.Simulation)
			Game.Instance.Pause();

		//Default action is to resume the game
		if (action == null) {
			action = Game.Instance.Resume;
		}
		Debug.Log("Showing " + resource);
		presentingTuts.Add(where, new Pair<Texture2D, Action>(resource, action));

	}

	void OnGUI() {
		if (presentingTuts.Count == 0) {
			return;
		}
		//Draw the tutorial texture (could be string btw, then just do GUI.Label()
		foreach (KeyValuePair<Rect, Pair<Texture2D, Action>> pair in presentingTuts) {
			GUI.DrawTexture(pair.Key, pair.Value.First);
		}

		//Create a screen-wide button that is invisible, when clicked, release all shown tutorials
		if (GUI.Button(new Rect(0,0, Screen.width, Screen.height), "", GUI.skin.label)) {
			foreach (KeyValuePair<Rect, Pair<Texture2D, Action>> pair in presentingTuts) {
				if (pair.Value.Second != null)
					pair.Value.Second();
			}
			presentingTuts.Clear();
		}

	}
}

