using UnityEngine;
using System.Collections;
using SimpleJSON;

public class Tutorial : Awakeable {

	//Awakeable tutorial class
	//Represents ingame tutorial that can be shown at arbitrary time

	public Texture2D texture;
	public float screenWidthPortion;
	public float screenHeightPortion;
	public float screenLeftPosition;
	public float screenTopPosition;
	public TutorialPresenter presenter;

	Rect position;

	public Tutorial() {}

	//Parse from json
	public Tutorial(JSONNode tutorialObject) {
		texture = Resources.Load<Texture2D>(tutorialObject["resource"]);

		screenWidthPortion = tutorialObject["screenwidth"].AsFloat;
		screenHeightPortion = tutorialObject["screenheight"].AsFloat;
		screenLeftPosition = tutorialObject["screenleft"].AsFloat;
		screenTopPosition = tutorialObject["screentop"].AsFloat;

		position = new Rect(Screen.width * screenLeftPosition, Screen.height * screenTopPosition, 
		                    Screen.width * screenWidthPortion, Screen.height * screenHeightPortion);
	}

	//Awakeable implementation
	public void WakeUp() {
		if (!presenter.enabled) {
			Debug.Log("Tutorial not shown because the presenter is disabled.");
			return;
		}
		presenter.Present(texture, position);
	}


}

