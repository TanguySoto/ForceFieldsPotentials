using UnityEngine;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Movement : MonoBehaviour {

	public float x 	= 0; 	// In meters (m), axe x
	public float y 	= 0; 	// In meters (m), axe y
	public float z 	= 0; 	// In meters (m), axe z

	public Vector3 speed 		= new Vector3(0,0,0); 	// In meters/seconds (m/s)
	public Vector3 acceleration = new Vector3(0,0,0); 	// In meters/seconds (m/s)

}