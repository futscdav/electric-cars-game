using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MainMenuUI : MonoBehaviour {

	//UI skin
	public GUISkin skin;
	//Logo in the main menu
	public Sprite logo;
	//tick for completed levels
	public Texture2D tick;

	public enum MenuState {
		MainScreen,
		LevelSelection
	}

	public MenuState state;

	void Awake() {
		if (!skin) {
			//same goes for logo and tick
			Debug.LogError("Please assign a skin on the Inspector.");  
			return;
		}
		//Set main menu level
		LevelManager.level = 8;
	}

	bool BackgroundSimulationInit = false;

	void Start() {
		//This loads level data + score into memory from files (do it now so it doesn't jam later on)
		LevelManager.GetLevelData();
	}

	void Update() {
		if (!BackgroundSimulationInit) {
			BackgroundSimulationInit = true;
			Car[] cars = FindObjectsOfType<Car>();

			//modify car behavior
			foreach (Car c in cars) {
				c.usingFuel = false;
				Destroy(c.GetComponent<TravelPlan>());
				c.plan = c.gameObject.AddComponent<RandomPlan>();
			}
			//destroy game ui
			Destroy(GameObject.Find("UI"));

			//disable any tutorials
			if (Game.Instance.GetComponent<TutorialPresenter>()) {
				Game.Instance.GetComponent<TutorialPresenter>().enabled = false;
			}
			
			//Attach random follow camera
			RandomFollowCamera cam = GameObject.Find("Main Camera").AddComponent<RandomFollowCamera>();
			List<GameObject> objs = new List<GameObject>();
			foreach (Car c in Game.Instance.world.vehicles) {
				objs.Add(c.gameObject);
			}
			cam.list = objs;

			//start simulation
			Game.Instance.SwitchPhase(Game.Phase.Simulation);
		}
	}

	void OnGUI() {
		GUI.depth = 2;
		//render logo
		DrawLogo();
		switch(state) {
		case MenuState.MainScreen : {
			//render main screen
			DrawMainScreen();
			break;
		}
		case MenuState.LevelSelection : {
			//render level selection
			DrawLevelSelection();
			break;
		}
		}
	}

	void DrawLogo() {
		GUILayout.BeginArea(GetLogoRect());
		GUILayout.Box(logo.texture, skin.customStyles[1]);
		GUILayout.EndArea();
	}

	int selectionSize = 0;

	void DrawLevelSelection() {
		Rect area = GetMenuRect();

		GUILayout.BeginArea(area, skin.box);
		GUILayout.FlexibleSpace();

		//Level selection
		//Level enumeration
		int levelcount = LevelManager.EnumerateLevels();
		int levelrow = 5;
		int rows = levelcount/levelrow + levelcount%levelrow != 0 ? 1 : 0;
		bool isHorizontal = false;

		#if UNITY_ANDROID
		//Button text size;
		if (selectionSize == 0) {
			selectionSize = Utils.TextMaximumSize("00", (int)area.width/(levelrow+2), (int)area.height/(rows+6), skin.button);
		}
		if (skin.button.fontSize != selectionSize) { 
			skin.button.fontSize = selectionSize;
			//skin.label.fontSize = selectionSize;
		}
		#endif

		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(LocaleManager.locale.HighScore + ": " + LevelManager.scoreSum, skin.label);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		for (int i = 1; i <= levelcount; ++i) {
			GUILayout.FlexibleSpace();
			if (i%levelrow == 1) {
				GUILayout.BeginHorizontal();
				isHorizontal = true;
				GUILayout.FlexibleSpace();
			}

			if (GUILayout.Button(string.Format("{0}", i), skin.button, 
			                     GUILayout.MinWidth(area.width/(levelrow+2)), GUILayout.MinHeight(area.height/(rows+6)))) {
				LevelManager.level = i;
				Application.LoadLevel("scene1");
			}

			//if complete
			LevelData data = LevelManager.GetLevelData(i);
			if (data.complete) {
				Rect tickr = GUILayoutUtility.GetLastRect();
				tickr.center = new Vector2(tickr.xMax, tickr.yMax);
				tickr.height /= 2;
				tickr.width = tickr.height;
				GUI.DrawTexture(tickr, tick);
			}


			if (i%levelrow == 0) {
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				isHorizontal = false;
			}
			GUILayout.FlexibleSpace();
		}
		if (isHorizontal) {
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndVertical();

		//Return
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button(LocaleManager.locale.Return, skin.button, GUILayout.MaxWidth(area.width/2),
		                     GUILayout.MinHeight(area.height/8))) {
			state = MenuState.MainScreen;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();

		GUILayout.EndArea();

	}

	#if UNITY_ANDROID
	int buttonTextSize = 0;
	#endif

	void DrawMainScreen() {
		Rect area = GetMenuRect();

		//Text size
		#if UNITY_ANDROID
		if (buttonTextSize == 0) {
			string[] texts = {"00"};
			buttonTextSize = Utils.TextMaximumSize(texts[0], (int)area.width/2, (int)area.height/8, skin.button);
			for (int i = 1; i < texts.Length; ++i) {
				int x = Utils.TextMaximumSize(texts[i], (int)area.width/2, (int)area.height/8, skin.button);
				if (x < buttonTextSize) {
					buttonTextSize = x;
				}
			}
		}
		if (skin.button.fontSize != buttonTextSize) {
			skin.button.fontSize = buttonTextSize;
			skin.label.fontSize = buttonTextSize;
		}
		#endif

		GUILayout.BeginArea(area, skin.GetStyle("box"));

		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(LocaleManager.locale.HighScore + ": " + LevelManager.scoreSum, skin.label);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button(LocaleManager.locale.SelectLevel, skin.button, GUILayout.MaxWidth(area.width/2),
		                     GUILayout.MinHeight(area.height/5))) {
			state = MenuState.LevelSelection;
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button(LocaleManager.locale.UploadScore, skin.button, GUILayout.MaxWidth(area.width / 2),
		                     GUILayout.MinHeight(area.height/5))) {
			bool success = true;
			string name = "";
			try {
				name = NameRetriever.GetName();
			} catch (System.Exception e) {
				Debug.Log(e.Message);
				success = false;
			}
			if (name != "" && name != null) {
				ScoreUploader su = new ScoreUploader();
				success &= su.SendScore(name, (int)LevelManager.scoreSum);
			}
			if (!success) {
				UiInfo(LocaleManager.locale.UploadFailed, Color.red);
				Debug.Log("Failed to upload score");
			}
			else {
				UiInfo(LocaleManager.locale.UploadSuccessful, Color.green);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button(LocaleManager.locale.Quit, skin.button, GUILayout.MaxWidth(area.width / 2),
		                     GUILayout.MinHeight(area.height/5))) {
			Application.Quit();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		
		GUILayout.EndArea();
	}

	void UiInfo(string mes, Color col) {
		GameInfoUI info = gameObject.AddComponent<GameInfoUI>();
		info.SetButton(LocaleManager.locale.OK, () => { info.HideMessage(); Destroy(info); });
		info.ShowMessage(mes, col);
	}

	Rect GetLogoRect() {
		Rect area = GetMenuRect();
		area.center -= new Vector2(0, area.height/1.5f);
		return area;
	}

	Rect GetMenuRect() {
		Rect area = new Rect(0,0,Screen.width/1.5f,Screen.height/2);
		area.center = new Vector2(Screen.width/2, Screen.height/2);
		return area;
	}

}

