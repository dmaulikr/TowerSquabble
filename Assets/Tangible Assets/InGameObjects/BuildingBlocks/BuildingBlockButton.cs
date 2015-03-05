using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildingBlockButton : MonoBehaviour {
	// Current type of the button
	private ParseBuildingBlock.Type currentBuildingBlockType = ParseBuildingBlock.Type.none;
	// Main camera
	public Camera mainCamera;
	// The Y position offset between building block and finger pos
	public float buildingBlockYPosOffset;
	// The building block instance
	private GameObject buildingBlockInstance = null;
	// Index of this button
	public int buttonIndex = 0;
	// Image for sprite
	public Image spriteImage;
	// Is the spin animation in progress?
	private bool isSpinAnimationInProgress = false;
	// Are we suppose to stop spinning the button?
	private bool stopSpinning = false;
	// Are we currently spinning?
	private bool isSpinning = false;
	// Implementaion detail, calculated target for spin
	private float calculatedSpinTarget = 0f;
	// Varible telling us if a user is currently interacting with the button
	public bool isUserInteractingWithButton = false;

	// Refrence to the level manager
	private MatchManager matchManager;
	// Reference to the building block slot machine
	private BuildingBlockSlotMachine buildingBlockSlotMachine;

	// Accessors
	public ParseBuildingBlock.Type CurrentBuildingBlockType { get { return currentBuildingBlockType; } set { currentBuildingBlockType = value; }}

	public void ResetTiledImagePosition()
	{
		Vector3 rt = spriteImage.rectTransform.anchoredPosition;
		rt.y = ((spriteImage.rectTransform.rect.height * spriteImage.rectTransform.localScale.x) / 2f) - (spriteImage.rectTransform.rect.width * spriteImage.rectTransform.localScale.x / 2f);
		spriteImage.rectTransform.anchoredPosition = rt;
	}

	public void Spin()
	{
		if(!stopSpinning)
		{
			Vector3 rt = spriteImage.rectTransform.anchoredPosition;
			rt.y -= 2000f * Time.deltaTime;
			spriteImage.rectTransform.anchoredPosition = rt;
		}
		else if(isSpinning)
		{
			// Spin slower
			Vector3 rt = spriteImage.rectTransform.anchoredPosition;
			rt.y -= 1500f * Time.deltaTime;
			// Have we reached the target?
			// Note: calculatedSpinTarget is calculated when stopSpinning is set to true
			if(rt.y <= calculatedSpinTarget)
			{
				// We are done spinning!
				rt.y = calculatedSpinTarget;
				isSpinning = false;
			}
			spriteImage.rectTransform.anchoredPosition = rt;
		}
	}

	public bool IsSpinning()
	{
		return isSpinning;
	}

	public void SetIsSpinning(bool value)
	{
		isSpinning = value;
	}

	public void SetStopSpinning(bool value)
	{
		stopSpinning = value;
	}

	public void SetIsSpinAnimationInProgress(bool value)
	{
		isSpinAnimationInProgress = value;
	}

	public bool IsHoldingTool()
	{
		return buildingBlockInstance != null ? true : false;
	}

	public void SetInteractable(bool interactable)
	{
		gameObject.GetComponent<Button>().interactable = interactable;
		// Set image visible/invisible
		spriteImage.enabled = interactable;
	}

	public void OnPointerDown()
	{
		isUserInteractingWithButton = true;
		if(buildingBlockInstance != null || matchManager.gameOver || !gameObject.GetComponent<Button>().IsInteractable() || isSpinAnimationInProgress)
		{
			return; // Do nothing
		}
		// Instantiate building block
		Vector3 fingerPos; 
		if(Input.touchCount > 0)
		{
			fingerPos = Input.GetTouch(0).position; // Get touch position
		}
		else
		{
			fingerPos = Input.mousePosition;
		}
		Vector3 relativePos = mainCamera.GetComponent<Camera>().ScreenToWorldPoint(fingerPos); // Get position relative to camera position											 	// Reset z position
		relativePos.z = -1;
		relativePos.y += buildingBlockYPosOffset;
		// Try to find building block prefab
		GameObject prefab = buildingBlockSlotMachine.GetBuildingBlockPrefab(currentBuildingBlockType);
		// Did we find it?
		if(prefab != null)
		{
			buildingBlockInstance = Instantiate(prefab, relativePos, Quaternion.Euler(new Vector3(0,0,0))) as GameObject;
			// Set building block type
			buildingBlockInstance.GetComponentsInChildren<BuildingBlock>()[0].buildingBlockType = currentBuildingBlockType;
		}
	}

	public void Drag()
	{
		// Is there an instance of a building block?
		if(buildingBlockInstance != null)
		{
			// Set position
			// Instantiate building block
			Vector3 fingerPos; 
			if(Input.touchCount > 0)
			{
				fingerPos = Input.GetTouch(0).position; // Get touch position
			}
			else
			{
				fingerPos = Input.mousePosition;
			}
			Vector3 relativePos = mainCamera.GetComponent<Camera>().ScreenToWorldPoint(fingerPos); // Get position relative to camera position											 	// Reset z position
			relativePos.z = -1;
			relativePos.y += buildingBlockYPosOffset;
			buildingBlockInstance.transform.position = relativePos;

			// OPEN POINT
			// Is there anything overlapping the building block?
			// Collider2D[] Collider2DObjects = Physics2D.OverlapPointAll(buildingBlockInstance.transform.position);
		}
	}

	public void OnPointerUp()
	{
		isUserInteractingWithButton = false;

		if(buildingBlockInstance != null)
		{
			// Check if we are allowed to release building block
			if(buildingBlockInstance.GetComponentsInChildren<BuildingBlock>()[0].NotReleasedAndTouchingCollider)
			{
				// Not allowed to release building block here
				Destroy(buildingBlockInstance);
				return;
			}

			// Handle building block index
			int buildingBlockIndex = matchManager.GetNextBuildingBlockIndex();
			buildingBlockInstance.GetComponentsInChildren<BuildingBlock>()[0].index = buildingBlockIndex;
			// Release building block
			buildingBlockInstance.GetComponentsInChildren<BuildingBlock>()[0].SetIsReleased(true);
			buildingBlockInstance = null;
			// Let the building block list know
			buildingBlockSlotMachine.BuildingBlockReleased(buttonIndex);
			SetInteractable(false);
		}
	}

	public void OnPointerClick()
	{
		if(buildingBlockInstance != null)
		{
			// We released the building block on the button, do not release it
			buildingBlockInstance.GetComponentsInChildren<BuildingBlock>()[0].DestroyToolObject();
		}
	}

	// Implementation detail for spinning animation
	public void CalculateOffsetToNextCurrentType(float currentYPos)
	{
		// Get current type as int
		int currentType = Convert.ToInt32 (currentBuildingBlockType);
		// Size of one "tilling" of the sprite
		float sizeOneUnit = spriteImage.rectTransform.rect.width * spriteImage.rectTransform.localScale.x * buildingBlockSlotMachine.listOfBuildingBlocks.Length;
		// Start value based on type
		float typeOffset = ((spriteImage.rectTransform.rect.height * spriteImage.rectTransform.localScale.x) / 2f) - (spriteImage.rectTransform.rect.width * spriteImage.rectTransform.localScale.x / 2f) - (currentType * spriteImage.rectTransform.rect.width * spriteImage.rectTransform.localScale.x);
		// Distance between typeOffset and current y pos
		float relativeYPos = typeOffset - currentYPos;
		// Units we need to travel (one unit is the size of one "tilling" of the sprite)
		int units = Mathf.FloorToInt((relativeYPos + sizeOneUnit) / sizeOneUnit);
		// The offset between current y pos and units we need to travel
		float offsetFromCurrentValue = (units * sizeOneUnit) - relativeYPos;
		calculatedSpinTarget = currentYPos - offsetFromCurrentValue;
	}

	void Awake()
	{
		SetInteractable(false);
	}

	void Start()
	{
		matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
		buildingBlockSlotMachine = GameObject.Find("BuildingBlockSlotMachine").GetComponent<BuildingBlockSlotMachine>();
		ResetTiledImagePosition();
	}
	
	void Update()
	{
		// Make button not interactable?
		if(matchManager.gameOver && gameObject.GetComponent<Button>().IsInteractable())
		{
			SetInteractable(false);
		}
	}
}
