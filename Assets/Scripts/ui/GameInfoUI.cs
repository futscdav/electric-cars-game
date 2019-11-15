using UnityEngine;
using System.Collections;
using System;

public class GameInfoUI : MonoBehaviour {
	private bool showmessage;
	private string message;
	private string buttontext;
	private Action buttonaction;
	private Color color = Color.blue;
	private float alpha = 0f;
	private int messageSize = 0;

	//Shows a message in the MessageArea rectangle
	public void ShowMessage(string text, Color color) {
		showmessage = true;
		message = text;
		this.color = color;
		SetAlpha(1);
	}

	//hides the message and gets rid of it
	public void HideMessage() {
		showmessage = false;
		buttonaction = null;
		buttontext = null;
		messageSize = 0;
	}

	//set aplha of the box
	public void SetAlpha(float alpha) {
		this.alpha = alpha;
	}

	//render required parts
	void OnGUI() {
		if (showmessage) {
			int depth = GUI.depth;
			Color old = GUI.color;
			GUI.color = new Color(old.r, old.g, old.b, alpha);
			GUI.depth = depth - 1;
			PrintMessage();
			//if there is a button associated with the message, render it too
			if (buttonaction != null) {
				DrawButtons();
			}
			GUI.color = old;
			GUI.depth = depth;
		}
	}

	public void SetButton(string text, Action func) {
		buttontext = text;
		buttonaction = func;
		#if UNITY_ANDROID
		buttonTextSize = 0;
		#endif
	}

	GUIStyle myStyle = null;

	void PrintMessage() {
		if (myStyle == null) {
			GUIStyle style = GUI.skin.GetStyle("box");
			myStyle = new GUIStyle(style);
			myStyle.fontStyle = FontStyle.Bold;
			myStyle.normal.textColor = color;
			myStyle.alignment = TextAnchor.MiddleCenter;
		}
		if (myStyle.normal.textColor != color) {
			myStyle.normal.textColor = color;
		}
		if (messageSize == 0) {
			messageSize = Utils.TextMaximumSize(message, (int)(MessageArea().width / 1.1f), (int)MessageArea().height, myStyle);
		}
		if (myStyle.fontSize != messageSize) {
			myStyle.fontSize = messageSize;
		}

		GUI.Box(MessageArea(), message, myStyle);
	}

	#if UNITY_ANDROID
	int buttonTextSize = 0;
	#endif
	void DrawButtons() {
		#if UNITY_ANDROID
		if (buttonTextSize == 0) {
			buttonTextSize = Utils.TextMaximumSize(buttontext,(int) ButtonArea().width, (int)ButtonArea().height, GUI.skin.button);
		}
		int old = GUI.skin.button.fontSize;
		if (GUI.skin.button.fontSize != buttonTextSize) {
			GUI.skin.button.fontSize = buttonTextSize;
		}
		#endif
		if (GUI.Button(ButtonArea(), buttontext)) {
			buttonaction();
		}
		#if UNITY_ANDROID
		GUI.skin.button.fontSize = old;
		#endif
	}

	static Rect? messageRekt = null;
	Rect MessageArea() {

		if (messageRekt != null) {
			return messageRekt.Value;
		}

		float height = Screen.height / 5;
		float width = Screen.width / 1.2f;

		messageRekt = new Rect?(new Rect(Screen.width/2 - width/2, Screen.height/2, width, height));

		return MessageArea();
	}

	static Rect? buttonRekt = null;
	Rect ButtonArea() {

		if (buttonRekt != null) {
			return buttonRekt.Value;
		}

		Rect mes = MessageArea();
		mes.width /= 3;
		mes.center = MessageArea().center + Vector2.up * 5;
		mes.y += MessageArea().height ;// 1.75f;
		mes.height /= 1.5f;
		#if !UNITY_ANDROID
		mes.height /= 2f;
		#endif

		buttonRekt = new Rect?(mes);

		return ButtonArea();
	}

}

