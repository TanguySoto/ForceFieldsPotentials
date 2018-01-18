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


public class UITuto : FSystem {


	private Family shipFamily 	= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family editableSourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position), typeof(Editable)));


	// State of the TUTO
	public int state = 0;

	// Panels
	public CanvasGroup middlePanel;
	public CanvasGroup topPanel;

	// Next buttons
	public Button middleNextButton;
	public Button topNextButton;

	// Texts
	public Text middleText;
	public Text topText;

	// Bonus image
	public GameObject img;

	public GameObject obstacle;

	// UI
	protected UI ui;

	// ==== LIFECYCLE ====
	public UITuto(){
		SystemsManager.AddFSystem (this);

		// Locked the hide interface toggle
		Toggle hideInterface = GameObject.Find("HideToggle").GetComponent<Toggle>();
		hideInterface.enabled = false;

		// Hide everything from the interface
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		//ui.OnHideToggled (true);

		middlePanel = GameObject.Find("MiddlePanel").GetComponent<CanvasGroup>();
		topPanel = GameObject.Find("TopPanel").GetComponent<CanvasGroup>();

		middleNextButton = GameObject.Find ("MiddleNextButton").GetComponent<Button> ();
		middleNextButton.onClick.AddListener (() => OnMiddleNextButtonClicked ());

		topNextButton = GameObject.Find ("TopNextButton").GetComponent<Button> ();
		topNextButton.onClick.AddListener (() => OnTopNextButtonClicked ());
		topNextButton.gameObject.SetActive(false);

		middleText = GameObject.Find ("MiddleText").GetComponent<Text> ();
		topText = GameObject.Find ("TopText").GetComponent<Text> ();

		obstacle = GameObject.Find ("Obstacle");
		obstacle.SetActive (false);

		img = GameObject.Find ("ImageBonus");
		img.SetActive (false);

		ui.Hide (topPanel);
		state = 0;
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		ui = (UI)SystemsManager.GetFSystem("UI");
		if (ui == null) {return;}

		HideEverything ();
		if (state == 1) {
			ShipState ();
		}
		if (state == 2) {
			obstacle.SetActive (true);
			ObstacleState ();
		}
	}


	protected void OnMiddleNextButtonClicked(){
		if (state == 0) {
			ui.Hide (middlePanel);
			ui.Show (topPanel);
			state = 1;
		}
		if (state == 3) {
			SystemsManager.ResetFSystems ();
			GameObjectManager.loadScene ("MenuScene");
		}
	}


	protected void OnTopNextButtonClicked(){
		if (state == 1) {
			ui.OnRetryButtonClicked ();
			state = 2;
		}
	}

	protected void ShipState(){
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

		GameObject ship = shipFamily.First ();
		if (pa.previousGameObject == ship) {
			topText.text = "Good job! Let's aim for the Eart now.\n" +
							"To do so, use the sliders at the bottom of the screen.\n"+
							"(You can use the keyboard arrows.)";
			
			ui.Show (ui.shipSpeedPanel);
		}
		ui.launchButton.gameObject.SetActive(true);


		if (gl.state == GameLogic.STATES.WON) {
			ui.launchButton.gameObject.SetActive (false);
			topText.text = "You've done it! Wasn't that hard, was it?\n" +
				"Let see what else we can do.";
			topNextButton.gameObject.SetActive(true);
		}
	}


	protected void ObstacleState(){
		topText.text = "Oh! A wild obstacle appeared!\n" +
			"Luckily for us, we can deviate the ship with planets and force fields.\n" +
			"Use the panel on you right to do so.";
		topNextButton.gameObject.SetActive(false);
		ui.Show (ui.sourceAddDelPanel);

		GameObject ship = shipFamily.First ();
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

		if (pa.previousGameObject == ship) {
			ui.Show (ui.shipSpeedPanel);
		}
		ui.launchButton.gameObject.SetActive (true);

		if (pa.previousGameObject!=null && editableSourcesFamily.contains(pa.previousGameObject.GetInstanceID())){

			topText.text = "Move the force field wherever you want.\nAs you can see on the bottom right, you can modify the paramaters of the source.\n";
			if (pa.previousGameObject.GetComponent<Field> ().isUniform) {
				topText.text += "Here we have an uniform force field.";
				ui.UpdateUniSourcesInformations (pa.previousGameObject);
				ui.Show (ui.uniSourcesInformationsPanel);
				ui.Hide (ui.sourcesInformationsPanel);
			} else {
				topText.text += "Here we have a pulling planet.";
				ui.UpdateSourcesInformations (pa.previousGameObject);
				ui.Show (ui.sourcesInformationsPanel);
				ui.Hide (ui.uniSourcesInformationsPanel);
			}
		}

		if (gl.state == GameLogic.STATES.WON) {
			ui.launchButton.gameObject.SetActive (false);
			ui.Hide (topPanel);
			ui.Show (middlePanel);
			middleText.text = "Congratulations, you've done it once more!\n" +
				"Now, before I let you explore dangerous worlds I'll give you one last tip.\n" +
				"To get a higher score after completing a level, do it again while collecting those little bonus called seriousgamium:";
			middleText.fontSize = 20;
			img.SetActive (true);
			GameObject.Find ("MiddleTextButton").GetComponent<Text>().text = "Menu";
			state = 3;
		}
	}

	protected void HideEverything(){
		ui.Hide (ui.shipSpeedPanel);
		ui.Hide (ui.sourcesInformationsPanel);
		ui.Hide (ui.uniSourcesInformationsPanel);
		ui.Hide (ui.pointPanel);
		ui.Hide (ui.endPanel);
		ui.Hide (ui.sourceAddDelPanel);
		ui.retryButton.gameObject.SetActive(false);
		ui.launchButton.gameObject.SetActive(false);
	}
}
