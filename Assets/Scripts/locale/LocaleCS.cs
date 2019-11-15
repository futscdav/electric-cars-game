using UnityEngine;
using System.Collections;

public class LocaleCS : Locale {

	public override string Quit {
		get { return "Konec"; }
	}

	public override string SelectLevel {
		get { return "Vybrat úroveň!"; }
	}

	public override string Return {
		get { return "Zpět"; }
	}

	public override string StartDay {
		get { return "Začít den"; }
	}

	public override string ToggleGrid {
		get { return "Zobrazit mřížku"; }
	}

	public override string RestartLevel {
		get { return "Reset úrovně"; }
	}

	public override string BackToMenu {
		get { return "Zpět do menu"; }
	}

	public override string BackToConstruction {
		get { return "Stavět"; }
	}


	public override string ResourcesFormat {
		get { return "Zdroje: {0}"; }
	}

	public override string Daytime {
		get { return "Čas"; }
	}

	public override string Back {
		get { return Return; }
	}

	public override string Undo {
		get { return "Zpět"; }
	}

	public override string BatteryRemaining {
		get { return "Zbývá baterie"; }
	}

	public override string CarNumber {
		get { return "Číslo auta"; }
	}

	public override string NearestDeparture {
		get { return "Odjezd"; }
	}

	public override string Never {
		get { return "Nikdy"; }
	}

	public override string InsufficientResources {
		get { return "Nedostatek zdrojů!"; }
	}

	public override string InvalidPlacement {
		get { return "Neplatné umístění!"; }
	}

	public override string LevelComplete {
		get { return "Úroveň dokončena!"; }
	}

	public override string LevelFailed {
		get { return "Autu číslo {0} došla baterie!"; }
	}

	public override string TryFix {
		get { return "Zkusím to napravit!"; }
	}

	public override string Delete {
		get { return "Odstranit"; }
	}

	public override string HighScore {
		get { return "Tvé celkové skóre"; }
	}

	public override string UploadScore {
		get { return "Nahraj skóre!"; }
	}

	public override string UploadFailed {
		get { return "Nahrání se nezdařilo."; }
	}

	public override string UploadSuccessful {
		get { return "Nahrání proběhlo v pořádku."; }
	}

	public override string OK {
		get { return "OK"; }
	}
}
