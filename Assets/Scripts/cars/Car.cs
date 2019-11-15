using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour, BubbleOwner {

	#region variables

	/* DRAWING */
	private bool drawJourney = false;
	//Draw journey property
	public bool DrawJourney {
		get {return drawJourney;}
		set {drawJourney = value; if (!drawJourney)DisableJourneyLines();}
	}
	//List of linerenderers for easy reuse
	private List<LineRenderer> journeyLines = null;

	/* CONTROLS */
	public Collider2D collider;
	public Road road;
	public TravelPlan plan;
	public CarState state;

	/* INFO */
	public int serialNumber;
	private bool infoShown;
	private SpeechBubble infoBubble;
	private string infoString;

	/* STATIC */
	private static GameObject lineprefab = null;

	/* SIMULATION VARS */
	//the direction car WANTS to go
	public Vector2 directionHeading;
	//the direction car is ACTUALLY FACING
	public Vector2 head;
	//current waypoint the car is driving to
	public Waypoint currentWaypoint;

	//maxspeed and currentspeed should be the same for most purposes
	public float maxSpeed;
	public float currentSpeed;
	//used for dynamic collision avoidance checking
	public float waitingFor = 0f;
	//remaining charge
	public float tankLevel = .5f;

	private CollisionAvoidance avoidance = CollisionAvoidance.dynamicc;
	//station for when the car is charging
	private PowerStationRoad station;
	private World world;

	/* MENU SIMULATION VAR */
	//this is used for the menus simulation - the cars do not need to recharge there (bit of a hack)
	public bool usingFuel = true;

	public enum CollisionAvoidance {
		disabled,
		staticc,
		dynamicc
	}

	//mostly only waiting, driving, charging, parked, injam and outofcharge are used
	public enum CarState {
		waiting,
		driving,
		charging,
		parked,
		injam,
		outofcharge
	}

	#endregion

	#region instance methods

	void Awake() {
		//Load the journey line prefab if it's not yet loaded
		if (lineprefab == null) {
			lineprefab = (GameObject) Resources.Load("LineRenderRoad");
		}
	}

	//Spaghetti code method (DestroyImmediate crashes unity for some reason :( )
	public void RemoveFromGame() {
		TravelPlan plan = GetComponent<TravelPlan>();
		plan.RemoveFromGame();
	}
	
	void Start () {
		//The collider of the car - used for collision avoidance & clicking on the car
		collider = GetComponent<BoxCollider2D>();
		//put the car into waiting state (which should be the default, but just to be sure)
		state = CarState.waiting;
		//find the reference to the world
		world = FindObjectOfType<World>();
	}

	void Update() {
		//Draw journey if enabled
		if (drawJourney) {
			DrawJourneyLines();
		}
		//Draw bubble with info if enabled
		if (infoShown) {
			UpdateInfoString();
			infoBubble.text = infoString;
		}
	}

	//The inside the #if blocks is for car performance measuring
	#if DEVELOPMENT_BUILD || UNITY_EDITOR
	static float total = 0;
	static float collisions = 0;
	static float realTimeMeasureStart = 0; // Time.realtimeSinceStartup;
	#endif
	void FixedUpdate() {
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
		float start = Time.realtimeSinceStartup;
		#endif
		UpdateVariables();
		switch (state) {
		case CarState.waiting : {
			break;
		}
		case CarState.parked : {
			break;
		}
		case CarState.driving : {
			int timeDelta = world.deltaTime();
			//This value is empiric, change it at your own risk
			//Or if you want to make cars use fuel at arbitrary rates
			UpdateTank( - currentSpeed * 1f/1500f * timeDelta);
			Drive(timeDelta);
			break;
		}
		case CarState.charging : {
			int timeDelta = world.deltaTime();
			//Same as above, affects the speed at which the tank is filled
			UpdateTank(currentSpeed * 1f/250 * timeDelta);
			//Could end sooner than at 100%?
			if (tankLevel >= 1f) {
				tankLevel = 1f;
				//Stop playing the "animation"
				StopCoroutine("FaceObject");
				//Let plan know we are finished with charging
				plan.RechargeComplete();
			}
			break;
		}
		case CarState.injam : {
			//this is for when the car is avoiding a collision
			waitingFor += Time.deltaTime;
			//check whether the collision is not imminent anymore
			if (!AvoidCollision()) {
				GoToState(CarState.driving);
			}
			break;
		}
		case CarState.outofcharge : {
			break;
		}
		}

		//Output the profiling information
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
		float end = Time.realtimeSinceStartup;
		total += (end - start);
		if (total > 0.5f) {
			Debug.Log("Spent " + total + " in fixed update.");
			Debug.Log(collisions + " of which were checking for collisions");
			Debug.Log("Which is roughly " + ((collisions/total)*100) + "%");
			Debug.Log("Fixed update is roughtly " + ((total/(Time.realtimeSinceStartup-realTimeMeasureStart))*100) + "% of the runtime.");
			total = 0;
			collisions = 0;
			realTimeMeasureStart = Time.realtimeSinceStartup;
		}
		#endif
	}

	public void GoToState(CarState newstate, params object[] list) {
		if (newstate == this.state) {
			return;
		}
		if (this.state == CarState.outofcharge) {
			Debug.LogError("Trying to move car out of charge");
		}

		switch (newstate) {
		case CarState.waiting : {
			waitingFor = 0f;
			break;
		}
		case CarState.parked : {
			break;
		}
		case CarState.driving : {
			break;
		}
		case CarState.charging : {
			break;
		}
		case CarState.injam : {
			waitingFor = 0f;
			break;
		}
		}
		state = newstate;
	}

	//Updates the current charge level;
	void UpdateTank(float amount) {
		if (!usingFuel) {
			return;
		}
		//when the tank is at 20%, start notifying the plan to replan
		//the plan itself then decides whether to go for charging or
		//to continue towards the destination
		if (tankLevel <= 0.2f) {
			plan.ReplanWithRecharge();
		}
		//if the car has 0% charge left, notify the game object which decides
		//what happens next
		if (tankLevel <= 0) {
			Game.Instance.OnCarOutOfCharge(this);
			tankLevel = 0;
			GoToState(CarState.outofcharge);
			return;
		}

		//actually modify the amount
		tankLevel += amount;
	}

	//Sends car onto driving towards a waypoint
	public void GoToWaypoint(Waypoint w) {
		currentWaypoint = w;
		GoToState(CarState.driving);
	}

	//use the delta time to make cars go faster (simply iterative)
	void Drive(int deltaTime) {
		for (int i = 0; i < deltaTime; ++i) {
			DriveToCurrentWaypoint();
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			float start = Time.realtimeSinceStartup;
			#endif
			AvoidCollision();
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			float end = Time.realtimeSinceStartup;
			collisions += end - start;
			#endif
		}
	}

	void DriveToCurrentWaypoint() {

		//Waypoint reached
		if (WaypointReached(currentWaypoint)) {
			//tell plan that we have arrived at the waypoint
			NotifyWaypointReached();
			return;
		}

		if (currentWaypoint == null) {
			Debug.LogError ("Car driving to null.");
			return;
		}
		
		//change direction
		Vector3 heading = currentWaypoint.AsVector() - transform.position.ToVector2();
		directionHeading = heading.normalized;

		//Do actual driving
		Accelerate(maxSpeed);
		Steer();
	}

	void NotifyWaypointReached() {
		plan.WaypointReached();
	}

	//When the car is parked, its collider is disabled so that other cars ignore it
	//when checking for collisions
	//This has the adverse effect of having to manully enable the collider when checking
	//for clicks on the car, but.. that's how it is, it works and so why change it
	public void Park(ParkingSpace p) {
		if (collider == null) 
			collider = GetComponent<BoxCollider2D>();
		collider.enabled = false;
		//move right on top of the space
		transform.position = p.transform.position;
		//rotate it towards the exit, this is sort of bad, but its actually hardly noticable
		transform.rotation = Quaternion.Euler(0, 0, p.road.transform.rotation.eulerAngles.z - 90);
		GoToState(CarState.parked);
	}

	public void Unpark() {
		//Enable collider again
		collider.enabled = true;
	}
		
	//This is called by the plan when we have arrived at a charging spot
	public void Recharge(PowerStation s) {
		//Start 'animations'
		StartCoroutine("MoveToWaypoint", s.GetWaypoint());
		StartCoroutine("FaceObject", s.transform);
		GoToState(CarState.charging);
	}

	//Slowly move the car to waypoint w
	public IEnumerator MoveToWaypoint(Waypoint w) {
		while (!WaypointReached(w)) {
			var diff = w.AsVector() - transform.position.ToVector2();
			var add = (diff * 0.01f).ToVector3();

			transform.position += Vector3.Lerp(Vector3.zero, add, Time.deltaTime);
			yield return new WaitForFixedUpdate();
		}
		yield break;
	}

	//Slowly rotate the car away from the object, dodgy (use with caution)
	public IEnumerator FaceObject(Transform position) {
		Vector2 facedir = (-(position.position - transform.position)).ToVector2();
		while (!facedir.AlmostEqual(head)) {
			float angle = Vector2.Angle(head, facedir);
			Vector3 cross = Vector3.Cross(head, facedir);
			int mul = -1;
				
			//left or right
			if (cross.z > 0) {
				mul *= -1;
			}
			//exponential decrease - 10f*Mathf.Abs(1-Mathf.Sqrt(angle))
			angle = angle / 180;
			float rotationMagnitude = Mathf.Exp(angle);
				
			transform.Rotate(new Vector3(0f,0f,mul*rotationMagnitude));
			yield return new WaitForSeconds(0.01f);
		}
		yield break;
	}

	public void LeavePowerStation() {
		//Does nothing now, because it mostly works without any special code.
	}

	//Stop the car at once!
	bool going = true;
	void ImmediateBrake() {
		going = false;
	}
	
	void Accelerate(float val) {
		head.Normalize();
		//use val instead of currentSpeed, right now its really bad design
		if (going)
			transform.position += new Vector3(head.x, head.y, 0) * (currentSpeed / 20f);
	}

	void Steer() {
		//Already driving in the right direction
		if (head.AlmostEqual(directionHeading)) {
			return;
		}
		
		float angle = Vector2.Angle(head, directionHeading);
		float or = angle;
		Vector3 cross = Vector3.Cross(head, directionHeading);
		int mul = -1;
		
		if (angle > 15){
			//The angle is pretty big, so slow down / stop to steer
			ImmediateBrake();
		} else {
			//Set going to true again (set to false by ImmediateBrake)
			going = true;
		}
		
		//left or right
		if (cross.z > 0) {
			mul *= -1;
		}
		//exponential decrease - 10f*Mathf.Abs(1-Mathf.Sqrt(angle))
		angle = angle / 180;
		float rotationMagnitude = Mathf.Exp(angle) * currentSpeed * 10;

		rotationMagnitude = Mathf.Clamp(rotationMagnitude, -Mathf.Abs(or), Mathf.Abs(or));

		transform.Rotate(new Vector3(0f,0f,mul*rotationMagnitude));
	}

	public void SetCollisionAvoidance(CollisionAvoidance technique) {
		avoidance = technique;
	}

	bool AvoidCollision() {

		if (avoidance == CollisionAvoidance.disabled) {
			return false;
		}

		/* Boxcast solution */
		/* Two boxes are cast, one to the front and another one to the right of the vehicles
			The right one should help with U turns and starting off, the front one is just general
			collision detection.
		 */
		/*
		 * This has been changed to a boxcast + a linecast for performance reasons
		 */

		float dynamicsize = 0.25f;
		float dynamicsize2 = 0.2f;
		float forward = 0.5f;
		bool disableright = false;

		if (avoidance == CollisionAvoidance.dynamicc) {
			dynamicsize /= waitingFor +1f;
			dynamicsize2 /= waitingFor +1f;
			if (waitingFor > 1f) {
				disableright = true;
			}
			forward /= (waitingFor+1f);
		}

		float angle = transform.rotation.eulerAngles.z;
		Vector2 size = new Vector2(dynamicsize, dynamicsize);

		RaycastHit2D hit = Physics2D.BoxCast(transform.position.ToVector2(), size, angle, head, forward);

		//raycast 2 = to the right of the vehicle
		Vector2 size2 = new Vector2(dynamicsize2, dynamicsize2);

		//old but reliable method of calculating the right side direction (the uncommented one should be correct too)
		//Vector2 sideVector = Quaternion.AngleAxis(-90, Vector3.forward) * head;

		Vector2 sideVector = transform.TransformDirection(Vector3.right).ToVector2();

		RaycastHit2D hit2 = new RaycastHit2D();
		if (!disableright) {
			hit2 = Physics2D.Linecast(transform.position.ToVector2() + (sideVector * 0.1f), transform.position.ToVector2() + sideVector);
		}

		//Check the right side cast
		if (hit2 != null) {
			if (hit2.collider != null && hit2.collider.GetComponent<Car>() != null) {
				Car other = hit2.collider.GetComponent<Car>();
				if (other == this) {
					Debug.Log("Right cast hit self, did the casting API change?");
				}
				float dot = Vector2.Dot(head, other.head);
				if (dot >= 0.75) {
					GoToState (CarState.injam);
					return true;
				}
			}
		}

		//Check the front side cast
		if (hit.collider == null) {
			return false;
		}
		Car c = hit.collider.GetComponent<Car>();
		if (c == null) {
			return false;
		}
		if (c == this) {
			Debug.Log ("Self hit, did the casting API change?");
		}
		if (state == CarState.waiting && waitingFor > Random.Range(2f, 3f)) {
			//this condition could be just removed
		}
		if (WillCollide(c)) {
			GoToState (CarState.injam);
			return true;
		}
		return false;
	}

	bool WillCollide(Car other) {
		//if the path will be straight up until the point, then yes.
		float dot = Vector2.Dot(head, other.head);
		if (dot > 0.75) {
			//are facing different directions
			return true;
		}
		List<Waypoint> points = plan.RequestPlanList();
		for (int i = 0; i < (points.Count > 2 ? 2:points.Count); ++i) {
			Bounds b = new Bounds();
			//Tries to position the cars on the next waypoint, imperfect, but works most of the time
			b = GetComponent<Collider2D>().bounds;
			b.center = points[i].AsVector().ToVector3();
			if (other.GetComponent<Collider2D>().bounds.Intersects(b)) {
				return true;
			}
		}

		return false;
	}

	//Checks whether the car is at the waypoint
	bool WaypointReached(Waypoint w) {
		if (w == null) {
			return false;
		}
		Vector2 dist = w.AsVector() - new Vector2(transform.position.x, transform.position.y);
		if (dist.magnitude < 0.1f) {
			if (w.onRoad == null) {
				Debug.Log (w);
			}
			road = w.onRoad;
			return true;
		}
		return false;
	}
	
	void UpdateVariables() {
		
		//update head vector
		float zAngle = gameObject.transform.rotation.eulerAngles.z;
		head.x = -Mathf.Sin(Mathf.Deg2Rad*zAngle);
		head.y = Mathf.Cos(Mathf.Deg2Rad*zAngle);
		
		//update current speed with direction - negative speed is going backwards
		//is speed vector in the same halfplane as the the head?
		Vector2 velocityVector = gameObject.GetComponent<Rigidbody2D>().velocity;
		zAngle = Vector2.Angle(head, velocityVector);
		if (Vector3.Cross(head, velocityVector).z > 0)
			zAngle = 360 - zAngle;
		float mul = zAngle < 90 || zAngle > 270 ? 1f : -1f;
		currentSpeed = Game.Instance.world.timescale;
	}

	void OnDestroy() {
		if (journeyLines == null) {
			return;
		}
		//Remove the lines so they dont clog up the system
		for (int i = journeyLines.Count - 1; i >= 0; --i) {
			DestroyImmediate(journeyLines[i]);
		}
	}

	#region journey lines
	void DisableJourneyLines() {
		if (journeyLines != null) {
			foreach (LineRenderer line in journeyLines) {
				line.enabled = false;
			}
		}
	}
	
	/// Draws the debug lines.
	void DrawJourneyLines() {
		
		//draw journey
		Color[] colors = new Color[] {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, 
			Color.grey, Color.magenta, Color.red, Color.white, Color.yellow};
		if (journeyLines == null || plan.GetPlan().Count+1 > journeyLines.Count) {
			if (journeyLines == null)
				journeyLines = new List<LineRenderer>();
			for (int i = journeyLines.Count; i < plan.GetPlan().Count+1; ++i) {
				journeyLines.Add(((GameObject)Instantiate(lineprefab)).GetComponent<LineRenderer>());
				journeyLines[i].SetColors(Color.green, Color.magenta);
				journeyLines[i].SetVertexCount(2);
				journeyLines[i].SetWidth(0.04f,0.04f);
			}
		}
		Waypoint prev = new Waypoint(transform.position.ToVector2());
		
		int index = 0;
		foreach (Waypoint r in plan.GetPlan()) {
			LineRenderer line = journeyLines[index++];
			line.SetPosition(0, new Vector3(prev.x, prev.y, -0.1f));
			line.SetPosition(1, new Vector3(r.x, r.y, -0.1f));
			line.enabled = true;
			prev = r;
		}
		//disable leftovers
		for (int i = index; i < journeyLines.Count; ++i) {
			if (journeyLines[i].enabled == false)
				break;
			journeyLines[i].enabled = false;
		}
	}
	#endregion

	#region info

	public void ShowInfo() {
		if (infoShown) {
			HideInfo();
			return;
		}
		InitBubble();
		infoBubble.render = true;
		infoShown = true;
		drawJourney = true;
	}

	private void UpdateInfoString() {
		string departure = plan.ActiveTrip == null ? LocaleManager.locale.Never : plan.ActiveTrip.departure.ToString();
		infoString = LocaleManager.locale.CarNumber + ":\t\t" + serialNumber + "\n" +
					LocaleManager.locale.NearestDeparture + ":\t" + departure + "\n" +
					string.Format(LocaleManager.locale.BatteryRemaining + ":\t{0:0.00}%", tankLevel*100);
	}

	static Material bubbleMat = null;
	static GUISkin bubbleSkin = null;

	private void InitBubble() {
		if (!infoBubble) {
			infoBubble = this.gameObject.AddComponent<SpeechBubble>();
			if (bubbleMat == null) {
				bubbleMat = (Material)Resources.Load("White Unlit", typeof(Material));
				bubbleSkin = (GUISkin)Resources.Load("BubbleSkin", typeof(GUISkin));
			}
			infoBubble.mat = bubbleMat;
			infoBubble.guiSkin = bubbleSkin;
			infoBubble.owner = this;
		}
	}

	public void OnBubbleClosed() {
		HideInfo();
	}

	public void HideInfo() {
		if (!infoShown) {
			return;
		}
		infoShown = false;
		DrawJourney = false;
		infoBubble.render = false;
	}

	#endregion

	public override string ToString () {
		return string.Format("Car {0}",serialNumber);
	}

	#endregion

	//Creates waypoints for roads in the path. Probles: path too short,
	//undecideablity for starting point (the weird little path when the car chooses a new path)
	public static List<Waypoint> CreateWaypoints(List<Road> path, Car car) {
		List<Waypoint> points = new List<Waypoint>();
		
		if (path.Count < 3) {
			Vector2 last = path[0].neighbourRoads[0].transform.position.ToVector2();
			foreach (Road r in path) {
				Vector2 dir = r.AsVector() - last;
				if (dir.AlmostEqual(Vector2.zero)) {
					last = r.AsVector();
					continue;
				}
				points.AddRange(r.CreateWaypoints(r.AsVector()-last, r.AsVector()-last));
				last = r.AsVector();
			}
			return points;
		}
		
		Road comingfrom = path[0];
		Road current = path[1];
		Road goingto = path[2];
		
		bool skipfirst = false;
		if (comingfrom is ParkingSpotRoad)
			skipfirst = true;
		
		if (!skipfirst) {
			Vector2 first = (comingfrom.AsVector() - car.transform.position.ToVector2()).normalized;
			Vector2 best = Vector2.up;
			float dist = first.Distance(Vector2.up);
			if (first.Distance(-Vector2.up) < dist) {
				dist = first.Distance(-Vector2.up);
				best = -Vector2.up;
			}
			if (first.Distance(-Vector2.right) < dist) {
				dist = first.Distance(-Vector2.right);
				best = -Vector2.right;
			}
			if (first.Distance(Vector2.right) < dist) {
				dist = first.Distance(Vector2.right);
				best = Vector2.right;
			}
			Vector2 other = current.AsVector() - comingfrom.AsVector();
			if (!other.AlmostEqual(-best)) {
				points.AddRange(CreateWaypoints(best, other, comingfrom));
			}
		}
		
		for (int i = 1; i < path.Count; ++i) {
			//Debug.Log("Looking for waypoint from " + comingfrom + " to " + current + " going to " + goingto);
			Vector2 dircomingfrom = current.AsVector() - comingfrom.AsVector();
			Vector2 dirgoingto = goingto.AsVector() - current.AsVector();
			
			List<Waypoint> w = CreateWaypoints(dircomingfrom, dirgoingto, current);
			
			//move a step
			points.AddRange(w);
			
			comingfrom = current;
			current = goingto;
			try {
				goingto = path[i+2];
			} catch (System.ArgumentOutOfRangeException) {
				for (int j = 0; j < current.neighbourRoads.Count; ++j) {
					goingto = current.neighbourRoads[j];
					if (goingto != comingfrom && goingto != current) {
						break;
					}
				}
			}
		}
		return points;
	}
	
	private static List<Waypoint> CreateWaypoints(Vector2 comingfrom, Vector2 goingto, Road road) {
		List<Waypoint> r = road.CreateWaypoints(comingfrom, goingto);
		return r;
	}

}
