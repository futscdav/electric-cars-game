using UnityEngine;
using System.Collections;

public class LocaleEN : Locale {

	public override string Quit {
		get { return "Quit"; }
	}

	public override string SelectLevel {
		get { return "Level selection"; }
	}

	public override string Return {
		get { return "Go Back"; }
	}

	public override string StartDay {
		get { return "Simulate"; }
	}

	public override string ToggleGrid {
		get { return "Toggle Grid"; }
	}

	public override string RestartLevel {
		get { return "Restart Level"; }
	}

	public override string BackToMenu {
		get { return "To Menu"; }
	}

	public override string BackToConstruction {
		get { return "Construction"; }
	}


	public override string ResourcesFormat {
		get { return "Resources: {0}"; }
	}

	public override string Daytime {
		get { return "Daytime"; }
	}

	public override string Back {
		get { return Return; }
	}

	public override string Undo {
		get { return "Undo"; }
	}

	public override string BatteryRemaining {
		get { return "Battery level"; }
	}

	public override string CarNumber {
		get { return "Car #"; }
	}

	public override string NearestDeparture {
		get { return "Departing"; }
	}

	public override string Never {
		get { return "Never"; }
	}

	public override string InsufficientResources {
		get { return "Insufficient resources!"; }
	}

	public override string InvalidPlacement {
		get { return "Can't do that there!"; }
	}

	public override string LevelComplete {
		get { return "Level complete!"; }
	}

	public override string LevelFailed {
		get { return "Car {0} ran out of juice!"; }
	}

	public override string TryFix {
		get { return "I'll fix it!"; }
	}

	public override string Delete {
		get { return "Delete"; }
	}

	public override string HighScore {
		get { return "Total score"; }
	}

	public override string UploadScore {
		get { return "Upload score!"; }
	}

	public override string UploadFailed {
		get { return "Upload failed."; }
	}

	public override string UploadSuccessful {
		get { return "Upload succssful!"; }
	}

	public override string OK {
		get { return "OK"; }
	}
}
