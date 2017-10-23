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

	// ==== VARIABLES ====

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
		Position shipPosition = ship.GetComponent<Position> ();
		Movement m = ship.GetComponent<Movement> ();
		Mass mass = ship.GetComponent<Mass> ();

		// Compute forces to be applied
		Vector3 forces = Vector3.zero;
		foreach(GameObject s in sourcesFamily){
			// Get associated field and position for each source
			Field f = s.GetComponent<Field> ();
			Position position = s.GetComponent<Position> ();
			Vector3 p = position.pos;
			forces.x += Mathf.Round(1000f * gaussianDerivativeX (p.x*hmWidth, p.y*hmHeight, f.sigx*hmWidth, f.sigy*hmHeight, f.A/2f, shipPosition.pos.x * hmWidth, shipPosition.pos.y * hmHeight))/1000f;
			forces.y += Mathf.Round(1000f * gaussianDerivativeY (p.x*hmWidth, p.y*hmHeight, f.sigx*hmWidth, f.sigy*hmHeight, f.A/2f, shipPosition.pos.x * hmWidth, shipPosition.pos.y * hmHeight))/1000f;
		}



		// Apply force to the ship
		m.acceleration = (forces/mass.mass);
		m.speed += m.acceleration * Time.deltaTime;
		shipPosition.pos += m.speed * Time.deltaTime; 

		Transform tr = ship.GetComponent<Transform>();
		tr.position = new Vector3 (shipPosition.pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, shipPosition.pos.y * terrDims.z);


		/* Apply force using unity and thus allowing collisons
		Rigidbody r = ship.GetComponent<Rigidbody> ();
		r.AddForce (new Vector3(forces.x,forces.z,forces.y));
		shipPosition.pos = new Vector3 (r.transform.position.x/terrDims.x, r.transform.position.z/terrDims.z, r.transform.position.y);
		m.speed =  new Vector3 (Mathf.Round (1000f * r.velocity.x) / 1000f, Mathf.Round (1000f * r.velocity.z) / 1000f, Mathf.Round (1000f * r.velocity.y) / 1000f);
		m.acceleration = forces;
		*/
	}

	protected float gaussianDerivativeX(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return Constants.FORCES_SCALING * A * ((x-x0)/(sigx*sigx)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}

	protected float gaussianDerivativeY(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return Constants.FORCES_SCALING *A * ((y-y0)/(sigy*sigy)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}
}