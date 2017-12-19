using UnityEngine;
using FYFY;
using FYFY_plugins.CollisionManager;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class TrajectoryViewer : FSystem {

	// ==== VARIABLES ====

	private Family pPlanFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass))); 

	// ==== LIFECYCLE ====
	public TrajectoryViewer(){
		SystemsManager.AddFSystem (this);
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		if (gl == null) {return;}

		if (gl.state == GameLogic.STATES.SETUP) {
			showTrajectory ();
		} else {
			hideTrajectory ();
		}
	}

	// ==== METHODS ====

	protected void showTrajectory(){
		// Parameters
		ForcesComputation fc = (ForcesComputation)SystemsManager.GetFSystem("ForcesComputation");

		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		Vector3 pos = shipFamily.First ().GetComponent<Position> ().initialPos;
		Vector3 spe = shipFamily.First ().GetComponent<Movement> ().speed;
		Vector3 acc = Vector3.zero;
		float mass = shipFamily.First ().GetComponent<Mass> ().mass;

		int nbPoint = 100;
		int i=1;
		float dist = 0;
		float deltaTime = 1	 /(nbPoint * spe.magnitude);
		Vector3[] positions = new Vector3 [nbPoint];
		positions [0] = new Vector3 (pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * terrDims.z);

		// Calculation
		LineRenderer line = shipFamily.First ().transform.GetChild (2).GetComponent<LineRenderer> ();
		if (!line.enabled) {
			line.enabled = true;
		}

		while(dist<terrDims.x && i<nbPoint){
			// Compute forces to be applied
			Vector3 forces = fc.computeForceAt(pos.x,pos.y);

			// Apply force to the point using Euler
			acc = (forces/mass);
			spe += acc * deltaTime;
			pos += spe * deltaTime; 
			positions [i] = new Vector3 (pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * terrDims.z);
			dist += Vector3.Distance (positions [i], positions [i - 1]);

			i++;
		}
		// fill array if not full
		for (int j = i; j < nbPoint; j++) {
			positions [j] = positions [i-1];
		}
			
		line.SetPositions (positions);
	}

	protected void hideTrajectory(){
		LineRenderer line = shipFamily.First ().transform.GetChild (2).GetComponent<LineRenderer> ();
		if (line.enabled) {
			line.enabled = false;
		}
	}
}