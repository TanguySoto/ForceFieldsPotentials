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

public class Credits : FSystem {

	public Button menuButton;

	public Credits(){
		InitCredits ();
	}


	protected void InitCredits(){
		menuButton = GameObject.Find ("MenuButton").GetComponent<Button> ();
		menuButton.onClick.AddListener (() => OnMenuButtonClicked ());
	}

	protected void OnMenuButtonClicked(){
		GameObjectManager.loadScene("MenuScene");
	}

}
