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

public class ForcesComputation : FSystem {

	private Family pPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));
	private Family sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));

	private bool isShipMovable = true;

	// ==== LIFECYCLE ====
	
	protected override void onPause(int currentFrame) {
	}
		
	protected override void onResume(int currentFrame){
		
	}
		
	protected override void onProcess(int familiesUpdateCount) {
		if (isShipMovable) {
			applyForceToShip ();
		}
	}

	// ==== METHODS ====

	protected void applyForceToShip(){
		// Get Terrain heightMap and place it
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		int hmWidth = terr.terrainData.heightmapWidth;
		int hmHeight = terr.terrainData.heightmapHeight;
		Vector3 terrDims = terr.terrainData.size;

		// Get the ship and its position, speed, acc and mass
		GameObject ship = shipFamily.First ();
		Position pos = ship.GetComponent<Position> ();
		Movement m = ship.GetComponent<Movement> ();
		Mass mass = ship.GetComponent<Mass> ();
		Transform tr = ship.GetComponent<Transform> ();

		// Compute forces to be applied
		Vector3 forces = Vector3.zero;
		foreach(GameObject s in sourcesFamily){
			// Get associated field and position for each source
			Field f = s.GetComponent<Field> ();
			Position p = s.GetComponent<Position> ();
			forces.x += gaussianDerivativeX (p.x*hmWidth, p.y*hmHeight, f.sigx*hmWidth, f.sigy*hmHeight, f.A/2f, pos.x * hmWidth, pos.y * hmHeight);
			forces.y += gaussianDerivativeY (p.x*hmWidth, p.y*hmHeight, f.sigx*hmWidth, f.sigy*hmHeight, f.A/2f, pos.x * hmWidth, pos.y * hmHeight);
		}

		// Apply force to the ship
		m.acceleration += (forces/mass.mass);
		m.speed += m.acceleration;
		pos.x += m.speed.x;
		pos.y += m.speed.y;
		pos.z += m.speed.z;

		Debug.Log (forces.x);

		tr.position = new Vector3 (pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * terrDims.z);
	}

	protected float gaussianDerivativeX(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return A * ((x-x0)/(sigx*sigx)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}

	protected float gaussianDerivativeY(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return A * ((y-y0)/(sigy*sigy)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}
}