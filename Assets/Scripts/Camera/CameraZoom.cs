using UnityEngine; 

public class CameraZoom : MonoBehaviour 
{ 
	// Reference to cameraRef
	private Camera cameraRef; 
	
	public float speed = 4; 
	public float minFieldOfView = 8.0F; 
	public float maxFieldOfView = 11.0F; 
	public float minZoomSpeed = 0.01F; 
	
	// Varibles used for mouse input
	private Vector2 previousMousePos = Vector2.zero;
	
	// Use this for initialization 
	void Start ()
	{
		cameraRef = gameObject.GetComponent<Camera>();
	}
	
	// Update is called once per frame 
	void Update () 
	{
		// Zoom for touch input
		if(Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			// Get current distance between input touches
			Vector2 currentDist = Input.GetTouch(0).position - Input.GetTouch(1).position;
			// Get distance between input touches previous frame
			Vector2 previousDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); 
			// Calculate touch delta
			float touchDelta = currentDist.magnitude - previousDist.magnitude;
			// Calculate speed of each finger
			float speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude * Time.deltaTime;
			float speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude * Time.deltaTime;

			// Inward pinch gesture
			if ((touchDelta < 0) && (speedTouch0 >= minZoomSpeed) && (speedTouch1 >= minZoomSpeed))
			{
				cameraRef.orthographicSize = Mathf.Clamp(cameraRef.orthographicSize + (speed * Time.deltaTime), minFieldOfView, maxFieldOfView);
			}
			// Outward pinch gesture
			else if ((touchDelta > 0) && (speedTouch0 >= minZoomSpeed) && (speedTouch1 >= minZoomSpeed))
			{
				cameraRef.orthographicSize = Mathf.Clamp(cameraRef.orthographicSize - (speed * Time.deltaTime), minFieldOfView, maxFieldOfView);
			}
		}
		// Zoom for mouse input
		else if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.Space))
		{
			// Get current distance between input touches
			Vector2 currentMousePos = Input.mousePosition;
			if(previousMousePos != Vector2.zero)
			{
				float xMovement = currentMousePos.x - previousMousePos.x;
				float mouseSpeed = Mathf.Abs(xMovement * Time.deltaTime);
				if((xMovement < 0) && (mouseSpeed >= minZoomSpeed))
				{
					cameraRef.orthographicSize = Mathf.Clamp(cameraRef.orthographicSize + (speed * Time.deltaTime), minFieldOfView, maxFieldOfView);
				}
				else if((xMovement > 0) && (mouseSpeed >= minZoomSpeed))
				{
					cameraRef.orthographicSize = Mathf.Clamp(cameraRef.orthographicSize - (speed * Time.deltaTime), minFieldOfView, maxFieldOfView);
				}
			}
			
			// Save this frame's mouse position
			previousMousePos = currentMousePos;
		}
		else
		{
			previousMousePos = Vector2.zero;
		}
	}
}