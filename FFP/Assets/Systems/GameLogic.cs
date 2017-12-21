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
using System.Collections;

public class GameLogic : FSystem {

	// ==== VARIABLES ====
	
	private Family pPlanFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));
	private Family finishFamily = FamilyManager.getFamily(new AnyOfTags("finish"));

	// State of game
	public enum STATES {SETUP, PLAYING, PAUSED, WON, LOST};
	public STATES state = STATES.SETUP;

	// ==== LIFECYCLE ====

	public GameLogic(){
		SystemsManager.AddFSystem (this);
		InitFinish ();
		InitShip ();
	}
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
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

	public void InitShip(){
		// Get Terrain dims to scale object
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// Get associated dims and position
		GameObject s = shipFamily.First();
		s.GetComponent<Movement> ().speed = s.GetComponent<Movement> ().initialSpeed;
		Position position = s.GetComponent<Position> ();
		Vector3 p = position.initialPos;
		position.pos = p;
		Transform tr = s.GetComponent<Transform> ();

		// Move it to right position and turn it
		Vector3 newPos = new Vector3 (p.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, p.y * terrDims.z);
		tr.position = newPos;
		Movement m = s.GetComponent<Movement> ();
		tr.rotation = Quaternion.Euler(90, (360-Mathf.Atan2(m.speed.y,m.speed.x)*Mathf.Rad2Deg+90)%360,0);

		// moving projection to correct height
		Transform projection = s.transform.GetChild(1);
		LineRenderer line = projection.gameObject.GetComponent<LineRenderer> ();

		RaycastHit hit = new RaycastHit();
		// touched something and was not UI
		if (Physics.Raycast (s.transform.position, Vector3.down, out hit)) {
			projection.position = hit.point;
		}
		line.SetPosition (1, s.transform.position);
		line.SetPosition (0, projection.position);
	}

	public void OnPlay(){
		// Systems needed
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");

		// close Add & Del sources panel
		ui.Hide(ui.sourceAddDelPanel);

		// make ship not editable anymore
		GameObject ship = shipFamily.First ();
		if (ship.GetComponent<Editable> ().editable) {
			ship.GetComponent<Editable> ().editable = false;
			if (pa.previousGameObject == ship) {
				ship.GetComponent<Renderer> ().material = pa.selectedMaterial;
				ui.UpdateShipInformations (ship);
			}
		}
		// save ship attributes in case of retry
		ship.GetComponent<Movement> ().initialSpeed = ship.GetComponent<Movement> ().speed;

		// make sources not editable anymore
		foreach (GameObject s in sourcesFamily) {
			if (s.GetComponent<Editable>()!=null && s.GetComponent<Editable> ().editable) {
				s.GetComponent<Editable> ().editable = false;
				if (pa.previousGameObject == s) {
					s.GetComponent<Renderer> ().material = pa.selectedMaterial;
					if (s.GetComponent<Field> ().isUniform) {
						ui.UpdateUniSourcesInformations (s);
					} else {
						ui.UpdateSourcesInformations (s);
					}
				}
			}
		}

		state = STATES.PLAYING;
	}

	public void OnPause(){
		state = STATES.PAUSED;
	}

	public void OnLost(){
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		ui.launchButton.GetComponentInChildren<Text> ().text = "Try again";
		Debug.Log ("LOST NOOB");
		// == Text after winning or losing
		Text wonOrLostText = GameObject.Find("WonOrLostText").GetComponent<Text>();
		wonOrLostText.text = "YOU LOST";
		wonOrLostText.enabled = true;
		state = STATES.LOST;
	}


	public void OnWon(){
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		ui.launchButton.GetComponentInChildren<Text> ().text = "Next level";
		Debug.Log ("GG WP");

		Text wonOrLostText = GameObject.Find("WonOrLostText").GetComponent<Text>();
		wonOrLostText.text = "YOU WON";

		// Doesn't update anything, update occurs in UI
		GameObject gameInfos = GameObject.Find("GameInformations");
		GameInformations levelInfos = gameInfos.GetComponent<GameInformations> ();
		if (levelInfos.unlockedLevels < levelInfos.totalLevels && levelInfos.noLevel == levelInfos.unlockedLevels) {
			wonOrLostText.text += "\nLevel " + (levelInfos.unlockedLevels+1) + " unlocked!";
		}
		if (levelInfos.unlockedLevels == levelInfos.totalLevels && levelInfos.noLevel == levelInfos.unlockedLevels) {
			wonOrLostText.text += "\nCongrats, you finished the game";
		}
		wonOrLostText.enabled = true;
		state = STATES.WON;
	}


}