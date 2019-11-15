using UnityEngine;
using System.Collections;

public class ResourceUI : MonoBehaviour {

	/* Class responsible for the right hand side upper UI (resources + time) */

	Camera cam;
	GameObject coin;
	GUISkin skin;
	UIController controller;

	// Use this for initialization
	void Start () {
		skin = Resources.Load("GUISkin") as GUISkin;
		controller = FindObjectOfType<UIController>();
	}

	void OnGUI() {
		if (skin == null) {
			Start();
		}
		DrawResourceGUI();
	}

	Rect GetResourceGUIArea() {
		return new Rect(Screen.width - Screen.width/3f, Screen.height/20f, Screen.width/4f, Screen.height/6f);
	}

	#if UNITY_ANDROID
	int labelSize = 0;
	int daytimeTextBoxSize = 0;
	int daytimeBoxSize = 0;
	#endif

	void DrawResourceGUI() {
		Rect left = GetResourceGUIArea();
		left.width /= 1.4f;
		left.height /= 3;

		Rect right = GetResourceGUIArea();
		right.width /= 2;
		right.height /= 1.5f;
		right.center += Vector2.right * right.width * 1.05f;

		//fix left
		left.center = new Vector2(right.center.x - left.width * 0.9f, left.center.y);

		GUILayout.BeginArea(left, skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		#if UNITY_ANDROID
		if (labelSize == 0) {
			labelSize = Utils.TextMaximumSize("Resources: 000000", (int)(left.width/1.05f), (int)left.height, skin.label);
		}
		if (skin.label.fontSize != labelSize) {
			skin.label.fontSize = labelSize;
		}
		#endif
		GUILayout.Label(string.Format(LocaleManager.locale.ResourcesFormat, controller.GetResource()), skin.label);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();


		GUILayout.BeginArea(right, skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		#if UNITY_ANDROID
		if (daytimeBoxSize == 0 || daytimeTextBoxSize == 0) {
			daytimeTextBoxSize = Utils.TextMaximumSize("Daytime", (int)right.width, (int)(right.height/2.05f), skin.box);
			daytimeBoxSize = Utils.TextMaximumSize(controller.GetWorldTime().ToString(), (int)right.width, (int)(right.height/2.05f), skin.box);
		}
		if (skin.box.fontSize != labelSize) {
			skin.box.fontSize = labelSize;
		}
		#endif
		GUILayout.Box(LocaleManager.locale.Daytime, skin.box);
		#if UNITY_ANDROID
		if (skin.box.fontSize != labelSize) {
			skin.box.fontSize = labelSize;
		}
		#endif
		GUILayout.Box(controller.GetWorldTime().ToString(), skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

}
