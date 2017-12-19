using UnityEngine;
using UnityEngine.SceneManagement;
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

public class UI : FSystem {

	// ==== VARIABLES ====

	private Family shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family editableSourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position), typeof(Editable)));
	private Family PPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));

	// === Main panel
	CanvasGroup mainCanvasGroup;

	// === Hide panel
	private Toggle hideInterface;

	// === Timers
	private float totalTime = 0;
	private float travelTime = 0;

	private Text totalTimeText;
	private Text travelTimeText;

	// === FPS
	static string[] stringsFrom00To99 = {
		"00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
		"10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
		"20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
		"30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
		"40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
		"50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
		"60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
		"70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
		"80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
		"90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
	};

	private int highestFPS;
	private int lowestFPS;
	private int FPS;
	private int frameRange = 60;
	private int[] fpsBuffer;
	private int fpsBufferIndex = 0;
	private Text FPSLabel, highestFPSLabel, lowestFPSLabel;

	// === MenuButton
	public Button menuButton;

	// === NextLevelButton
	public Button retryButton;

	// === LaunchButton
	public Button launchButton;

	// === Ship speed and position
	public CanvasGroup shipSpeedPanel;
	private Slider 	speedIntensitySlider;
	private Text 	speedIntensityText;
	private Slider 	speedAngleSlider;
	private Text 	speedAngleText;
	private Text 	shipX;
	private Text 	shipY;

	// === Sources informations
	protected Material lavaMaterial;
	protected Material circuitMaterial;

	public CanvasGroup sourcesInformationsPanel;
	private Slider 	sourceStrengthSlider;
	private Text 	sourceStrengthText;
	private Slider 	sourceRadiusSlider;
	private Text 	sourceRadiusText;
	private Text 	sourceX;
	private Text	sourceY;

	// === Uniform Sources informations
	public CanvasGroup uniSourcesInformationsPanel;
	private Slider 	uniSourceDxSlider;
	private Text 	uniSourceDxText;
	private Slider 	uniSourceDySlider;
	private Text 	uniSourceDyText;
	private Slider 	uniSourceWidthSlider;
	private Text 	uniSourceWidthText;
	private Slider 	uniSourceDepthSlider;
	private Text 	uniSourceDepthText;
	private Text 	uniSourceX;
	private Text	uniSourceY;

	// === Point panel
	public CanvasGroup pointPanel;
	private Text pointX;
	private Text pointY;
	private Text fPointX;
	private Text fPointY;

	// === Sources add/del panel
	public CanvasGroup sourceAddDelPanel;
	private Button addButton;
	public Button deleteButton;
	private Text fieldLeft;

	// === Minimap
	private Camera miniMapCamera;


	// ==== LIFECYCLE ====
	public UI(){
		SystemsManager.AddFSystem (this);
		InitUI ();
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		UpdateFPS ();
		UpdateTimers ();
		UpdateNextLevel ();
	}

	// ==== METHODS ====

	protected void InitUI(){
		//=== Hide Toggle
		hideInterface = GameObject.Find("HideToggle").GetComponent<Toggle>();
		hideInterface.onValueChanged.AddListener ((bool value) => OnHideToggled (value));
		mainCanvasGroup = GameObject.Find ("MainPanel").GetComponent<CanvasGroup> (); 

		// === Timers
		totalTimeText = GameObject.Find("TotalTime").GetComponent<Text>();
		travelTimeText = GameObject.Find ("TravelTime").GetComponent<Text> ();

		// === FPS
		highestFPSLabel = GameObject.Find("HighestFPSLabel").GetComponent<Text>();
		FPSLabel = GameObject.Find("FPSLabel").GetComponent<Text>();
		lowestFPSLabel = GameObject.Find("LowestFPSLabel").GetComponent<Text>();
		fpsBuffer = new int[frameRange];

		// === Menu button
		menuButton = GameObject.Find ("MenuButton").GetComponent<Button> ();
		menuButton.onClick.AddListener (() => OnMenuButtonClicked ());

		// === Retry button
		retryButton = GameObject.Find ("RetryButton").GetComponent<Button> ();
		retryButton.onClick.AddListener (() => OnRetryButtonClicked ());

		// === Launch button
		launchButton = GameObject.Find ("ButtonLaunch").GetComponent<Button> ();
		launchButton.onClick.AddListener (() => OnLaunchButtonClicked ());

		// === Ship speed and position
		shipSpeedPanel = GameObject.Find("ShipPanel").GetComponent<CanvasGroup>();
		Hide (shipSpeedPanel);
		speedIntensitySlider = GameObject.Find("SliderSpeed").GetComponent<Slider>();
		speedIntensitySlider.onValueChanged.AddListener((float value) => OnSliderIntensityChanged (value));
		speedIntensityText = GameObject.Find("TextSpeed").GetComponent<Text>();
		speedAngleSlider = GameObject.Find("SliderAngle").GetComponent<Slider>();
		speedAngleSlider.onValueChanged.AddListener((float value) => OnSliderAngleChanged (value));
		speedAngleText = GameObject.Find("TextAngle").GetComponent<Text>();
		shipX = GameObject.Find("ShipX").GetComponent<Text>();
		shipY = GameObject.Find("ShipY").GetComponent<Text>();

		// === Source Informations
		lavaMaterial = Resources.Load("Materials/LavaMaterial",typeof(Material)) as Material;
		circuitMaterial = Resources.Load("Materials/CircuitryMaterial",typeof(Material)) as Material;
		sourcesInformationsPanel = GameObject.Find("SourcesPanel").GetComponent<CanvasGroup>();
		Hide (sourcesInformationsPanel);
		sourceStrengthSlider = GameObject.Find("SliderStrength").GetComponent<Slider>();
		sourceStrengthSlider.onValueChanged.AddListener((float value) => OnSliderStrengthChanged (value));
		sourceStrengthText = GameObject.Find("TextStrength").GetComponent<Text>();
		sourceRadiusSlider = GameObject.Find("SliderRadius").GetComponent<Slider>();
		sourceRadiusSlider.onValueChanged.AddListener((float value) => OnSliderRadiusChanged (value));
		sourceRadiusText = GameObject.Find("TextRadius").GetComponent<Text>();
		sourceX = GameObject.Find ("SourceX").GetComponent<Text> ();
		sourceY = GameObject.Find ("SourceY").GetComponent<Text> ();

		// === Uniform Source informations
		uniSourcesInformationsPanel = GameObject.Find("UniformSourcesPanel").GetComponent<CanvasGroup>();
		Hide (uniSourcesInformationsPanel);
		uniSourceDxSlider = GameObject.Find ("SliderDx").GetComponent<Slider> ();
		uniSourceDxSlider.onValueChanged.AddListener ((float value) => OnSliderDxChanged (value));
		uniSourceDxText = GameObject.Find("TextDx").GetComponent<Text>();
		uniSourceDySlider = GameObject.Find ("SliderDy").GetComponent<Slider> ();
		uniSourceDySlider.onValueChanged.AddListener ((float value) => OnSliderDyChanged (value));
		uniSourceDyText = GameObject.Find("TextDy").GetComponent<Text>();
		uniSourceWidthSlider = GameObject.Find ("SliderWidth").GetComponent<Slider> ();
		uniSourceWidthSlider.onValueChanged.AddListener ((float value) => OnSliderWidthChanged (value));
		uniSourceWidthText = GameObject.Find("TextWidth").GetComponent<Text>();
		uniSourceDepthSlider = GameObject.Find ("SliderDepth").GetComponent<Slider> ();
		uniSourceDepthSlider.onValueChanged.AddListener ((float value) => OnSliderDepthChanged (value));
		uniSourceDepthText = GameObject.Find("TextDepth").GetComponent<Text>();

		uniSourceX = GameObject.Find("SourceUniX").GetComponent<Text>();
		uniSourceY = GameObject.Find("SourceUniY").GetComponent<Text>();

		// === Point panel
		pointPanel = GameObject.Find("PointPanel").GetComponent<CanvasGroup>();
		Hide (pointPanel);
		pointX = GameObject.Find("PointX").GetComponent<Text>();
		pointY = GameObject.Find("PointY").GetComponent<Text>();
		fPointX = GameObject.Find("FPointX").GetComponent<Text>();
		fPointY = GameObject.Find("FPointY").GetComponent<Text>();


		// === Add & Delete panel
		sourceAddDelPanel = GameObject.Find("AddSourcesPanel").GetComponent<CanvasGroup>();
		addButton = GameObject.Find ("AddButton").GetComponent<Button> ();
		addButton.onClick.AddListener (() => OnAddButtonClicked ());
		deleteButton = GameObject.Find ("DeleteButton").GetComponent<Button> ();
		deleteButton.onClick.AddListener (() => OnDeleteButtonClicked ());
		deleteButton.interactable = false;
		fieldLeft = GameObject.Find("SourcesLeft").GetComponent<Text>();
		fieldLeft.text = ""+GameObject.Find ("AddSourcesPanel").GetComponent<FieldsCounter> ().fieldsLeft;

		// === MiniMap
		miniMapCamera = GameObject.Find("SecondaryCamera").GetComponent<Camera>();	
	}

	// === Hide Toggle
	protected void OnHideToggled(bool value){
		if (value) {
			miniMapCamera.gameObject.SetActive (false);
			mainCanvasGroup.alpha = 0;
			mainCanvasGroup.blocksRaycasts = false;
		} else {
			miniMapCamera.gameObject.SetActive (true);
			mainCanvasGroup.alpha = 1;
			mainCanvasGroup.blocksRaycasts = true;		}
	}

	// === Timer
	protected void UpdateTimers(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		if (gl == null) {return;}

		// update timers
		if (gl.state != GameLogic.STATES.LOST && gl.state != GameLogic.STATES.WON) {
			totalTime += Time.deltaTime;
		}
		if (gl.state==GameLogic.STATES.PLAYING) {
			travelTime += Time.deltaTime;
		}

		// update UI
		int roundedTotalTime = (int)Mathf.Round(100f * totalTime);
		int seconds = roundedTotalTime/100;
		int centiemes = roundedTotalTime % 100;
		totalTimeText.text = string.Format ("{0:0}.{1:00} s", seconds, centiemes);

		int roundedTravelTime = (int)Mathf.Round(100f * travelTime);
		seconds = roundedTravelTime / 100;
		centiemes = roundedTravelTime % 100;
		travelTimeText.text = string.Format ("{0:0}.{1:00} s", seconds, centiemes);
	}

	// === FPS
	protected void UpdateFPS(){
		// add last frame
		fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
		if (fpsBufferIndex >= frameRange) {
			fpsBufferIndex = 0;
		}
		// calculate average
		int sum = 0;
		int highest = 0;
		int lowest = int.MaxValue;
		for (int i = 0; i < frameRange; i++) {
			int fps = fpsBuffer[i];
			sum += fps;
			if (fps > highest) {
				highest = fps;
			}
			if (fps < lowest) {
				lowest = fps;
			}
		}
		FPS = sum / frameRange;
		highestFPS = highest;
		lowestFPS = lowest;

		// update UI
		highestFPSLabel.text = stringsFrom00To99[Mathf.Clamp(highestFPS, 0, 99)];
		FPSLabel.text = stringsFrom00To99[Mathf.Clamp(FPS, 0, 99)];
		lowestFPSLabel.text = stringsFrom00To99[Mathf.Clamp(lowestFPS, 0, 99)];
	}


	// === Showing Next level button and updating number of unlocked levels
	public void UpdateNextLevel(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		GameObject gameInfos = GameObject.Find("GameInformations");
		GameInformations levelInfos = gameInfos.GetComponent<GameInformations> ();
		if (gl == null) {return;}	

		// if level is won and is not the last one
		if (gl.state == GameLogic.STATES.WON && levelInfos.noLevel < levelInfos.totalLevels) {
			// if first win, save it
			if (levelInfos.unlockedLevels < levelInfos.totalLevels && levelInfos.noLevel == levelInfos.unlockedLevels) {
				levelInfos.unlockedLevels++;
				Debug.Log ("Level " + levelInfos.unlockedLevels + " unlocked!");
			}
		}
	}


	// === Next level button
	protected void OnRetryButtonClicked(){
		// Reset game state
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem ("PlayerActions");
		gl.state = GameLogic.STATES.SETUP;

		// reset ship
		gl.InitShip ();
		GameObject s = shipFamily.First ();
		s.GetComponent<Editable> ().editable = true;
		if (pa.previousGameObject == s) {
			s.GetComponent<Renderer> ().material = pa.selectedAndEditableMaterial;
			UpdateShipInformations (s);
		}

		// reset sources
		foreach (GameObject src in editableSourcesFamily) {
			src.GetComponent<Editable> ().editable = true;
			if (pa.previousGameObject == src) {
				src.GetComponent<Renderer> ().material = pa.selectedAndEditableMaterial;

				if (src.GetComponent<Field> ().isUniform) {
					UpdateUniSourcesInformations (src);
				} else {
					UpdateSourcesInformations (src);
				}
			}
		}

		// reset ui & terrain
		travelTime = 0;
		launchButton.GetComponentInChildren<Text> ().text = "Launch";
		FieldsCounter fc = FamilyManager.getFamily (new AllOfComponents (typeof(FieldsCounter))).First ().GetComponent<FieldsCounter> ();
		if (fc.sources.Length>0) {
			Show (sourceAddDelPanel);
		}

		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem ("ForcesDisplay");
		fd.refresh ();
	}


	// === Menu button
	protected void OnMenuButtonClicked(){
		SystemsManager.ResetFSystems ();
		GameObjectManager.loadScene ("MenuScene");
	}


	// === Launch button
	protected void OnLaunchButtonClicked(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		GameObject gameInfos = GameObject.Find("GameInformations");
		GameInformations levelInfos = gameInfos.GetComponent<GameInformations> ();

		switch (gl.state) {
		case GameLogic.STATES.SETUP:
			gl.OnPlay ();
			launchButton.GetComponentInChildren<Text> ().text = "Pause";
			break;
		case GameLogic.STATES.PLAYING:
			gl.OnPause ();
			launchButton.GetComponentInChildren<Text> ().text = "Play";
			break;
		case GameLogic.STATES.PAUSED:
			gl.OnPlay ();
			launchButton.GetComponentInChildren<Text> ().text = "Pause";
			break;
		case GameLogic.STATES.WON:
			if (levelInfos.noLevel < levelInfos.totalLevels) {
				levelInfos.noLevel++;
				Debug.Log ("Go to level "+levelInfos.noLevel);
				SystemsManager.ResetFSystems ();
				GameObjectManager.loadScene ("level_"+levelInfos.noLevel);
			}
			break;
		case GameLogic.STATES.LOST:
			OnRetryButtonClicked ();
			break;
		default:
			break;
		}
	}

	// === Ship speed
	public void UpdateShipInformations(GameObject ship){
		Position p = ship.GetComponent<Position> ();
		Movement m = ship.GetComponent<Movement> ();
		speedIntensityText.text = m.speed.magnitude.ToString("F2") + " m/s";
		speedAngleText.text = ((int)(Mathf.Atan2(m.speed.y,m.speed.x)*Mathf.Rad2Deg)) + "°";
		shipX.text = p.pos.x.ToString ("F3") + " m";
		shipY.text = p.pos.y.ToString ("F3") + " m";

		if (ship.GetComponent<Editable> ().editable) {
			shipSpeedPanel.interactable = true;
		} else {
			shipSpeedPanel.interactable = false;
		}
	}

	protected void OnSliderIntensityChanged(float value){
		value = value/100f;
		Movement m = shipFamily.First ().GetComponent<Movement> ();

		// New speed vector and text
		m.speed.Normalize ();
		m.speed *= value;
		speedIntensityText.text = value +" m/s";
	}

	protected void OnSliderAngleChanged(float value){
		GameObject ship = shipFamily.First ();
		Movement m = ship.GetComponent<Movement> ();

		// Turn the ship
		Transform t = ship.transform;
		t.rotation=Quaternion.Euler(90, (360-value+90)%360, 0);

		// New speed vector
		float oldMagnitude = m.speed.magnitude;
		m.speed = new Vector3(Mathf.Cos(Mathf.Deg2Rad*value),Mathf.Sin(Mathf.Deg2Rad*value),0);
		m.speed.Normalize();
		m.speed *= oldMagnitude;

		// New Text
		speedAngleText.text = (int)value +"°";
	}

	// === Source informations
	public void UpdateSourcesInformations(GameObject sources){
		Position p = sources.GetComponent<Position> ();
		Field f = sources.GetComponent<Field> ();

		// values
		int nextValue = (int)(f.A*100);
		if (f.isRepulsive) {
			sourceStrengthSlider.maxValue = 100;
			sourceStrengthSlider.minValue = 0;
		} else {
			sourceStrengthSlider.maxValue = 0;
			sourceStrengthSlider.minValue = -100;
		}
		sourceStrengthSlider.value = nextValue;
			
		sourceRadiusSlider.value = (int)(f.sigx*100);
		sourceX.text = p.pos.x.ToString ("F3") + " m";
		sourceY.text = p.pos.y.ToString ("F3") + " m";

		// editable ?
		if (sources.GetComponent<Editable> () == null || !sources.GetComponent<Editable> ().editable) {
			sourcesInformationsPanel.interactable = false;
		} else {
			sourcesInformationsPanel.interactable = true;
		}
	}

	protected void OnSliderStrengthChanged(float value){
		value = value/100f;

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.A = value;

		// Change look according to field strength
		if (value > 0) {
			pa.previousMaterial = circuitMaterial;
		} else {
			pa.previousMaterial = lavaMaterial;
		}
			
		// New text
		sourceStrengthText.text = value +"";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	protected void OnSliderRadiusChanged(float value){
		value = value/100f;

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.sigx = value;
		f.sigy = value;

		// New Text
		sourceRadiusText.text = value +" m";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	// === Uniform source informations
	public void UpdateUniSourcesInformations(GameObject sources){
		Position p = sources.GetComponent<Position> ();
		Field f = sources.GetComponent<Field> ();

		// values
		uniSourceDxSlider.value = (int)Mathf.Round(f.b*5000);
		uniSourceDySlider.value = (int)Mathf.Round(f.c*5000);
		uniSourceWidthSlider.value = (int)(f.sigx*200);
		uniSourceDepthSlider.value = (int)(f.sigy*200);
		uniSourceX.text = p.pos.x.ToString ("F3") + " m";
		uniSourceY.text = p.pos.y.ToString ("F3") + " m";

		// Editable ?
		if (sources.GetComponent<Editable> () == null || !sources.GetComponent<Editable> ().editable) {
			uniSourcesInformationsPanel.interactable = false;
		} else {
			uniSourcesInformationsPanel.interactable = true;
		}
	}

	protected void OnSliderWidthChanged(float value){
		value = value/100f;

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.sigx = value/2.0f;
		// New Text
		uniSourceWidthText.text = value +" m";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	protected void OnSliderDepthChanged(float value){
		value = value/100f;		

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.sigy = value/2.0f;

		// New Text
		uniSourceDepthText.text = value +" m";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	protected void OnSliderDxChanged(float value){
		value = value/5000f;		

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.b = value;

		// New Text
		uniSourceDxText.text = value*50 +"";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	protected void OnSliderDyChanged(float value){
		value = value/5000f;		

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.c = value;

		// New Text
		uniSourceDyText.text = value*50+"";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
	}

	// === Point informations
	public void UpdatePointInformations(Vector3 point){
		TerrainData td = PPlanFamily.First ().GetComponent<Terrain> ().terrainData;

		// update coordinates
		float x = point.x/td.size.x;
		pointX.text = x.ToString ("F3")+" m";
		float y = point.z/td.size.z;
		pointY.text = y.ToString ("F3")+" m";

		// update forces
		ForcesComputation fc = (ForcesComputation)SystemsManager.GetFSystem("ForcesComputation");
		Vector3 f = fc.computeForceAt (x, y);
		fPointX.text = f.x.ToString ("F3")+" kg.m/s2";
		fPointY.text = f.y.ToString ("F3")+" kg.m/s2";

	}

	// === Add & Delete Panel
	protected void OnAddButtonClicked(){
		FieldsCounter fc = FamilyManager.getFamily (new AllOfComponents (typeof(FieldsCounter))).First ().GetComponent<FieldsCounter> ();
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem ("GameLogic");

		// field left
		if (fc.fieldsLeft > 0  && gl.state==GameLogic.STATES.SETUP) {
			GameObject s = fc.sources [fc.fieldsLeft - 1];
			s.SetActive (true);
			GameObjectManager.bind (s);

			// update UI
			fc.fieldsLeft--;
			fieldLeft.text = "" + fc.fieldsLeft;
			if (fc.fieldsLeft == 0) {
				addButton.interactable = false;
			}
		}
	}

	protected void OnDeleteButtonClicked(){
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		FieldsCounter fc = FamilyManager.getFamily (new AllOfComponents (typeof(FieldsCounter))).First ().GetComponent<FieldsCounter> ();

		// a field is indeed selected
		if (pa.previousGameObject!=null && editableSourcesFamily.contains(pa.previousGameObject.GetInstanceID())) {
			// update UI
			fc.fieldsLeft++;
			fieldLeft.text = "" + fc.fieldsLeft;

			// we delete it
			fc.sources[fc.fieldsLeft-1]=pa.previousGameObject;
			GameObjectManager.unbind (pa.previousGameObject);
			pa.previousGameObject.GetComponent<Renderer> ().material = pa.previousMaterial;
			pa.previousGameObject.SetActive (false);
			pa.previousGameObject = null;
			pa.previousMaterial = null;
			Hide (sourcesInformationsPanel);
			Hide (uniSourcesInformationsPanel);
			addButton.interactable = true;
		}
	}

	// === General
	public void Hide(CanvasGroup cg){
		cg.alpha = 0;
		cg.blocksRaycasts = false;
	}

	public void Show(CanvasGroup cg){
		cg.alpha = 1;
		cg.blocksRaycasts = true;
	}
}