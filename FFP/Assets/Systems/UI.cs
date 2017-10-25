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

	private Button launchButton;

	private Slider speedIntensitySlider;
	private Text speedIntensityText;
	private Slider speedAngleSlider;
	private Text speedAngleText;

	private bool isUIInit = false;

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
		// Launch button
		launchButton = GameObject.Find ("ButtonLaunch").GetComponent<Button> ();
		launchButton.onClick.AddListener (() => OnLaunchButtonClicked ());

		// Ship speed
		speedIntensitySlider = GameObject.Find("SliderSpeed").GetComponent<Slider>();
		speedIntensitySlider.onValueChanged.AddListener((float value) => OnSliderIntensityChanged (value));
		speedIntensityText = GameObject.Find("TextSpeed").GetComponent<Text>();
		speedAngleSlider = GameObject.Find("SliderAngle").GetComponent<Slider>();
		speedAngleSlider.onValueChanged.AddListener((float value) => OnSliderAngleChanged (value));
		speedAngleText = GameObject.Find("TextAngle").GetComponent<Text>();
	}

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

	protected void OnSliderIntensityChanged(float value){
		value = value/50f;
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
}