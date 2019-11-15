using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {

	UIController controller;

	bool drawTime = true;

	public bool enabled = true;

	/* This is the class responsible for the left hand side menu openable with the little arrow */

	void Start () {
		controller = GetComponent<UIController>();
	}

	void OnGUI() {
		if (!enabled) {
			return;
		}
		style = GUI.skin.GetStyle("button");
		DrawGUI();
	}

	void DrawGUI() {
		if (menuOpen || initial) {
			DrawMenu();
		} else {
			DrawArrow();
		}
	}

	void DrawArrow() {
		GUILayout.BeginArea(MenuArea());

		if (GUILayout.Button(">")) {
			menuOpen = true;
		}

		GUILayout.EndArea();
 	}

	Rect MenuArea() {

		float xMenuMin = ((float)Screen.width)/20;
		float yMenuMin = ((float)Screen.height)/20;
		float xMenuSize = ((float)Screen.width)/6;
		float yMenuSize = ((float)Screen.width)/4;
		/*float xMenuMax = xMenuMin + xMenuSize;
		float yMenuMax = yMenuMin + yMenuSize;*/

		return new Rect(xMenuMin,yMenuMin,xMenuSize,yMenuSize);
	}
	
	private bool menuOpen = false;
	private bool initial = true;
	private Vector2 buttonsize = Vector2.zero;
	private GUIStyle style;
	private int buttonTextSize = 0;

	void DrawMenu() {

		if (initial) {
			initial = false;
		}

		int numbuttons = 5;
		Rect area = MenuArea();
		float height = area.height - numbuttons * 5;

		buttonsize = new Vector2(area.width - area.width/10, height/numbuttons);

		GUILayout.BeginArea(area);

		if (GUILayout.Button("<")) {
			menuOpen = false;
		}

		string[] texts = { LocaleManager.locale.ToggleGrid, LocaleManager.locale.StartDay, LocaleManager.locale.RestartLevel,
							 LocaleManager.locale.BackToMenu, LocaleManager.locale.BackToConstruction };

		#if UNITY_ANDROID
		if (buttonTextSize == 0) {
			style.fontSize = Utils.TextMaximumSize(texts[0], (int)buttonsize.x, (int)buttonsize.y, style);
			foreach (string btext in texts) {
				int size = Utils.TextMaximumSize(btext, (int)buttonsize.x, (int)buttonsize.y, style);
				if (size < style.fontSize) {
					style.fontSize = size;
				}
			}
			buttonTextSize = style.fontSize;
		}
		#endif

		string text = "";

		//Toggle grid
		/*text = "Toggle Grid";
		if (GUILayout.Button(text, style, GUILayout.Height(height/numbuttons))) {
			controller.ToggleGrid();
		}*/

		//Start day
		/*text = "Start Day";
		if (GUILayout.Button(text, style, GUILayout.Height(height/numbuttons))) {
			controller.StartDay();
		}*/

		//Restart
		text = LocaleManager.locale.RestartLevel;
		if (GUILayout.Button(text, style, GUILayout.Height(height/numbuttons))) {
			controller.RestartLevel();
		}

		if (Game.Instance.phase == Game.Phase.Simulation) {
			text = LocaleManager.locale.BackToConstruction;
			if (GUILayout.Button(text, style, GUILayout.Height(height/numbuttons))) {
				Game.Instance.GoBackToConstruction();
			}
		}

		//Exit
		text = LocaleManager.locale.BackToMenu;
		if (GUILayout.Button(text, style, GUILayout.Height(height/numbuttons))) {
			controller.LoadMenu();
		}
		
		GUILayout.EndArea();
	}
}
