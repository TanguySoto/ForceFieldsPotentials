using UnityEngine;
using FYFY;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class GameLogic : FSystem {

	// ==== VARIABLES ====
	
	private static Family pPlanFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private static Family shipFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));
	private static Family finishFamily = FamilyManager.getFamily(new AnyOfTags("finish"));

	private static bool isShipInit 		= false;
	private static bool isFinishInit 	= false;

	// State of game
	public enum STATES {SETUP, PLAYING, PAUSED, WON, LOST};
	public static STATES state = STATES.SETUP;

	// ==== LIFECYCLE ====
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isFinishInit) {
			InitFinish ();
			isFinishInit = true;
		}
		if (!isShipInit) {
			InitShip ();
			isShipInit = true;
		}
	}
		
	protected override void onProcess(int familiesUpdateCount) {
	}

	// ==== METHODS ====

	protected void InitFinish(){
		// Get Terrain dims to scale object
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// Scale and place every finish
		foreach(GameObject f in finishFamily){
			Position position = f.GetComponent<Position> ();
			Dimensions dim = f.GetComponent<Dimensions> ();
			Transform tr = f.transform;

			// Move it to right position
			Vector3 newPos = new Vector3 (position.pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, position.pos.y * terrDims.z);
			tr.position = newPos;
			// Scale it
			f.transform.localScale = new Vector3(dim.width,dim.height,dim.length);
		}
	}

	protected static void InitShip(){
		// Get Terrain dims to scale object
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// Get associated dims and position
		GameObject s = shipFamily.First();
		Position position = s.GetComponent<Position> ();
		Vector3 p = position.pos;
		Transform tr = s.GetComponent<Transform> ();

		// Move it to right position
		Vector3 newPos = new Vector3 (p.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, p.y * terrDims.z);
		tr.position = newPos;
	}

	public static void OnPlay(){
		GameObject ship = shipFamily.First ();
		Component.Destroy (ship.GetComponent<Editable> ());
		if (PlayerActions.previousGameObject == ship) {
			ship.GetComponent<Renderer> ().material = PlayerActions.selectedMaterial;
		}
		UI.UpdateShipInformations (ship);
		state = STATES.PLAYING;
	}

	public static void OnPause(){
		state = STATES.PAUSED;
	}

	public static void OnLost(){
		state = STATES.LOST;
	}

	public static void OnWon(){
		state = STATES.WON;
		Debug.Log ("GG WP");
	}
}