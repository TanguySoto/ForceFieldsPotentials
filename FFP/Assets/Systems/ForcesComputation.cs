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

	private Family pPlanFamily 		= FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family shipFamily 		= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family sourcesFamily 	= FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));

	private bool isShipMovable = true;

	// ==== LIFECYCLE ====
	public ForcesComputation(){
		SystemsManager.AddFSystem (this);
	}

	protected override void onPause(int currentFrame) {
	}
		
	protected override void onResume(int currentFrame){
	}
		
	protected override void onProcess(int familiesUpdateCount) {
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		if (gl == null) {return;}

		if (isShipMovable && gl.state == GameLogic.STATES.PLAYING) {
			applyForceToShip ();
		} else {
			keepShipInPlace ();
		}
	}

	// ==== METHODS ====
	protected void keepShipInPlace(){
		// Get Terrain heightMap and place it
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// Get the ship and its position, speed, acc and mass
		GameObject ship = shipFamily.First ();
		Position shipPosition = ship.GetComponent<Position> ();
		Transform tr = ship.GetComponent<Transform>();
		tr.position = new Vector3 (shipPosition.pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, shipPosition.pos.y * terrDims.z);
	}

	protected void applyForceToShip(){
		// Get Terrain heightMap and place it
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// Get the ship and its position, speed, acc and mass
		GameObject ship = shipFamily.First ();
		Position shipPosition = ship.GetComponent<Position> ();
		Movement m = ship.GetComponent<Movement> ();
		Mass mass = ship.GetComponent<Mass> ();

		// Compute forces to be applied
		Vector3 forces = computeForceAt(shipPosition.pos.x,shipPosition.pos.y);

		// Apply force to the ship using Euler and rotate it in the direciton of the speed
		m.acceleration = (forces/mass.mass);
		m.speed += m.acceleration * Time.deltaTime;
		shipPosition.pos += m.speed * Time.deltaTime; 

		Transform tr = ship.GetComponent<Transform>();
		tr.position = new Vector3 (shipPosition.pos.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, shipPosition.pos.y * terrDims.z);
		tr.rotation = Quaternion.Euler(90, (360-Mathf.Atan2(m.speed.y,m.speed.x)*Mathf.Rad2Deg+90)%360,0);

		UI ui = (UI)SystemsManager.GetFSystem("UI");
		ui.UpdateShipInformations (ship);

		// moving projection to correct height
		Transform projection = ship.transform.GetChild(1);
		LineRenderer line = projection.gameObject.GetComponent<LineRenderer> ();

		RaycastHit hit = new RaycastHit();
		// touched terrain
		if (Physics.Raycast (ship.transform.position, Vector3.down, out hit)) {
			projection.position = hit.point;
		}
		line.SetPosition (1, ship.transform.position);
		line.SetPosition (0, projection.position);

		/* Apply force using unity and thus allowing smooth collisons
		Rigidbody r = ship.GetComponent<Rigidbody> ();
		r.AddForce (new Vector3(forces.x,forces.z,forces.y));
		shipPosition.pos = new Vector3 (r.transform.position.x/terrDims.x, r.transform.position.z/terrDims.z, r.transform.position.y);
		m.speed =  new Vector3 (Mathf.Round (1000f * r.velocity.x) / 1000f, Mathf.Round (1000f * r.velocity.z) / 1000f, Mathf.Round (1000f * r.velocity.y) / 1000f);
		m.acceleration = forces;
		*/
	}

	public Vector3 computeForceAt(float x, float y){
		// Get Terrain heightMap and place it
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		int hmWidth = terr.terrainData.heightmapWidth;
		int hmHeight = terr.terrainData.heightmapHeight;

		Vector3 forces = Vector3.zero;
		foreach(GameObject s in sourcesFamily){
			// Get associated field and position for each source
			Field f = s.GetComponent<Field> ();
			Position position = s.GetComponent<Position> ();
			Vector3 p = position.pos;
			// Compute force
			if (f.isUniform) {
				forces.x += Mathf.Round (Constants.FORCES_ROUNDING *planDerivativeX(p.x * hmWidth, p.y * hmHeight, f.sigx * hmWidth, f.sigy * hmHeight, f.b, f.c, x * hmWidth, y*hmHeight)) / Constants.FORCES_ROUNDING;
				forces.y += Mathf.Round (Constants.FORCES_ROUNDING * planDerivativeY (p.x * hmWidth, p.y * hmHeight, f.sigx * hmWidth, f.sigy * hmHeight, f.b, f.c,x * hmWidth, y*hmHeight)) / Constants.FORCES_ROUNDING;
			} else {
				forces.x += Mathf.Round (Constants.FORCES_ROUNDING * gaussianDerivativeX (p.x * hmWidth, p.y * hmHeight, f.sigx / 2f * hmWidth, f.sigy / 2f * hmHeight, f.A / 2f, x * hmWidth, y * hmHeight)) / Constants.FORCES_ROUNDING;
				forces.y += Mathf.Round (Constants.FORCES_ROUNDING * gaussianDerivativeY (p.x * hmWidth, p.y * hmHeight, f.sigx / 2f * hmWidth, f.sigy / 2f * hmHeight, f.A / 2f, x * hmWidth, y * hmHeight)) / Constants.FORCES_ROUNDING;
			}
		}

		return forces;
	}

	public static float gaussianDerivativeX(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return Constants.FORCES_SCALING * A * ((x-x0)/(sigx*sigx)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}

	public static float gaussianDerivativeY(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return Constants.FORCES_SCALING *A * ((y-y0)/(sigy*sigy)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}

	protected float planDerivativeX(float x0, float y0, float sizeX, float sizeY, float b, float c, float x, float y){
		float planX = x0 - x;
		float planY = y0 - y;

		if (Mathf.Abs(planX) < sizeX && Mathf.Abs(planY) < sizeY) {
			return b * Constants.FORCES_SCALING*2;
		}

		return 0;
	}

	protected float planDerivativeY(float x0, float y0, float sizeX, float sizeY, float b, float c, float x, float y){
		float planX = x0 - x;
		float planY = y0 - y;

		if (Mathf.Abs(planX) <= sizeX && Mathf.Abs(planY) <= sizeY) {
			return c * Constants.FORCES_SCALING *2;
		}

		return 0;
	}
}