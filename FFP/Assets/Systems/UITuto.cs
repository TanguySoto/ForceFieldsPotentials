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


	// State of the Tuto
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

	// system UI
	protected UI ui;



	// ==== LIFECYCLE ====
	public UITuto(){
		SystemsManager.AddFSystem (this);

		// Lock the hide interface toggle so the player can't interact with it
		Toggle hideInterface = GameObject.Find("HideToggle").GetComponent<Toggle>();
		hideInterface.enabled = false;

		UI ui = (UI)SystemsManager.GetFSystem("UI");

		// The panel at the middle of the screen (first and last one of the tutorial)
		// with its next button and its text
		middlePanel = GameObject.Find("MiddlePanel").GetComponent<CanvasGroup>();
		middleNextButton = GameObject.Find ("MiddleNextButton").GetComponent<Button> ();
		middleNextButton.onClick.AddListener (() => OnMiddleNextButtonClicked ());
		middleText = GameObject.Find ("MiddleText").GetComponent<Text> ();

		// The panel at the top of the screen
		// with its next button and its text
		topPanel = GameObject.Find("TopPanel").GetComponent<CanvasGroup>();
		topNextButton = GameObject.Find ("TopNextButton").GetComponent<Button> ();
		topNextButton.onClick.AddListener (() => OnTopNextButtonClicked ());
		topNextButton.gameObject.SetActive(false);
		topText = GameObject.Find ("TopText").GetComponent<Text> ();

		// An obstacle for state 2 of the tuto
		obstacle = GameObject.Find ("Obstacle");
		obstacle.SetActive (false);

		// Image illustrating a bonus
		img = GameObject.Find ("ImageBonus");
		img.SetActive (false);

		ui.Hide (topPanel);

		// Starting state
		state = 0;
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		ui = (UI)SystemsManager.GetFSystem("UI");
		if (ui == null) {return;}

		// Hide everything and then show what is needed at this point of the tuto
		HideEverything ();

		// State 1 is for the first "scene": the player just has to reach Earth
		// using the angle and speed of the ship
		if (state == 1) {
			ShipState ();
		}

		// State 2 is for the second "scene": avoid the obstacle using one planet
		// and eventually the force field
		if (state == 2) {
			obstacle.SetActive (true);
			ObstacleState ();
		}
	}


	protected void OnMiddleNextButtonClicked(){
		// First time it is clicked
		if (state == 0) {
			ui.Hide (middlePanel);
			ui.Show (topPanel);
			state = 1;
		}

		// It is clicked a last time at the end
		if (state == 3) {
			SystemsManager.ResetFSystems ();
			GameObjectManager.loadScene ("MenuScene");
		}
	}


	protected void OnTopNextButtonClicked(){
		// After completing the state 1
		if (state == 1) {
			ui.OnRetryButtonClicked ();
			state = 2;
		}
	}

	// State 1
	protected void ShipState(){
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

		GameObject ship = shipFamily.First ();

		// Once the ship is selected
		if (pa.previousGameObject == ship) {
			topText.text = "Good job! Let's aim for the Eart now.\n" +
							"To do so, use the sliders at the bottom of the screen.\n"+
							"(You can use the keyboard arrows.)";
			
			ui.Show (ui.shipSpeedPanel);
			ui.launchButton.gameObject.SetActive(true);
		}

		// Once the goal is reached
		if (gl.state == GameLogic.STATES.WON) {
			ui.launchButton.gameObject.SetActive (false);
			topText.text = "You've done it! Wasn't that hard, was it?\n" +
				"Let see what else we can do.";
			topNextButton.gameObject.SetActive(true);
		}
	}


	// State 2
	protected void ObstacleState(){
		// Every time the player is not selecting a source
		topText.text = "Oh! A wild obstacle appeared!\n" +
			"Luckily for us, we can deviate the ship with planets and force fields.\n" +
			"Use the panel on you right to do so.";
		topNextButton.gameObject.SetActive(false);

		// Show the add/del panel
		ui.Show (ui.sourceAddDelPanel);

		GameObject ship = shipFamily.First ();
		PlayerActions pa = (PlayerActions)SystemsManager.GetFSystem("PlayerActions");
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");

		if (pa.previousGameObject == ship) {
			ui.Show (ui.shipSpeedPanel);
		}
		ui.launchButton.gameObject.SetActive (true);

		// A source is selected
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

		// Once the goal is reached
		if (gl.state == GameLogic.STATES.WON) {
			ui.launchButton.gameObject.SetActive (false);
			ui.Hide (topPanel);
			ui.Show (middlePanel);
			middleText.text = "Congratulations, you've done it once more!\n" +
				"Now, before I let you explore dangerous worlds I'll give you one last tip.\n" +
				"To get a higher score after completing a level, do it again while collecting those little bonus called Seriousgamium:";
			middleText.fontSize = 20;
			img.SetActive (true);
			GameObject.Find ("MiddleTextButton").GetComponent<Text>().text = "Menu";
			state = 3;
		}
	}


	// Hide everything from the interface
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
