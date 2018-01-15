using UnityEngine;
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

	// ==== VARIABLES ====

	private Family shipInCollision 	= FamilyManager.getFamily(new AllOfComponents(typeof(InCollision3D)));
	private Family shipFamily 		= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));

	// ==== LIFECYCLE ====
	public Collisions(){
		SystemsManager.AddFSystem (this);
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		resolveCollision ();
	}

	// ==== METHODS ====

	protected void resolveCollision(){
		GameLogic gl = (GameLogic)SystemsManager.GetFSystem("GameLogic");
		if (gl == null) {return;}

		// /!\ these 2 if can't happen at the same time (exclusive conditions) because collision always happen inside terrain /!\

		if (shipFamily.First ().GetComponent<Position> ().pos.x > 1.0f || shipFamily.First ().GetComponent<Position> ().pos.y > 1.0f
			|| shipFamily.First ().GetComponent<Position> ().pos.x < 0.0f || shipFamily.First ().GetComponent<Position> ().pos.y < 0.0f) {
			if (gl.state != GameLogic.STATES.LOST) {
				gl.OnLost ();
			}
		}
		if (shipInCollision.Count > 0 && gl.state == GameLogic.STATES.PLAYING) {
			InCollision3D col = shipInCollision.First ().GetComponent<InCollision3D> ();

			foreach (GameObject target in col.Targets) {
				// finish
				if (target.tag == "finish") {
					gl.OnWon ();
				} else if (target.tag == "obstacle") {
					gl.OnLost ();
				} else if (target.tag == "bonus") {
					GameObject levelInformations = levelInfoFamily.First ();
					LevelInformations infos = levelInformations.GetComponent<LevelInformations> ();

					Bonus b = target.GetComponent<Bonus> ();
					if (!b.isCollected) {
						infos.collectedBonus++;
						GameObjectManager.removeComponent<SphereCollider> (target);
						GameObject.Destroy(target.GetComponent<CollisionSensitive3DTarget>());
						GameObjectManager.setGameObjectState (target, false);
						b.isCollected=true;
					}
				}
				else {
					Field f = target.GetComponent<Field> ();
					// gaussian field source
					if (f != null && !f.isUniform) {
						gl.OnLost ();
					}
				}

			}
		} 
	}
}