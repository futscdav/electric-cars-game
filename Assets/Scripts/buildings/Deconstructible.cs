using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public interface Deconstructor {
	void UndoChanges();
}

public class Deconstructible : MonoBehaviour, BubbleOwner {

	public Buildable building;

	private SpeechBubble bubble;

	#if UNITY_ANDROID
	bool firstClick = true;
	#endif

	void OnMouseUp() {
		//Check for the correct phase to be deconstructing
		if (Game.Instance.phase != Game.Phase.Construction) {
			return;
		}

		#if UNITY_ANDROID
		//#there is a bug in android that triggers mouse events when tapped somewhere else
		if (firstClick) {
			firstClick = !firstClick;
			return;
		}
		#endif

		ToggleBubble();
	}

	void ToggleBubble() {
		InitBubble();
		bubble.render = !bubble.render;
	}

	public void OnBubbleClosed() {

	}

	void InitBubble() {
		if (bubble == null) {
			bubble = gameObject.AddComponent<SpeechBubble>();
			bubble.mat = (Material)Resources.Load("White Unlit", typeof(Material));
			bubble.guiSkin = (GUISkin)Resources.Load("GUISkin", typeof(GUISkin));
			bubble.owner = this;
			bubble.text = LocaleManager.locale.Delete+"?";
			bubble.action = Deconstruct;
		}
	}

	public void Deconstruct() {
		List<Connectible> connectibles = FindMyConnectibles();

		foreach (Connectible connectible in connectibles) {
			if (connectible == null) {
				Debug.LogError("Deconstructible has no connectible!");
				return;
			}
			List<Connectible> connected = connectible.Connected.Keys.ToList();
			foreach (Connectible c in connected) {
				connectible.Disconnect(c);
			}
		}

		Game.Instance.SubtractResource(-building.GetCost());

		//if the object has a deconstructor, find it and call it
		Deconstructor d = GetComponents<Component>().OfType<Deconstructor>().FirstOrDefault();
		if (d != null) {
			d.UndoChanges();
		}

		Destroy (gameObject);
	}

	List<Connectible> FindMyConnectibles() {
		List<Connectible> list = new List<Connectible>();
		list.AddRange(GetComponents<Connectible>());
		list.AddRange(GetComponentsInChildren<Connectible>());
		return list.Distinct().ToList();
	}
}

