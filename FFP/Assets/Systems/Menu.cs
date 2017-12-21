using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


using FYFY;
using System.Collections;
using System.Collections.Generic;


/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Menu : FSystem {

	private Family createGameInfosFamily = FamilyManager.getFamily (new AllOfComponents(typeof(CreateGameInfos)));


	public Button playButton;
	public Button levelsButton;
	public Button helpButton;
	public Button creditsButton;


	public GameObject gameInfos;



	// ==== LIFECYCLE ====
	public Menu(){
		InitMenu ();
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		UpdateMenu ();
	}



	// ==== METHODS ====


	protected void InitMenu(){
		// TO RESET PLAYER PREF
		//PlayerPrefs.DeleteAll ();

		// Loading saved game informations
		gameInfos = GameObject.Find ("GameInformations");

		if (gameInfos == null) {
			GameObject goCreateGameInfos = createGameInfosFamily.First ();
			gameInfos = GameObject.Instantiate (goCreateGameInfos.GetComponent<CreateGameInfos> ().gameInfosPrefab);
			gameInfos.name = "GameInformations";
			Object.DontDestroyOnLoad (gameInfos);
		}


		if (PlayerPrefs.HasKey ("highestUnlockedLevel")) {
			GameInformations levelInfos = gameInfos.GetComponent<GameInformations> ();
			levelInfos.unlockedLevels = PlayerPrefs.GetInt ("highestUnlockedLevel");
		}


		// === Play button
		playButton = GameObject.Find ("PlayButton").GetComponent<Button> ();
		playButton.onClick.AddListener (() => OnPlayButtonClicked ());


		// === Levels button
		playButton = GameObject.Find ("LevelsButton").GetComponent<Button> ();
		playButton.onClick.AddListener (() => OnLevelsButtonClicked ());


		// === Help button
		playButton = GameObject.Find ("HelpButton").GetComponent<Button> ();
		playButton.onClick.AddListener (() => OnHelpButtonClicked ());


		// === Credits button
		playButton = GameObject.Find ("CreditsButton").GetComponent<Button> ();
		playButton.onClick.AddListener (() => OnCreditsButtonClicked ());

	}


	protected void OnPlayButtonClicked (){
		GameObjectManager.loadScene("level_"+gameInfos.GetComponent<GameInformations> ().unlockedLevels);
	}


	protected void OnLevelsButtonClicked(){
		GameObjectManager.loadScene("LevelScene");
	}

	protected void OnHelpButtonClicked(){
		GameObjectManager.loadScene("HelpScene");
	}

	protected void OnCreditsButtonClicked(){
		GameObjectManager.loadScene("CreditsScene");
	}

	public void UpdateMenu(){

	}
}
