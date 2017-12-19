﻿using UnityEngine;
using FYFY;
using System.Collections;
using System.Linq; // used for Sum of array

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

	private Family pPlanFamily	 = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));

	private bool isShowSources 			= true;
	private bool isShowFields 			= true;

	// ==== LIFECYCLE ====

	public ForcesDisplay(){
		SystemsManager.AddFSystem (this);
		init ();
	}
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		refresh ();
	}
		
	protected override void onProcess(int familiesUpdateCount) {
		rotateSources ();
	}
		
	// ==== METHODS ====

	protected void init(){
		sourcesFamily.addEntryCallback (OnSourceEntered);
		sourcesFamily.addExitCallback (OnSourceExited);
	}

	public void refresh(){
		if (isShowFields) {
			showFields ();
			showFieldsColors ();
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
		float max = level [0, 0];
		float min = level [0, 0];
		foreach(GameObject s in sourcesFamily){
			// Get associated field and position
			Field f = s.GetComponent<Field>();
			Position position = s.GetComponent<Position> ();
			Vector3 p = position.pos;

			// Apply field to Terrain heights
			for (int x = 0; x < hmWidth; x++) {
				for (int y = 0; y < hmHeight; y++) {
					if (f.isUniform) {
						level [y, x] += plan (p.x * hmWidth, p.y * hmHeight, f.sigx * hmWidth, f.sigy * hmHeight, f.b, f.c, x, y);
					} else {
						level [y, x] += gaussian (p.x * hmWidth, p.y * hmHeight, f.sigx/2f * hmWidth, f.sigy/2f * hmHeight, f.A / 2f, x, y);
					}
					max = Mathf.Max(max,level[y,x]);
					min = Mathf.Min(min,level[y,x]);	
				}
			}
		}

		// Normalize and keep plan at 0.5f
		float maxDiff = Mathf.Max(Mathf.Abs(max-0.5f), Mathf.Abs(min-0.5f));
		for (int x = 0; x < hmWidth; x++) {
			for (int y = 0; y < hmHeight; y++) {
				if (maxDiff > 0.5f) {
					if (level [y, x] > 0.5f) {
						level [y, x] = 0.5f * (level [y , x] - 0.5f) / (0.5f+maxDiff - 0.5f) + 0.5f; //(b-a)*(myValue-min)/(max-min) + a
					}
					else if (level [y, x] < 0.5f) {
						level [y, x] = 0.5f - 0.5f * (Mathf.Abs(level[y, x] - 0.5f)) / maxDiff;
					}
				}
			}
		}

		// Set new Terrain heights
		terr.terrainData.SetHeights (0, 0, level);
	}

	/**
	 * From https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/ and adapted to ECS
	 * TODO make faster
	 */
	protected void showFieldsColors(){
		// Get the terrain
		Terrain terr = pPlanFamily.First ().GetComponent<Terrain>();

		// Get a reference to the terrain data
		TerrainData terrainData = terr.terrainData;

		// Compute max and min height
		float maxHeight = terrainData.GetHeight (0, 0);
		float minHeight = terrainData.GetHeight (0, 0);
		for (int y = 0; y < terrainData.alphamapHeight; y++) {
			for (int x = 0; x < terrainData.alphamapWidth; x++) {
				float height = terrainData.GetHeight (y, x);
				maxHeight = Mathf.Max (height, maxHeight);
				minHeight = Mathf.Min (height, minHeight);
			}
		}
			
		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
		float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];	

		for (int y = 0; y < terrainData.alphamapHeight; y++){
			for (int x = 0; x < terrainData.alphamapWidth; x++){
				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
				float height = terrainData.GetHeight(y,x);

				// Setup an array to record the mix of texture weights at this point
				float[] splatWeights = new float[terrainData.alphamapLayers];

				// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
				splatWeights [0] = 0.5f;
				if (maxHeight - minHeight != 0) {
					splatWeights [1] = Mathf.Pow (1 - (height - minHeight) / (maxHeight - minHeight), 2);
					splatWeights [2] = Mathf.Pow ((height - minHeight) / (maxHeight - minHeight), 2);
				} else {
					splatWeights [1] = 0.25f;
					splatWeights [2] = 0.25f;
				}

				// Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
				float z = splatWeights.Sum();

				// Loop through each terrain texture
				for(int i = 0; i<terrainData.alphamapLayers; i++){

					// Normalize so that sum of all texture weights = 1
					splatWeights[i] /= z;

					// Assign this point to the splatmap array
					splatmapData[x, y, i] = splatWeights[i];
				}
			}
		}

		// Finally assign the new splatmap to the terrainData:
		terrainData.SetAlphamaps(0, 0, splatmapData);
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
			if (field.isUniform) {
				dims.height = 0.33f;
				dims.width = field.sigx*20;
				dims.length = field.sigy*20;
			} else {
				dims.width = field.sigx * scale;
				dims.length = field.sigy * scale;
				dims.height = field.sigy * scale;
				tr.Rotate (0, Time.deltaTime, 0);
			}

			tr.localScale = new Vector3(dims.width, dims.height, dims.length);
		}
	}

	protected void rotateSources(){
		foreach (GameObject s in sourcesFamily) {
			Transform tr = s.GetComponent<Transform> ();
			if (!s.GetComponent<Field> ().isUniform) {
				tr.Rotate (0, 15*Time.deltaTime, 0);
			}
		}
	}

	protected void OnSourceEntered(GameObject s){
		refresh ();
	}

	protected void OnSourceExited(int id){
		refresh ();
	}

	protected float gaussian(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return A * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}

	protected float plan(float x0, float y0, float sizeX, float sizeY, float b, float c, float x, float y){
		float planX = x0 - x;
		float planY = y0 - y;
		float offset = Mathf.Abs (b * sizeX) + Mathf.Abs (c * sizeY);

		if (Mathf.Abs(planX) <= sizeX && Mathf.Abs(planY) <= sizeY) {
			return b * planX + c * planY + offset;
		}

		return 0;
	}
}