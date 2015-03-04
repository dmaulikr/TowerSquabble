using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {
	
	public float speed = 0.1F;
	public float damp = 0.05F;
	private float xInputLastUpdate = 0.0F;
	private float yInputLastUpdate = 0.0F;

	public float borderEastXFromCenter;
	public float borderWestXFromCenter;
	public float borderNorthYFromCenter;
	public float borderSouthYFromCenter;

	// Reference to the background sprite
	public GameObject backgroundSprite;
	public float backgroundRatioSpeed = 1.0f;

	// Reference to the level manager
	private MatchManager matchManager;
	// Reference to the building block button
	public BuildingBlockButton[] buildingBlockButtons;

	// Mouse position varibles
	private Vector2 currentMousePositon;
	private Vector2 lastMousePosition;

	// Did we receive input last frame?
	private bool receivedInputLastFrame = false;

	void Start()
	{
		matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
	}
	
	/* SetTransformX
	 */ 
	void SetTransformX(float value)
	{	
		transform.position = new Vector3(value, transform.position.y, transform.position.z);
		// Set transform of background
		backgroundSprite.transform.position = new Vector3(backgroundSprite.transform.position.x, value*backgroundRatioSpeed, backgroundSprite.transform.position.z);
	}
	
	/* SetTransformY
	 */
	void SetTransformY(float value)
	{	
		transform.position = new Vector3(transform.position.x, value, transform.position.z);
		// Set transform of background
		backgroundSprite.transform.position = new Vector3(value*backgroundRatioSpeed, backgroundSprite.transform.position.y, backgroundSprite.transform.position.z);
	}
	
	/* MoveCamera
	 */
	void MoveCamera(float inputDeltaX, float inputDeltaY)
	{
		// Camera
		transform.Translate(inputDeltaX * speed, 
		                    inputDeltaY * speed, 0);
		// Background
		backgroundSprite.transform.Translate(inputDeltaX * speed * backgroundRatioSpeed, 
		                    inputDeltaY * speed * backgroundRatioSpeed, 0);
	}
	
	/* AreWeWithinXBorders
	 */
	bool AreWeWithinXBorders()
	{
		return transform.position.x <= borderEastXFromCenter && transform.position.x >= borderWestXFromCenter;
	}
	
	/* AreWeWithinYBorders
	 */
	bool AreWeWithinYBorders()
	{
		return transform.position.y <= borderNorthYFromCenter && transform.position.y >= borderSouthYFromCenter;
	}
	
	/* IsCameraWithinBorders
	 */
	bool IsCameraWithinBorders()
	{
		return AreWeWithinXBorders() && AreWeWithinYBorders();
	}
	
	/* Update is called once per frame
	 */ 
	void LateUpdate() 
	{	
		// Is user currently holding tool? (bbb)
		bool isHoldingTool = false;
		// Did the user interact with any GUI component this frame?
		bool isUserInteractingWithGUI = matchManager.isUserInteractingWithGUI;
		foreach(BuildingBlockButton bbb in buildingBlockButtons)
		{
			if(bbb.IsHoldingTool())
			{
				isHoldingTool = true;
			}
			// Did user interact with any of the building block buttons this frame?
			if(bbb.isUserInteractingWithButton)
			{
				isUserInteractingWithGUI = true;
			}
		}

		// Check for user input
		// NOTE: Input.GetMouseButton(0 is true for touch input as well
		if(!isHoldingTool /* && !GamePaused*/ && !isUserInteractingWithGUI &&
		   (Input.touchCount == 1 || (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.Space) && Input.touchCount != 2))) 
		{
			Vector2 inputDeltaPosition;
			if(Input.touchCount > 0) // Touch input?
			{
				inputDeltaPosition = -Input.GetTouch(0).deltaPosition;
			}
			else // Mouse input
			{
				currentMousePositon = Input.mousePosition;
				// This check is here so that first mouse press does not make the camera jump to a new position
				if(receivedInputLastFrame)
				{
					inputDeltaPosition = currentMousePositon - lastMousePosition;
				}
				else
				{
					inputDeltaPosition = Vector2.zero;
				}
				lastMousePosition = currentMousePositon;
			}
			// Movement along the x-axis
			// If at the left border, do not apply negative touchDeltaPosition.x
			// If at the right border, do not apply positive touchDeltaPosition.x
			if(!((transform.position.x <= borderWestXFromCenter && inputDeltaPosition.x < 0)
			     || (transform.position.x >= borderEastXFromCenter && inputDeltaPosition.x > 0)))
			{
				if((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
				   || Input.touchCount <= 0) // Movement
				{
					MoveCamera(inputDeltaPosition.x, 0F);
					xInputLastUpdate = inputDeltaPosition.x;
				}
			}
			
			// Movement along the y-axis
			// If at the bottom border, do not apply negative touchDeltaPosition.x
			// If at the top border, do not apply positive touchDeltaPosition.x
			if(!((transform.position.y <= borderSouthYFromCenter && inputDeltaPosition.y < 0)
			     || (transform.position.y >= borderNorthYFromCenter && inputDeltaPosition.y > 0)))
			{
				if((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
				   || Input.touchCount <= 0) // Movement
				{
					MoveCamera(0F, inputDeltaPosition.y);
					yInputLastUpdate = inputDeltaPosition.y;
				}
			}
			receivedInputLastFrame = true;
		}
		else
		{
			receivedInputLastFrame = false;
		}

		// Are we suppose to "glide"
		if(!isHoldingTool /*&& !GameOver && !GamePaused*/)
		{
			// Gliding X
			if(Mathf.Abs(xInputLastUpdate) > damp)
			{
				bool bNegativeInput;
				if(xInputLastUpdate > 0.0F)
				{
					xInputLastUpdate -= damp;
					bNegativeInput = false;
				}
				else
				{
					xInputLastUpdate += damp;
					bNegativeInput = true;
				}
				if((xInputLastUpdate > 0.0F && !bNegativeInput)
				   || (xInputLastUpdate < 0.0F && bNegativeInput))
				{
					MoveCamera(xInputLastUpdate, 0F);
				}
			}
			// Gliding Y
			if(Mathf.Abs(yInputLastUpdate) > damp)
			{
				bool bNegativeInput;
				if(yInputLastUpdate > 0.0F)
				{
					yInputLastUpdate -= damp;
					bNegativeInput = false;
				}
				else
				{
					yInputLastUpdate += damp;
					bNegativeInput = true;
				}
				if((yInputLastUpdate > 0.0F && !bNegativeInput)
				   || (yInputLastUpdate < 0.0F && bNegativeInput))
				{
					MoveCamera(0F, yInputLastUpdate);
				}
			}
		}
		
		// Clamp camera to bounds
		transform.position = new Vector3(
			Mathf.Clamp(transform.position.x, borderWestXFromCenter, borderEastXFromCenter), 
			Mathf.Clamp(transform.position.y, borderSouthYFromCenter, borderNorthYFromCenter),
			transform.position.z);
		// Clamp background to bounds
		backgroundSprite.transform.position = new Vector3(
			Mathf.Clamp(backgroundSprite.transform.position.x, borderWestXFromCenter*backgroundRatioSpeed, borderEastXFromCenter*backgroundRatioSpeed), 
			Mathf.Clamp(backgroundSprite.transform.position.y, borderSouthYFromCenter*backgroundRatioSpeed, borderNorthYFromCenter*backgroundRatioSpeed), 
			backgroundSprite.transform.position.z);
	}
}
