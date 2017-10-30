using UnityEngine;
using FYFY;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 * 
 *  - Camera from http://wiki.unity3d.com/index.php?title=MouseOrbitZoom and adapted to ECS
 */

public class PlayerActions : FSystem {

	// ==== VARIABLES ====

	// === Camera
	private Family cameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(CameraParams)));
	private bool isCameraInitialized = false;
	private bool isCameraMovable	 = true;
	private float xDeg = 0.0f;
	private float yDeg = 0.0f;
	private float currentDistance;
	private float desiredDistance;
	private Quaternion currentRotation;
	private Quaternion desiredRotation;
	private Quaternion rotation;
	private Vector3 position;

	// === Object Selection
	private Family sourcesFamily 	= FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));
	private Family shipFamily 		= FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass),typeof(Charge)));
	private bool isSelectionInitialized = false;
	private bool canPlayerSelect 		= true;
	public Material selectedMaterial;
	public Material selectedAndEditableMaterial;
	public GameObject previousGameObject;
	private Material previousMaterial;

	// ==== LIFECYCLE ====
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
		if (!isCameraInitialized) {
			SystemsManager.AddFSystem (this);
			InitCamera ();
			isCameraInitialized = true;
		}

		if (!isSelectionInitialized) {
			selectedMaterial = Resources.Load("Materials/SelectedMaterial",typeof(Material)) as Material;
			selectedAndEditableMaterial = Resources.Load("Materials/SelectedAndEditableMaterial",typeof(Material)) as Material;
			previousGameObject = null;
			previousMaterial = null;
			isSelectionInitialized = true;
		}
	}

	protected override void onProcess(int familiesUpdateCount) {
		if (isCameraMovable && isCameraInitialized) { CameraUpdate (); }
		if (canPlayerSelect) { DetectMouseSelection (); }
	} 

	// ==== METHODS ====

	// === Object Selection
	protected void DetectMouseSelection(){  
		if (Input.GetMouseButtonDown (0)) {
			
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		  
			if (Physics.Raycast (ray, out hit)) {
				// put old go material back
				if (previousGameObject != null) {
					previousGameObject.GetComponent<Renderer> ().material = previousMaterial;
				}
				// change new go if it match our criteria
				GameObject go = GameObject.Find(hit.collider.name);

				bool nothingFound = isSourcesSelected(go);
				nothingFound &= isShipSelected (go);
				if (nothingFound) {
					previousMaterial = null;
					previousGameObject = null;
				}
			}
		}
	}

	protected bool isSourcesSelected(GameObject go){
		UI ui = (UI)SystemsManager.GetFSystem("UI");

		// Sources
		if (sourcesFamily.contains (go.GetInstanceID ())) {
			previousMaterial = go.GetComponent<Renderer> ().material;
			previousGameObject = go;

			if (go.GetComponent<Editable> () == null) {
				go.GetComponent<Renderer> ().material = selectedMaterial;
			} else {
				go.GetComponent<Renderer> ().material = selectedAndEditableMaterial;
			}

			ui.UpdateSourcesInformations (go);
			ui.Show (ui.sourcesInformationsPanel);
			return true;
		}

		ui.Hide (ui.sourcesInformationsPanel);
		return false;
	}

	protected bool isShipSelected(GameObject go){
		UI ui = (UI)SystemsManager.GetFSystem("UI");
		// Ship
		if (shipFamily.contains (go.GetInstanceID ())) {
			previousMaterial = go.GetComponent<Renderer> ().material;
			previousGameObject = go;

			if (go.GetComponent<Editable> () == null) {
				go.GetComponent<Renderer> ().material = selectedMaterial;
			} else {
				go.GetComponent<Renderer> ().material = selectedAndEditableMaterial;
			}

			ui.Show (ui.shipSpeedPanel);
			return true;
		} 

		ui.Hide (ui.shipSpeedPanel);
		return false;
	}

	// === Camera
	protected void InitCamera()
	{
		// Get data according to ECS
		GameObject camera = cameraFamily.First ();
		CameraParams cameraParams = camera.GetComponent<CameraParams> ();
		Transform target = cameraParams.target;
		Transform transform = camera.transform;
		float distance = cameraParams.distance;

		//If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
		if (!target)
		{
			GameObject go = new GameObject("Cam Target");
			go.transform.position = transform.position + (transform.forward * distance);
			target = go.transform;
			cameraParams.target = target;
		}

		distance = Vector3.Distance(transform.position, target.position);
		currentDistance = distance;
		desiredDistance = distance;

		//be sure to grab the current rotations as starting points.
		position = transform.position;
		rotation = transform.rotation;
		currentRotation = transform.rotation;
		desiredRotation = transform.rotation;

		xDeg = Vector3.Angle(Vector3.right, transform.right );
		yDeg = Vector3.Angle(Vector3.up, transform.up );
	}

	protected void CameraUpdate()
	{
		// Get data according to ECS
		GameObject camera = cameraFamily.First ();
		CameraParams cameraParams = camera.GetComponent<CameraParams> ();
		Transform target = cameraParams.target;
		Transform transform = camera.transform;
		float zoomRate = cameraParams.zoomRate;
		float xSpeed = cameraParams.xSpeed;
		float ySpeed = cameraParams.ySpeed;
		float yMinLimit = cameraParams.yMinLimit;
		float yMaxLimit = cameraParams.yMaxLimit;
		float zoomDampening = cameraParams.zoomDampening;
		float panSpeed = cameraParams.panSpeed;
		float keyBoardPanSpeed = cameraParams.keyBoardPanSpeed;
		Vector3 targetOffset = cameraParams.targetOffset;
		float minDistance = cameraParams.minDistance;
		float maxDistance = cameraParams.maxDistance;


		// If right mouse is selected? ORBIT
		if (Input.GetMouseButton(1))
		{
			xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			////////OrbitAngle

			//Clamp the vertical axis for the orbit
			yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
			// set camera rotation 
			desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
			currentRotation = transform.rotation;

			rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
			transform.rotation = rotation;
		}
		// otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
		else if (Input.GetMouseButton(2))
		{
			//grab the rotation of the camera so we can move in a pseudo local XY space
			target.rotation = transform.rotation;
			target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
			target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
		}

		// we can also pan with Z-S/Q-D keys
		if (Input.GetKey ("z")) {
			target.rotation = transform.rotation;
			target.Translate (Vector3.forward * keyBoardPanSpeed);
		}
		if (Input.GetKey ("s")) {
			target.rotation = transform.rotation;
			target.Translate (Vector3.back *  keyBoardPanSpeed);
		}
		if (Input.GetKey ("q")) {
			target.rotation = transform.rotation;
			target.Translate (Vector3.left *  keyBoardPanSpeed);
		}
		if (Input.GetKey ("d")) {
			target.rotation = transform.rotation;
			target.Translate (Vector3.right *  keyBoardPanSpeed);
		}

		////////Orbit Position

		// affect the desired Zoom distance if we roll the scrollwheel
		desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
		//clamp the zoom min/max
		desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
		// For smoothing of the zoom, lerp distance
		currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

		// calculate position based on the new currentDistance 
		position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
		transform.position = position;
	}

	protected static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}
}