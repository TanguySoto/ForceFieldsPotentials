using UnityEngine;
using UnityEngine.UI;
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
	
	private Family pPlanFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));
	private Family sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));
	private Family finishFamily = FamilyManager.getFamily(new AnyOfTags("finish"));

	private bool isShipInit 	= false;
	private bool isFinishInit 	= false;

	// State of game
	public enum STATES {SETUP, PLAYING, PAUSED, WON, LOST};
	public STATES state = STATES.SETUP;

	// ==== LIFECYCLE ====
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isFinishInit) {
			SystemsManager.AddFSystem (this);
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

	protected void InitShip(){
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

	public void OnPlay(){
		// Systems needed
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");

		// make ship not editable anymore
		GameObject ship = shipFamily.First ();
		Component.Destroy (ship.GetComponent<Editable> ());
		if (pa.previousGameObject == ship) {
			ship.GetComponent<Renderer> ().material = pa.selectedMaterial;
			ui.UpdateShipInformations (ship);
		}

		// make sources not editable anymore
		foreach (GameObject s in sourcesFamily) {
			Component.Destroy (s.GetComponent<Editable> ());
			if (pa.previousGameObject == s) {
				s.GetComponent<Renderer> ().material = pa.selectedMaterial;
				ui.UpdateSourcesInformations (s);
			}
		}

		state = STATES.PLAYING;
	}

	public void OnPause(){
		state = STATES.PAUSED;
	}

	public void OnLost(){
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		ui.launchButton.GetComponentInChildren<Text> ().text = "Retry";
		Debug.Log ("LOST NOOB");
		state = STATES.LOST;
	}

	public void OnWon(){
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		ui.launchButton.GetComponentInChildren<Text> ().text = "Replay";
		Debug.Log ("GG WP");
		state = STATES.WON;
	}
}