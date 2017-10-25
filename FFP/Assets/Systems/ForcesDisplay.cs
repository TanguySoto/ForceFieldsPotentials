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

public class ForcesDisplay : FSystem {

	// ==== VARIABLES ====

	private Family pPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));

	private bool isShowSources 	= true;
	private bool isShowFields 	= true;

	// ==== LIFECYCLE ====
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		refresh ();
	}
		
	protected override void onProcess(int familiesUpdateCount) {
	}
		
	// ==== METHODS ====

	protected void refresh(){
		if (isShowFields) {
			showFields ();
		}
		if (isShowSources) {
			showSources ();
		}
	}

	protected void showFields(){
		// Get Terrain
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();

		// Get Terrain heightMap and place it
		int hmWidth = terr.terrainData.heightmapWidth;
		int hmHeight = terr.terrainData.heightmapHeight;
		float [,] level = terr.terrainData.GetHeights(0,0,hmWidth,hmHeight);
		for (int x = 0; x < hmWidth; x++) {
			for (int y = 0; y < hmHeight; y++) {
				level [y, x] = Constants.BASE_PPLAN_HEIGHT;
			}
		}

		// For each sources
		foreach(GameObject s in sourcesFamily){
			// Get associated field and position
			Field f = s.GetComponent<Field>();
			Position position = s.GetComponent<Position> ();
			Vector3 p = position.pos;

			// Apply field to Terrain heights
			for (int x = 0; x < hmWidth; x++) {
				for (int y = 0; y < hmHeight; y++) {
					level [y,x] += gaussian(p.x*hmWidth,p.y*hmHeight,f.sigx*hmWidth,f.sigy*hmHeight,f.A/2f,x,y);
				}
			}
		}

		// Set new Terrain heights
		terr.terrainData.SetHeights (0, 0, level);		
	}

	protected void showSources(){
		// Get Terrain dims to scale object
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();
		Vector3 terrDims = terr.terrainData.size;

		// For each sources
		foreach(GameObject s in sourcesFamily){
			// Get associated dims and position
			Dimensions dims = s.GetComponent<Dimensions>();
			Field field = s.GetComponent<Field> ();
			Position position = s.GetComponent<Position> ();
			Vector3 p = position.pos;
			Transform tr = s.GetComponent<Transform> ();

			// Move it to right position
			Vector3 newPos = new Vector3 (p.x * terrDims.x, Constants.BASE_SOURCE_HEIGHT * terrDims.y, p.y * terrDims.z);
			tr.position = newPos;

			// Scale it to the right dimension
			float scale = Constants.SOURCES_SIZE_SCALING;
			dims.width = field.sigx * scale;
			dims.length = field.sigy * scale;
			dims.height = Mathf.Min (dims.width, dims.length);
			tr.localScale = new Vector3(dims.width, dims.height, dims.length);
		}
	}

	protected float gaussian(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return A * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}
}