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

public class Levels : FSystem {

	private Family menuFamily = FamilyManager.getFamily (new AllOfComponents(typeof(ButtonLevel)));
	//private Family gameInfosFamily = FamilyManager.getFamily (new AllOfComponents (typeof(GameInformations))); // Should be only one - and in DontDestroyOnLoad


	private GameObject[] buttonLevels;


	public int buttonPerLine = 4;
	public float distanceBetweenLines = 100f;
	public float distanceBetweenButtons = 100f;
	public float offsetTopBot = 50f; // Spaces before and after the buttons


	// width and height of the level buttons
	float buttonWidth; 
	float buttonHeight;


	// Position coordinates for the image holding the buttons
	public Vector3 menuCoordinates = new Vector3(20f, 0f, 0f);


	// Component holding game informations - should be access in DontDestroyOnLoad
	GameObject gameInfos;


	// ==== LIFECYCLE ====
	public Levels(){
		InitMenu ();
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}
		
	protected override void onProcess(int familiesUpdateCount) {
		UpdateMenuLevel ();
	}



	// ==== METHODS ====


	public void InitMenu(){

		GameObject menu = menuFamily.First ();
		gameInfos = GameObject.Find("GameInformations");
		GameObject menuFrame = GameObject.Find ("Menu"); // to get the menu stretched size


		ButtonLevel buttonLevel = menu.GetComponent<ButtonLevel> ();
		RectTransform menuRectT = menu.GetComponent<RectTransform> ();

		GameInformations levelInfos = gameInfos.GetComponent<GameInformations>();
		int totalLevels = levelInfos.totalLevels;


		buttonLevels = new GameObject[totalLevels];


		// width and height of the level buttons
		buttonWidth = buttonLevel.buttonLevelPrefab.GetComponent<RectTransform>().sizeDelta.x + 15f; // Size not right between prefab and instantiated object
		buttonHeight = buttonLevel.buttonLevelPrefab.GetComponent<RectTransform>().sizeDelta.y + 15f;


		// Dynamically compute the number of buttons per line
		buttonPerLine = (int)((menuFrame.GetComponent<RectTransform> ().rect.width - 10f) / (buttonWidth+distanceBetweenButtons));


		// sizeDelta.x => width
		// sizeDelta.y => height

		// height and width of the menu
		menuRectT.sizeDelta = new Vector2( (((buttonWidth + distanceBetweenButtons) * buttonPerLine) - distanceBetweenButtons),
											(buttonHeight + distanceBetweenLines) * (int)((totalLevels-1)/buttonPerLine) + buttonHeight + 2* offsetTopBot) ;

		menu.transform.localPosition += menuCoordinates;



		// For each level, add a button
		for (int i = 0; i < totalLevels; i++) {
			GameObject newButton = GameObject.Instantiate (buttonLevel.buttonLevelPrefab);

			buttonLevels [i] = newButton;

			newButton.transform.SetParent(menu.transform);
			newButton.name = "Level "+(i+1);
			newButton.GetComponentInChildren<Text> ().text = "Level " + (i+1);



			newButton.transform.localPosition = new Vector3 (
													(i%buttonPerLine * buttonWidth + i%buttonPerLine * distanceBetweenButtons) + (buttonWidth/2f), 
													(-1)*(int)(i/buttonPerLine)*buttonHeight - (buttonHeight/2f) - (int)(i/buttonPerLine)*distanceBetweenLines - offsetTopBot,
													0f);



			// Color and deactivate locked level buttons
			if (i < levelInfos.unlockedLevels) {
				ColorBlock cb = newButton.GetComponent<Button> ().colors;
				cb.normalColor = Color.green;
				newButton.GetComponent<Button> ().colors = cb;
			}
			else{
				newButton.GetComponent<Button> ().interactable = false;
			}

			int index = i+1;
			newButton.GetComponent<Button>().onClick.AddListener (() => OnLevelButtonClicked (index));
		}
	}

	public void OnLevelButtonClicked (int index){
		Debug.Log ("Go to level "+index);
		gameInfos = GameObject.Find ("GameInformations");
		gameInfos.GetComponent<GameInformations> ().noLevel = index;
		GameObjectManager.loadScene("level_"+index);
	}


	// Checks if stretched size is different and adjusts buttons accordingly
	public void UpdateMenuLevel(){

		GameObject menu = menuFamily.First ();
		//GameObject gameInfos = gameInfosFamily.First ();
		GameObject menuFrame = GameObject.Find ("Menu"); // to get the menu stretched size

		RectTransform menuRectT = menu.GetComponent<RectTransform> ();

		GameInformations levelInfos = gameInfos.GetComponent<GameInformations>();
		int totalLevels = levelInfos.totalLevels;

		// Dynamically compute the number of buttons per line
		buttonPerLine = (int)((menuFrame.GetComponent<RectTransform> ().rect.width - 20f) / (buttonWidth+distanceBetweenButtons));

		// Resize the image holding the buttons
		menuRectT.sizeDelta = new Vector2( (((buttonWidth + distanceBetweenButtons) * buttonPerLine) - distanceBetweenButtons),
											(buttonHeight + distanceBetweenLines) * (int)((totalLevels-1)/buttonPerLine) + buttonHeight + 2* offsetTopBot) ;
		
		for (int i = 0; i < totalLevels; i++) {
			GameObject button = buttonLevels [i];

			button.transform.localPosition = new Vector3 (
												(i%buttonPerLine * buttonWidth + i%buttonPerLine * distanceBetweenButtons) + (buttonWidth/2f), 
												(-1)*(int)(i/buttonPerLine)*buttonHeight - (buttonHeight/2f) - (int)(i/buttonPerLine)*distanceBetweenLines - offsetTopBot,
												0f);
		}
	}
}
