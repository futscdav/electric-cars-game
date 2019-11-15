using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {

	private MenuScript menu;
	private UserUI userui;
	private ClickScript click;
	private GridOverlay grid;
	private ResourceUI resourceui;

	private World world;
	private Builder builder;

	private Camera camera;

	#if UNITY_ANDROID
	private CameraScript camerascript;
	#endif

	// Use this for initialization
	void Start () {
		//Initialize all the variables
		if (!(world == null || menu == null || userui == null || grid == null)) {
			return;
		}
		if (GetComponent<MenuScript>() == null)
			menu = gameObject.AddComponent<MenuScript>();
		if (GetComponent<UserUI>() == null)
			userui = gameObject.AddComponent<UserUI>();
		if (GetComponent<ResourceUI>() == null)
			resourceui = gameObject.AddComponent<ResourceUI>();
		if (GetComponent<Builder>() == null)
			builder = gameObject.AddComponent<Builder>();
		if (GetComponent<ClickScript>() == null)
			click = gameObject.AddComponent<ClickScript>();
		if (grid == null)
			grid = FindObjectOfType<CameraScript>() != null? FindObjectOfType<CameraScript>().overlay : null;
		if (world == null)
			world = FindObjectOfType<World>();
		if (camera == null) {
			camera = Camera.main;
		}
		#if UNITY_ANDROID
		if (camerascript == null) {
			camerascript = FindObjectOfType<CameraScript>();
		}
		#endif
	}

	//Called when the used presses a buildable button on the bottom-right toolbar
	//this really has a deeper value on a non-mobile platform
	public void BuildableClicked(Buildable buildable) {
		//Switch buildable
		if (builder.CurrentBuildable != buildable) {
			builder.CancelCurrent();
			builder.Prebuild(buildable);
			ToggleGrid();
			#if UNITY_ANDROID
			DisableCameraMovement();
			#endif
		} 
		//Same buildable clicked
		else {
			builder.CancelCurrent();
			ToggleGrid();
			#if UNITY_ANDROID
			EnableCameraMovement();
			#endif
		}

	}

	//initialize user ui
	public void SetupUserUI(LevelProperties properties) {
		Start();
		userui.Init(properties);
	}

	//return the remaining resource
	public int GetResource() {
		return Game.Instance.Resource;
	}

	//Tell the game to go back to constucting
	//WARNING - THIS HAS CAUSED SOME PROBLEMS IN THE PAST, IT WORKS NOW,
	//BUT IF A PROBLEM SURROUNDING THIS SHOULD APPEAR, THIS IS WHERE YOU LOOK
	public void ReturnToConstruction() {
		Game.Instance.GoBackToConstruction();
	}

	//build the thing we are currently placing
	public void BuildCurrent() {
		builder.BuildCurrent();
		#if UNITY_ANDROID
		EnableCameraMovement();
		#endif
		ToggleGrid();
	}

	//On click - this is called by the ClickScript
	//here we check for building stuff
	public void OnClick(Vector3 where) {
		//The building of a thing has a grace period (0.2-0.5 seconds)
		#if UNITY_ANDROID
		if (builder.IsBuilding && builder.TimeStarted + 0.2f < Time.realtimeSinceStartup) {
			BuildCurrent();
		}
		#else
		if (builder.IsBuilding && builder.TimeStarted + 0.3f < Time.realtimeSinceStartup) {
			Debug.Log ("Click!");
			BuildCurrent();
		}
		#endif
	}
	
	#if UNITY_ANDROID
	//only really relevant on mobile, though could be also used on pc
	public void OnMouseRelease() {
		if (builder.IsBuilding && builder.TimeStarted + 0.2f < Time.realtimeSinceStartup) {
			BuildCurrent();
		}
		builder.CancelCurrent();
		DisableGrid();
	}
	#endif

	public TimeClass GetWorldTime() {
		return world.time.time;
	}

	public void EnableConstructionUI() {
		EnableUserUI();
		userui.SetConstructionMode();
	}

	public void EnableSimulationUI() {
		EnableUserUI();
		userui.SetSimulationMode();
	}

	//back to main menu
	public void LoadMenu() {
		Application.LoadLevel("menu");
	}

	public void StartDay() {
		Game.Instance.StartDay();
	}

	//reload this level (static class is not destroyed)
	public void RestartLevel() {
		Application.LoadLevel(1);
	}
	
	public void EnableUserUI() {
		Start ();
		userui.enabled = true;
	}

	public void DisableUserUI() {
		builder.CancelCurrent();
		userui.enabled = false;
	}

	public void SetConstructionCamera() {
		Start ();
		if (camera.GetComponent<SimulationCamera>() != null)
			camera.GetComponent<SimulationCamera>().enabled = false;

		//THIS ENABLES CAMERA MOVEMENT BY PLAYER !!!
		if (camera.GetComponent<CameraScript>() != null) {
			camera.GetComponent<CameraScript>().enabled = true;
		}
	}

	public void SetSimulationCamera() {
		Start ();
		if (camera.GetComponent<SimulationCamera>() != null)
			camera.GetComponent<SimulationCamera>().enabled = true;

		//THIS DISABLES CAMERA MOVEMENT BY PLAYER !!!
		if (camera.GetComponent<CameraScript>() != null) {
			camera.GetComponent<CameraScript>().enabled = false;
		}
	}

	public void ToggleGrid() {
		if (grid != null)
			grid.enabled = !grid.enabled;
	}

	#if UNITY_ANDROID
	//Enable and disable panning when constructing (obviously)
	public void DisableCameraMovement() {
		camerascript.moveCamera = false;
	}

	public void EnableCameraMovement() {
		camerascript.moveCamera = true;
	}
	#endif

	public void SetGridBox(Rect box) {
		if (grid != null)
			grid.box = box;
	}

	//undo last building placement
	public void UndoAction() {
		builder.UndoAction();
	}

	public void EnableGrid() {
		if (grid != null)
			grid.enabled = true;
	}

	public void DisableGrid() {
		if (grid != null)
			grid.enabled = false;
	}
}
