using UnityEngine;
using UnityEngine.EventSystems;
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
	private bool canPlayerSelect 	= true;
	public Material selectedMaterial;
	public Material selectedAndEditableMaterial;
	public GameObject previousGameObject;
	public Material previousMaterial;

	// === Object drag and drob
	private Family PPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private bool isMouseDrag = false;
	private Vector3 screenPosition;
	private Vector3 offset;

	// ==== LIFECYCLE ====

	public PlayerActions(){
		SystemsManager.AddFSystem (this);
		InitCamera ();
		InitSelection ();
	}
	
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
		if (isCameraMovable) { CameraUpdate (); }
		if (canPlayerSelect) { DetectMouseSelection (); }
	} 

	// ==== METHODS ====
	protected void InitSelection(){
		selectedMaterial = Resources.Load("Materials/SelectedMaterial",typeof(Material)) as Material;
		selectedAndEditableMaterial = Resources.Load("Materials/SelectedAndEditableMaterial",typeof(Material)) as Material;
		previousGameObject = null;
		previousMaterial = null;
	}

	// === Object Selection and Drag & drop
	protected void DetectMouseSelection(){  
		if (Input.GetMouseButtonDown (0)) {

			// try selecting no object
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// touched something and was not UI
			if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject()) {
				// unselect previous object
				if (previousGameObject != null) {
					previousGameObject.GetComponent<Renderer> ().material = previousMaterial;
					previousMaterial = null;
					previousGameObject = null;
				}
				
				// select now object if it match our criteria
				GameObject go = GameObject.Find (hit.collider.name);
				isSourceSelected (go);
				isShipSelected (go);
				isTerrinSelected (go,hit);
			}
		}

		if (Input.GetMouseButtonUp(0)){
			isMouseDrag = false;
		}

		if (isMouseDrag){
			//track mouse position.
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			//convert screen position to world position with offset changes.
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;

			//It will update target gameobject's current postion.
			Terrain terr = PPlanFamily.First ().GetComponent<Terrain>();
			TerrainData td = terr.terrainData;
			Vector3 relativeNewPos = new Vector3 (currentPosition.x / td.size.x, currentPosition.z / td.size.z, previousGameObject.GetComponent<Position> ().pos.z);
			if (relativeNewPos.x <= 1 && relativeNewPos.x >= 0 && relativeNewPos.y <= 1 && relativeNewPos.y >= 0) {
				previousGameObject.transform.position = new Vector3(currentPosition.x,previousGameObject.transform.position.y,currentPosition.z);
				previousGameObject.GetComponent<Position> ().pos = relativeNewPos;

				// Refresh forces
				ForcesDisplay fd = (ForcesDisplay)SystemsManager.GetFSystem ("ForcesDisplay");
				fd.refresh ();

				// Refresh UI
				UI ui = (UI)SystemsManager.GetFSystem ("UI");
				if (previousGameObject.GetComponent<Field> ().isUniform) {
					ui.UpdateUniSourcesInformations (previousGameObject);
				} else {
					ui.UpdateSourcesInformations (previousGameObject);
				}
			}
		}	
	}

	protected bool isSourceSelected(GameObject go){
		UI ui = (UI)SystemsManager.GetFSystem("UI");

		// Source found
		if (sourcesFamily.contains (go.GetInstanceID ())) {
			previousMaterial = go.GetComponent<Renderer> ().material;
			previousGameObject = go;

			// Still editable ?
			if (go.GetComponent<Editable> () == null) {
				go.GetComponent<Renderer> ().material = selectedMaterial;
			} else {
				go.GetComponent<Renderer> ().material = selectedAndEditableMaterial;
				// prepare dragging
				isMouseDrag = true;
				screenPosition = Camera.main.WorldToScreenPoint(go.transform.position);
				offset = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
			}

			if (go.GetComponent<Field> ().isUniform) {
				ui.UpdateUniSourcesInformations (go);
				ui.Show (ui.uniSourcesInformationsPanel);
				ui.Hide (ui.sourcesInformationsPanel);
			} else {
				ui.UpdateSourcesInformations (go);
				ui.Show (ui.sourcesInformationsPanel);
				ui.Hide (ui.uniSourcesInformationsPanel);
			}

			ui.deleteButton.interactable = go.GetComponent<Editable>()!=null;
			return true;
		}
			
		ui.Hide (ui.sourcesInformationsPanel);
		ui.Hide (ui.uniSourcesInformationsPanel);
		ui.deleteButton.interactable = false;
		return false;
	}

	protected bool isShipSelected(GameObject go){
		UI ui = (UI)SystemsManager.GetFSystem("UI");

		// Ship found
		if (shipFamily.contains (go.GetInstanceID ())) {
			previousMaterial = go.GetComponent<Renderer> ().material;
			previousGameObject = go;

			// Still editable ?
			if (go.GetComponent<Editable> () == null) {
				go.GetComponent<Renderer> ().material = selectedMaterial;
			} else {
				go.GetComponent<Renderer> ().material = selectedAndEditableMaterial;
			}

			ui.UpdateShipInformations (go);
			ui.Show (ui.shipSpeedPanel);
			return true;
		} 

		ui.Hide (ui.shipSpeedPanel);
		return false;
	}

	protected bool isTerrinSelected(GameObject go, RaycastHit hit){
		UI ui = (UI)SystemsManager.GetFSystem("UI");

		if(PPlanFamily.contains(go.GetInstanceID())){
			ui.UpdatePointInformations (hit.point);
			ui.Show (ui.pointPanel);

			return true;
		}

		ui.Hide (ui.pointPanel);
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