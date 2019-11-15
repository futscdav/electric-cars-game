using UnityEngine;
using System.Collections;

public abstract class Locale {

	public abstract string Quit { get; }
	public abstract string SelectLevel { get; }
	public abstract string Return { get; }
	public abstract string ToggleGrid { get; }
	public abstract string StartDay { get; }
	public abstract string RestartLevel { get; }
	public abstract string BackToMenu { get; }
	public abstract string BackToConstruction { get; }
	public abstract string ResourcesFormat { get; }
	public abstract string Daytime { get; }
	public abstract string Back { get; }
	public abstract string Undo { get; }
	public abstract string Never { get; }
	public abstract string CarNumber { get; }
	public abstract string NearestDeparture { get; }
	public abstract string BatteryRemaining { get; }
	public abstract string LevelFailed { get; }
	public abstract string TryFix { get; }
	public abstract string LevelComplete { get; }
	public abstract string InsufficientResources { get; }
	public abstract string InvalidPlacement { get; }
	public abstract string Delete { get; }
	public abstract string HighScore { get; }
	public abstract string UploadScore { get; }
	public abstract string UploadSuccessful { get; }
	public abstract string UploadFailed { get; }
	public abstract string OK { get; }
}
