using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Game : MonoBehaviour {

	//the world object
	public World world;

	//localization
	public static Locale locale;

	//This is a "singleton"
	private static Game inst;
	public static Game Instance {
		get {
			if (inst == null) 
				inst = FindObjectOfType<Game>(); 
			return inst;
		}
	}

	void OnDestroy() {
		//reset instance when destroyed
		inst = null;
	}

	//Current remaining resource
	private int resource;
	public int Resource {
		get {
			return resource;
		}
	}

	//Current game phase
	public Phase phase;
	//ui controller
	public UIController ui;
	//tutorial presenter
	public TutorialPresenter tutpresenter;

	//a collection of cars that have finished - so we can check for level complete
	//if you ever want to implement a system that lets the level decide when its finished
	//instead of the game, move this logic into the LevelScript category and simply call
	//a check function in this class (update or have it as a callback)
	public List<Car> finishedCars;

	//currently assigned LevelScript, this is for special effects in different levels
	//such as having a preconnected tuple or special things happen when construction / simulation is entered
	public LevelScript levelScript;
	
	public enum Phase {
		Construction,
		Paused,
		Simulation,
		Review
	}

	//this is the info ui which shows the messages on screen.
	//this reference should very likely be in the UI controller instead
	private GameInfoUI gameui;

	void Awake() {
		//Set the Instance property
		if (inst != null) {
			Debug.LogError("Something is wrong");
		}
		inst = this;
	}
	
	void Start () {
		//Change gravity direction
		//this should have no effect anymore
		//Physics2D.gravity = new Vector3(0,0,-1);

		//Aplication settings
		#if UNITY_ANDROID
		//do not turn the screen off while the game is running
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		#endif
		//do we run the application while its in the background? 
		//Generally no, however, in a PC build, this is useful to have set to true
		Application.runInBackground = false;

		//Create world object
		world = gameObject.AddComponent<World>();
		//find the UI object (added in inspector)
		ui = FindObjectOfType<UIController>();
		//Let the world create references to its variables
		world.Init();

		//Game setup
		//this order is not arbitrary by the way - World has to be created before terrain and before cars
		//this creates the world - roads, buildings, etc
		world.CreateWorld(string.Format("Levels/{0}/world", LevelManager.level));
		//this creates the terrain - water, sidewalks, etc
		world.CreateTerrain(string.Format("Levels/{0}/terrain", LevelManager.level));
		//this creates the cars - loads their starting points, their tanks, trips, etc
		CarFactory.CurrentSerialNumber = 1;
		world.SpawnCars(string.Format("Levels/{0}/cars", LevelManager.level));
		//this creates the textures around the world
		world.SurroundWorld();

		//Set up tutorials if they are available
		TutorialPresenter presenter = gameObject.AddComponent<TutorialPresenter>();
		presenter.Prepare(Resources.Load<TextAsset>(string.Format("Levels/{0}/tutorials", LevelManager.level)));
		tutpresenter = presenter;

		//Load the level properties - see LevelProperties for a full list
		LevelProperties level = LevelProperties.Parse(Utils.ReadText(string.Format("Levels/{0}/props", LevelManager.level)));
		if (level is null) {
			Debug.Log("Failed to parse level.");
		}

		//Add level script if its available (otherwise add generic script)
		if (!(level.ScriptName is null)) {
			levelScript = (LevelScript) gameObject.AddComponent(Type.GetType(level.ScriptName));
		}
		if (levelScript is null) {
			Debug.Log("Adding a generic level script");
			levelScript = gameObject.AddComponent<GenericLevelScript>();
		}
	
		//Set up stuff
		resource = level.MaxResource;
		LevelManager.properties = level;
		ui.SetupUserUI(level);

		Debug.Log ("World is created and prepared");
		//Create the GameInfoUI
		gameui = gameObject.AddComponent<GameInfoUI>();

		//Call level script event
		levelScript.OnLevelLoaded();
	}

	public void WorldReady() {
		//Dirty hack to make menu work 100% of the time
		if (phase == Phase.Simulation) {
			SwitchPhase(Phase.Construction);
			SwitchPhase(Phase.Simulation);
		}
		else
			SwitchPhase(Phase.Construction);
	}

	public void Warn(string msg, float duration = 3f) {
		//hide previous warning
		HideWarning();
		//warning is yellow
		gameui.ShowMessage(msg, Color.yellow);
		//passed since start of fading out
		passed = 0;
		//stop previous fading out
		StopCoroutine("FadeInfoUI");
		//start fading the info out
		StartCoroutine("FadeInfoUI", duration);
	}

	private float passed = 0;
	IEnumerator FadeInfoUI(float duration) {
		while (true) {
			passed += Time.deltaTime;
			gameui.SetAlpha(Mathf.Lerp(1, 0, passed/duration));
			if (passed >= duration) {
				HideWarning();
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	public void HideWarning() {
		//stop fading out
		StopCoroutine("FadeInfoUI");
		//destroy the message
		gameui.HideMessage();
	}

	public void OnCarOutOfCharge(Car c) {
		Debug.Log(c + " called game event");
		//Let the user know what happened
		HideWarning();
		gameui.SetButton(LocaleManager.locale.TryFix, GoBackToConstruction);
		gameui.ShowMessage(string.Format(LocaleManager.locale.LevelFailed, c.serialNumber), Color.red);

		//Move the camera to the car which ran out
		//This does not currently work, as CameraScript is no longer used in simulation
		//Change this to SimulationCamera
		/*FindObjectOfType<CameraScript>().MoveTo(new Vector3(c.transform.position.x, c.transform.position.y, Camera.main.transform.position.z));*/

		//Switch over to the review phase
		SwitchPhase(Phase.Review);
		//Call event on level script
		levelScript.OnGameOver();
	}

	public void GoBackToConstruction() {
		gameui.HideMessage();
		//reset all cars to their "factory settings"
		for (int i = world.vehicles.Count - 1; i >= 0; --i) {
			//Need to have stuff released right away, which would be easily acoomplished by
			//calling destroyimmediate on the car, however, that seems to crash the unity player
			world.vehicles[i].RemoveFromGame();
			Destroy(world.vehicles[i].gameObject);
		}
		world.vehicles.Clear();
		//spawn cars again
		CarFactory.CurrentSerialNumber = 1;
		world.SpawnCars(string.Format("Levels/{0}/cars", LevelManager.level));

		//set day to its begining
		//this will invalidate all remaining tutorials, the workaround would be to load them again
		//or persist them in another way, is it necessary?
		Destroy(world.time);
		world.time = gameObject.AddComponent<Daytime>();
		world.SetTime(LevelManager.properties.DayStart);

		//switch to construction
		SwitchPhase(Phase.Construction);

	}

	public void OnCarFinished(Car c) {
		Debug.Log(c + " finished");
		if (finishedCars == null) {
			finishedCars = new List<Car>();
		}

		//add it to the finished list
		finishedCars.Add(c);

		foreach (Car wc in world.vehicles) {
			if (!finishedCars.Contains(wc)){
				//car exists that is not in the finished list
				return;
			}
		}

		//go to review phase
		SwitchPhase(Phase.Review);

		/* Save data */
		Debug.Log ("Serializing data");

		LevelData data = new LevelData();
		data.complete = true;
		//this is not currently used
		data.score = LevelManager.properties.MaxResource - resource;
		LevelManager.SaveLevel(data);

		Debug.Log("Data serialized");

		/* Data saved */

		HideWarning();

		//let the player know
		gameui.SetButton(LocaleManager.locale.BackToMenu, ui.LoadMenu);
		gameui.ShowMessage(LocaleManager.locale.LevelComplete, Color.green);

		//call level script event
		levelScript.OnGameWon();
	}

	public void StartDay() {
		//Switch to simulation (this currently works much like Resume())
		SwitchPhase(Game.Phase.Simulation);
	}

	//Subtract resource if its available (return value)
	public bool SubtractResource(int amount) {
		if (resource - amount >= 0) {
			resource -= amount;
			return true;
		}
		return false;
	}

	public void SwitchPhase(Phase p) {
		phase = p;
		switch (p) {
		case Phase.Construction : {
			//Show construction gui
			ui.EnableConstructionUI();
			ui.SetGridBox(world.GetBoundingBox());
			ui.DisableGrid();
			ui.SetConstructionCamera();

			world.SetDaylight();

			levelScript.OnConstruction();

			break;
		}
		case Phase.Simulation : {
			//Start day and hide construction gui
			ui.DisableGrid();
			ui.SetSimulationCamera();
			ui.EnableSimulationUI();

			world.SetLightByTime();

			levelScript.OnSimulation();

			break;
		}
		case Phase.Paused : {
			ui.DisableUserUI();
			break;
		}
		case Phase.Review : {
			//Show some stat gui perhaps?
			ui.DisableUserUI();
			ui.SetConstructionCamera();
			break;
		}
		}
	}
	
	public void Pause() {
		SwitchPhase(Phase.Paused);
	}

	public void Resume() {
		if (phase != Phase.Paused) {
			return;
		}
		SwitchPhase(Phase.Simulation);
	}
	
	static int sc;
	//For capturing screenshots on PC platform
	void Update () {
		if (Input.GetKeyDown(KeyCode.F9)) {
			ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + ++sc + "_sc.jpg");
		}
	}

	void LateUpdate() {
		WorldTimeControl();
	}

	//when car continues its drive, set timescale
	public void CarWokenUp() {
		world.timescale = 1;
	}

	void WorldTimeControl() {
		List<Car> cars = world.vehicles;
		bool speedup = true;
		foreach (Car c in cars) {
			if (c.state != Car.CarState.waiting && c.state != Car.CarState.parked) {
				speedup = false;
				break;
			}
		}

		//no car is doing anyting interesting
		if (speedup) {
			world.timescale = 60;
		} else {
			world.timescale = 1;
		}
	}
}
