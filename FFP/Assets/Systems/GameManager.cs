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

public class GameManager : FSystem {

	private Family menuFamily = FamilyManager.getFamily (new AllOfComponents(typeof(Game)));

	private bool isMenuInit = false;
	private GameObject[] buttonLevels;

	public int buttonPerLine = 4;
	public float distanceBetweenLines = 100f;
	public float distanceBetweenButtons = 100f;
	public float offsetTopBot = 50f;


	public Vector3 menuCoordinates = new Vector3(20f, 0f, 0f);








	// ==== LIFECYCLE ====

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isMenuInit) {
			InitMenu ();
			isMenuInit = true;
		}
	}



	protected override void onProcess(int familiesUpdateCount) {
		//UpdateMenu ();
	}












	// ==== METHODS ====
	public void InitMenu(){
		GameObject menu = menuFamily.First ();
		Game infolevels = menu.GetComponent<Game>();
		int totalLevels = infolevels.totalLevels;

		buttonLevels = new GameObject[totalLevels];

		RectTransform menuRectT = menu.GetComponent<RectTransform> ();


		// width and height of the level buttons
		float buttonWidth = infolevels.buttonLevelPrefab.GetComponent<RectTransform>().sizeDelta.x + 15f; // Size not right between prefab and instantiated object
		float buttonHeight = infolevels.buttonLevelPrefab.GetComponent<RectTransform>().sizeDelta.y + 15f;


		// TODO : Dynamically compute the number of button per line
		//buttonPerLine = (int)((menu.GetComponentInParent<RectTransform> ().sizeDelta.x - 10f) / (buttonWidth+distanceBetweenButtons));


		// sizeDelta.x => width
		// sizeDelta.y => height

		// height and width of the menu
		menuRectT.sizeDelta = new Vector2( (((buttonWidth + distanceBetweenButtons) * buttonPerLine) - distanceBetweenButtons),
											(buttonHeight + distanceBetweenLines) * (int)((totalLevels-1)/buttonPerLine) + buttonHeight + 2* offsetTopBot) ;

		menu.transform.localPosition += menuCoordinates;




		// For each level, add a button
		for (int i = 0; i < totalLevels; i++) {
			GameObject newButton = GameObject.Instantiate (infolevels.buttonLevelPrefab);

			buttonLevels [i] = newButton;

			newButton.transform.SetParent(menu.transform);
			newButton.name = "Level "+(i+1);
			newButton.GetComponentInChildren<Text> ().text = "Level " + (i+1);



			newButton.transform.localPosition = new Vector3 (
													(i%buttonPerLine * buttonWidth + i%buttonPerLine * distanceBetweenButtons) + (buttonWidth/2f), 
													(-1)*(int)(i/buttonPerLine)*buttonHeight - (buttonHeight/2f) - (int)(i/buttonPerLine)*distanceBetweenLines - offsetTopBot,
													0f);



			// Color and deactivate locked level buttons
			if (i < infolevels.unlockedLevels) {
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
		//GameObjectManager.loadScene ("scene_test"); // Switch the name for "scene_index" or something like that
		GameObjectManager.loadScene("level_"+index);
	}






	// Check if new levels are unlocked -- unnessary actually? Initialization should be enough
	/*public void UpdateMenu(){
	
	}*/

}
