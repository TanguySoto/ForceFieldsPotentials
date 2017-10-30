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

	private Family shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));

	// === Timers
	private int totalTime = 0;
	private int travelTime = 0;

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

	// === LaunchButton
	public Button launchButton;

	// === Ship speed
	public CanvasGroup shipSpeedPanel;
	private Slider 	speedIntensitySlider;
	private Text 	speedIntensityText;
	private Slider 	speedAngleSlider;
	private Text 	speedAngleText;

	// === Sources informations
	public CanvasGroup sourcesInformationsPanel;
	private Slider 	sourceStrengthSlider;
	private Text 	sourceStrengthText;
	private Slider 	sourceRadiusSlider;
	private Text 	sourceRadiusText;

	private bool isUIInit = false;

	// ==== LIFECYCLE ====
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isUIInit) {
			SystemsManager.AddFSystem (this);
			InitUI ();
			isUIInit = true;
		}
	}

	protected override void onProcess(int familiesUpdateCount) {
		UpdateFPS ();
		UpdateTimers ();

	}

	// ==== METHODS ====

	protected void InitUI(){
		// === Timers
		totalTimeText = GameObject.Find("TotalTime").GetComponent<Text>();
		travelTimeText = GameObject.Find ("TravelTime").GetComponent<Text> ();

		// === FPS
		highestFPSLabel = GameObject.Find("HighestFPSLabel").GetComponent<Text>();
		FPSLabel = GameObject.Find("FPSLabel").GetComponent<Text>();
		lowestFPSLabel = GameObject.Find("LowestFPSLabel").GetComponent<Text>();
		fpsBuffer = new int[frameRange];

		// === Launch button
		launchButton = GameObject.Find ("ButtonLaunch").GetComponent<Button> ();
		launchButton.onClick.AddListener (() => OnLaunchButtonClicked ());

		// === Ship speed
		shipSpeedPanel = GameObject.Find("ShipPanel").GetComponent<CanvasGroup>();
		Hide (shipSpeedPanel);
		speedIntensitySlider = GameObject.Find("SliderSpeed").GetComponent<Slider>();
		speedIntensitySlider.onValueChanged.AddListener((float value) => OnSliderIntensityChanged (value));
		speedIntensityText = GameObject.Find("TextSpeed").GetComponent<Text>();
		speedAngleSlider = GameObject.Find("SliderAngle").GetComponent<Slider>();
		speedAngleSlider.onValueChanged.AddListener((float value) => OnSliderAngleChanged (value));
		speedAngleText = GameObject.Find("TextAngle").GetComponent<Text>();

		// === Source Informations
		sourcesInformationsPanel = GameObject.Find("SourcesPanel").GetComponent<CanvasGroup>();
		Hide (sourcesInformationsPanel);
		sourceStrengthSlider = GameObject.Find("SliderStrength").GetComponent<Slider>();
		sourceStrengthSlider.onValueChanged.AddListener((float value) => OnSliderStrengthChanged (value));
		sourceStrengthText = GameObject.Find("TextStrength").GetComponent<Text>();
		sourceRadiusSlider = GameObject.Find("SliderRadius").GetComponent<Slider>();
		sourceRadiusSlider.onValueChanged.AddListener((float value) => OnSliderRadiusChanged (value));
		sourceRadiusText = GameObject.Find("TextRadius").GetComponent<Text>();
	}

	// === Timer
	protected void UpdateTimers(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

		// update timers
		if (gl.state != GameLogic.STATES.LOST && gl.state != GameLogic.STATES.WON) {
			totalTime += (int)Mathf.Round (100f * Time.deltaTime);
		}
		if (gl.state==GameLogic.STATES.PLAYING) {
			travelTime += (int)Mathf.Round (100f * Time.deltaTime);
		}

		// update UI
		int seconds = totalTime/100;
		int centiemes = totalTime % 100;
		totalTimeText.text = string.Format ("{0:0}.{1:00} s", seconds, centiemes);
		seconds = travelTime / 100;
		centiemes = travelTime % 100;
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
		
	// === Launch button
	protected void OnLaunchButtonClicked(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

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
			SystemsManager.ResetFSystems ();
			GameObjectManager.loadScene ("scene_test");
			break;
		case GameLogic.STATES.LOST:
			SystemsManager.ResetFSystems ();
			GameObjectManager.loadScene ("scene_test");
			break;
		default:
			break;
		}
	}

	// === Ship speed
	public void UpdateShipInformations(GameObject ship){
		Movement m = ship.GetComponent<Movement> ();
		speedIntensityText.text = (Mathf.Round(100f*m.speed.magnitude)/100f) + " m/s";
		speedAngleText.text = ((int)(Mathf.Atan2(m.speed.y,m.speed.x)*Mathf.Rad2Deg)) + "°";

		if (ship.GetComponent<Editable> () == null) {
			speedAngleSlider.enabled = false;
			speedIntensitySlider.enabled = false;
		} else {
			speedAngleSlider.enabled = true;
			speedIntensitySlider.enabled = true;
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
		Field f = sources.GetComponent<Field> ();
		sourceStrengthSlider.value = (int)(f.A*100);
		sourceRadiusSlider.value = (int)(f.sigx*100);

		if (sources.GetComponent<Editable> () == null) {
			sourceStrengthSlider.enabled = false;
			sourceRadiusSlider.enabled = false;
		} else {
			sourceStrengthSlider.enabled = true;
			sourceRadiusSlider.enabled = true;
		}
	}

	protected void OnSliderStrengthChanged(float value){
		value = value/100f;

		// New value
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameObject source = pa.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.A = value;

		// New  text
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
		sourceRadiusText.text = value +"";

		// Update scene
		ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem("ForcesDisplay");
		fd.refresh ();
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

	public void HideAllPanels(){
		// Ship
		shipSpeedPanel.alpha = 0;
		shipSpeedPanel.blocksRaycasts = false;
		// Sources
		sourcesInformationsPanel.alpha = 0;
		sourcesInformationsPanel.blocksRaycasts = false;
	}
}