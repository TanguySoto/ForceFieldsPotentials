using UnityEngine;
using System.Collections;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class CameraParams : MonoBehaviour {
	
	public Transform target;
	public Vector3 targetOffset;
	public float distance = 5.0f;
	public float maxDistance = 20;
	public float minDistance = .6f;
	public float xSpeed = 200.0f;
	public float ySpeed = 200.0f;
	public int yMinLimit = -80;
	public int yMaxLimit = 80;
	public int zoomRate = 50;
	public float panSpeed = 0.4f;
	public float keyBoardPanSpeed = 0.15f;
	public float zoomDampening = 7.0f;

}
