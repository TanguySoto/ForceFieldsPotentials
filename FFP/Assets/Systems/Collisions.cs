﻿using UnityEngine;
using FYFY;
using FYFY_plugins.CollisionManager;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Collisions : FSystem {

	Family shipInCollision = FamilyManager.getFamily(new AllOfComponents(typeof(InCollision3D)));

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		resolveCollision ();
	}

	protected void resolveCollision(){
		if (shipInCollision.Count > 0 && GameLogic.state == GameLogic.STATES.PLAYING) {
			GameObject ship = shipInCollision.First ();
			InCollision3D col = ship.GetComponent<InCollision3D> ();

			foreach (GameObject target in col.Targets) {
				if (target.tag == "finish") {
					Debug.Log ("GG");
					GameLogic.state = GameLogic.STATES.WON;
				}
			}
		}
	}
}