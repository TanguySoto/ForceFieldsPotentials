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

public class UI : FSystem {

	// ==== VARIABLES ====

	private static Family shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));

	// === LaunchButton
	private static Button launchButton;

	// === Ship speed
	public static CanvasGroup shipSpeedPanel;
	private static Slider speedIntensitySlider;
	private static Text speedIntensityText;
	private static Slider speedAngleSlider;
	private static Text speedAngleText;

	// === Sources informations
	public static CanvasGroup sourcesInformationsPanel;
	private static Slider sourceStrengthSlider;
	private static Text sourceStrengthText;
	private static Slider sourceRadiusSlider;
	private static Text sourceRadiusText;

	private static bool isUIInit = false;

	// ==== LIFECYCLE ====

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isUIInit) {
			InitUI ();
			isUIInit = true;
		}
	}

	protected override void onProcess(int familiesUpdateCount) {
	}

	// ==== METHODS ====

	protected void InitUI(){
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
		
	// === Launch button
	protected void OnLaunchButtonClicked(){
		switch (GameLogic.state) {
		case GameLogic.STATES.SETUP:
			GameLogic.OnPlay ();
			launchButton.GetComponentInChildren<Text> ().text = "Pause";
			break;
		case GameLogic.STATES.PLAYING:
			GameLogic.OnPause ();
			launchButton.GetComponentInChildren<Text> ().text = "Play";
			break;
		case GameLogic.STATES.PAUSED:
			GameLogic.OnPlay ();
			launchButton.GetComponentInChildren<Text> ().text = "Pause";
			break;
		case GameLogic.STATES.WON:
			// TODO
			break;
		case GameLogic.STATES.LOST:
			// TODO
			break;
		default:
			break;
		}
	}

	// === Ship speed
	public static void UpdateShipInformations(GameObject ship){
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
	public static void UpdateSourcesInformations(GameObject sources){
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
		GameObject source = PlayerActions.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.A = value;

		// New  text
		sourceStrengthText.text = value +"";

		// Update scene
		ForcesDisplay.refresh();
	}

	protected void OnSliderRadiusChanged(float value){
		value = value/100f;

		// New value
		GameObject source = PlayerActions.previousGameObject;
		Field f = source.GetComponent<Field> ();
		f.sigx = value;
		f.sigy = value;

		// New Text
		sourceRadiusText.text = value +"";

		// Update scene
		ForcesDisplay.refresh();
	}

	// === General
	public static void Hide(CanvasGroup cg){
		cg.alpha = 0;
		cg.blocksRaycasts = false;
	}

	public static void Show(CanvasGroup cg){
		cg.alpha = 1;
		cg.blocksRaycasts = true;
	}

	public static void HideAllPanels(){
		// Ship
		shipSpeedPanel.alpha = 0;
		shipSpeedPanel.blocksRaycasts = false;
		// Sources
		sourcesInformationsPanel.alpha = 0;
		sourcesInformationsPanel.blocksRaycasts = false;
	}
}