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
	
	private Family pPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));
	private Family finishFamily = FamilyManager.getFamily(new AnyOfTags("finish"));

	private bool isShipInit = false;
	private bool isFinishInit = false;

	// State of game
	public enum STATES {SETUP, PLAYING, PAUSED, WON, LOST};
	public static STATES state = STATES.PLAYING;
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isShipInit) {
			InitShip ();
			isShipInit = true;
		}
		if (!isFinishInit) {
			InitFinish ();
			isFinishInit = true;
		}
	}
		
	protected override void onProcess(int familiesUpdateCount) {
	}

	public void InitFinish(){
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
		Movement m = s.GetComponent<Movement> ();
		Position position = s.GetComponent<Position> ();
		Vector3 p = position.pos;
		Transform tr = s.GetComponent<Transform> ();

		// Move it to right position
		Vector3 newPos = new Vector3 (p.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, p.y * terrDims.z);
		tr.position = newPos;
		// Put right initial speed
		Rigidbody rb = s.GetComponent<Rigidbody>();
		rb.velocity = m.speed;
	}
}