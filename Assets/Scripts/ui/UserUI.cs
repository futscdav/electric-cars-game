using UnityEngine;
using System;
using System.Collections.Generic;

public class UserUI : MonoBehaviour {

	public bool enabled;
	public Texture2D placeholder;

	//the box around the ui
	private BoxWrapper bw;
	//the toolbar inside the box
	private Toolbar tb;
	//the buttons inside the toolbar
	private List<ToolbarButton> buttons;

	private static UIController controller;

	void Awake() {
		controller = GetComponent<UIController>();
		placeholder = Resources.Load("charge") as Texture2D;
	}

	//set back button
	public void SetSimulationMode() {
		if (bw != null)
			bw.ResetRekt();
		buttons = new List<ToolbarButton>();
		buttons.Add(new ToolbarActionButton {desc = LocaleManager.locale.Back, image = Resources.Load("undo-icon") as Texture2D, action = Game.Instance.GoBackToConstruction});
	}

	//reinit construction buttons
	public void SetConstructionMode() {
		if (bw != null)
			bw.ResetRekt();
		Init (LevelManager.properties);
	}

	public void Init(LevelProperties properties) {
		buttons = new List<ToolbarButton>();

		if (properties.ChargingLeft) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Powerstation_left.GetCost().ToString(), image = Resources.Load("charge-left") as Texture2D, item = Buildable.Powerstation_left});
		}
		if (properties.ChargingUp) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Powerstation_up.GetCost().ToString(), image = Resources.Load("charge-up") as Texture2D, item = Buildable.Powerstation_up});
		}
		if (properties.ChargingDown) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Powerstation_down.GetCost().ToString(), image = Resources.Load("charge-down") as Texture2D, item = Buildable.Powerstation_down});
		}
		if (properties.ChargingRight) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Powerstation_right.GetCost().ToString(), image = Resources.Load("charge-right") as Texture2D, item = Buildable.Powerstation_right});
		}
		if (properties.Poles) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Pole.GetCost().ToString(), image = Resources.Load("pole-text") as Texture2D, item = Buildable.Pole});
		}
		if (properties.Powerplants) {
			buttons.Add(new ToolbarBuildableButton {desc = Buildable.Plant.GetCost().ToString(), image = Resources.Load("powerplant-text") as Texture2D, item = Buildable.Plant});
		}
		buttons.Add(new ToolbarActionButton {desc = LocaleManager.locale.Undo, image = Resources.Load("undo-icon") as Texture2D, action = controller.UndoAction});
		buttons.Add(new ToolbarActionButton {desc = LocaleManager.locale.StartDay, image = Resources.Load("play-icon") as Texture2D, action = controller.StartDay});
	}

	void OnGUI() {
		if (!enabled) {
			return;
		}
		if (bw == null) {
			bw = new BoxWrapper();
		}
		if (tb == null) {
			tb = new Toolbar();
		}
		if (buttons == null) {
			InitButtons();
		}
		bw.OnGUI(buttons.Count);
		tb.OnGUI(TrimRekt(bw.GetRekt()), buttons);
	}

	void InitButtons() {
		Init (LevelManager.properties);
	}

	Rect TrimRekt(Rect rekt) {
		Rect newRekt = rekt;

		newRekt.yMin = rekt.yMin + 5;
		newRekt.yMax = rekt.yMax - 30;

		return newRekt;
	}

	private class BoxWrapper {

		float xstart = Screen.width/5;
		float ystart = Screen.height - Screen.height/4.5f;
		static Rect nulrekt = new Rect();
		Rect rekt = nulrekt;
		private GUIStyle style = GUI.skin.GetStyle("box");

		public void OnGUI(int buttonCount) {
			if (rekt == nulrekt) {
				//calculate desired width
				int width = (int)(Toolbar.buttonwidth * buttonCount * 1.1f);
				//rekt = new Rect(xstart, ystart, Screen.width-xstart, Screen.height/5);
				rekt = new Rect(Screen.width - width, ystart, width, Screen.height/5);
			}
			style.alignment = TextAnchor.UpperCenter;
			GUI.Box(rekt, "" /*"Select a building to build"*/, style);
		}

		public void ResetRekt() {
			rekt = nulrekt;
		}

		public Rect GetRekt() {
			return rekt;
		}

	}

	private abstract class ToolbarButton {
		public string desc;
		public Texture2D image;
		public bool pressed;

		public abstract void DoAction();
	}

	private class ToolbarBuildableButton : ToolbarButton {
		public Buildable item;
		public override void DoAction () {
			 UserUI.controller.BuildableClicked(item);
		}
	}

	private class ToolbarActionButton : ToolbarButton {
		public Action action;
		public override void DoAction() {
			action();
		}
	}

	private class Toolbar {

		private List<bool> duplicateList;
		private GUIStyle style = GUI.skin.GetStyle("label");
		public static float buttonwidth = Screen.width/10;
		#if UNITY_ANDROID
		private int textSize = 0;
		#endif

		public void OnGUI(Rect where, List<ToolbarButton> buttons) {
			float offset = 5;
			Rect buttonstart = where;
			buttonstart.xMin += offset;
			buttonstart.xMax = buttonstart.xMin+buttonwidth;

			if (duplicateList == null || duplicateList.Count < buttons.Count) {
				duplicateList = new List<bool>(buttons.Count);
				foreach (ToolbarButton b in buttons) {
					duplicateList.Add(false);
				}
			}

			for (int i = 0; i < buttons.Count; ++i) {
				if (GUI.Toggle(buttonstart, buttons[i].pressed, buttons[i].image, "Button")) {
					//Debug.Log("Toggle toggled");
				}

				//Check for drag
				//DODGY AS FUCK!!!!
				if (Input.GetMouseButtonDown(0) && !duplicateList[i]) {
					Vector2 checkAgainst = Input.mousePosition.ToVector2();
					checkAgainst.y = Screen.height - checkAgainst.y;
					if (buttonstart.Contains(checkAgainst)) {
						buttons[i].DoAction();
					}
				}
				duplicateList[i] = Input.GetMouseButtonDown(0);

				Rect labelRect = buttonstart;
				labelRect.center += new Vector2(0, buttonstart.height);
				//HARDCODED - check trimrekt
				labelRect.height = 30;

				style.alignment = TextAnchor.UpperCenter;

				#if UNITY_ANDROID
				if (textSize == 0) {
					textSize = Utils.TextMaximumSize("0000", (int)labelRect.width, (int)labelRect.height, style);
				}
				if (style.fontSize != textSize) {
					style.fontSize = textSize;
				}
				#endif

				GUI.Label(labelRect, buttons[i].desc, style);

				buttonstart.center = buttonstart.center + new Vector2(buttonwidth + offset, 0);
			}
		}

	}

}

