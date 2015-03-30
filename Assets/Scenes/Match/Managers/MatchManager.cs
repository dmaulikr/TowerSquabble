using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Parse;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//using UnityEditor; // for debug

public class MatchManager : MonoBehaviour {
	// Are we debugging physics 2D?
	public bool debugPhysics2D = false;
	// Switch user button
	public Button switchUserButton;
	// Is the scene refreshing?
	public Image refreshImage;
	public bool refreshing = false;
	// Is the game over?
	public bool gameOver = false;
	// Building block prefabs
	public GameObject[] buildingBlocks;
	// Refrence to the building block slot machine
	public BuildingBlockSlotMachine buildingBlockSlotMachine;
	// Refrence to the result from parse
	// NOTE: Only set when blocksRetrievedFromParse = true
	IEnumerable<ParseBuildingBlock> result;
	private bool blocksRetrievedFromParse = false;
	public bool dataSubmittedToParse = false; // Was data submitted to Parse this round? (OPEN POINT: Make public)
	// Reference to header text (OPEN POINT: Move?)
	public Text headerText;
	// Has match ended?
	private bool hasMatchEnded = false;
	// An index added to a building block that keeps track of in what order the buildingblock was put in the scene
	public int nextBuildingBlockIndex = 0;

	// Timer varibles
	private float swayTimerStarted = 0f;
	public float swayMaxWaitTime = 5f;
	private bool hasStartSwayTimerStarted = false;

	// Is user interacting with GUI?
	public bool isUserInteractingWithGUI = false;

	// Has first building block collided with another building block?
	private bool hasBuildingBlockCollided = false;

	// Add stuff here that is going to be initalized/reseted
	// when scene is loaded or when it is the logged in player's turn again
	private void ResetSceneVaribles()
	{
		// Init building block collided flag
		hasBuildingBlockCollided = false;
		// Init timer flag
		hasStartSwayTimerStarted = false;
		// Init game over varible
		gameOver = false;
		// Init has match ended varible
		hasMatchEnded = false;
		// Init "data submitted to Parse" varible
		dataSubmittedToParse = false;
		// Init building block index
		nextBuildingBlockIndex = 0;
		blocksRetrievedFromParse = false;
		refreshing = true;
	}

	// Get the next index for a building block
	// This function will also increase the varible keeping track of next building block index
	public int GetNextBuildingBlockIndex()
	{
		nextBuildingBlockIndex++;
		return nextBuildingBlockIndex - 1;
	}

	// Get number of building blocks in the scene
	public static int GetNumberOfBuildingBlocks()
	{
		return GameObject.FindGameObjectsWithTag("BuildingBlock").Length;
	}

	private void StartSwayTimer()
	{
		swayTimerStarted = Time.time;
		hasStartSwayTimerStarted = true;
	}

	// Are all building blocks in the scene still
	public bool AreAllBuildingBlocksStill()
	{
		bool areAllBuildingBlocksStill = true;
		// Get all building blocks in the scene
		GameObject[] buildingBlocks = GameObject.FindGameObjectsWithTag("BuildingBlock");
		// Loop through the rigidbodies of all the building blocks and check their velocity
		foreach(GameObject g in buildingBlocks)
		{
			if(!MathUtilities.IsApproximately(g.GetComponent<Rigidbody2D>().velocity.x, 0.0f, 0.001f) || 
			   !MathUtilities.IsApproximately(g.GetComponent<Rigidbody2D>().velocity.y, 0.0f, 0.001f))
			{
				areAllBuildingBlocksStill = false;
				// Check if sway timer has passed max time
				if(Time.time > (swayTimerStarted + swayMaxWaitTime)) 
				{
					// Check velocity: if large, do not set it to 0
					if((g.GetComponent<Rigidbody2D>().velocity.x > 0.1f || g.GetComponent<Rigidbody2D>().velocity.x < -0.1f) ||
					   (g.GetComponent<Rigidbody2D>().velocity.y > 0.1f || g.GetComponent<Rigidbody2D>().velocity.y < -0.1f))
					{
						continue; // Skip this building block
					}
					// Set velocity to 0 for this building block
					g.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
					g.GetComponent<Rigidbody2D>().angularVelocity = 0f;
					g.GetComponent<Rigidbody2D>().Sleep();
				}
			}
			else 
			{
				// Set building blocks that are approximatley still to sleep
				g.GetComponent<Rigidbody2D>().Sleep();
			}
		}

		// Return false if timer has not been on for at least 3 sec
		if((Time.time <= (swayTimerStarted + 3) && hasStartSwayTimerStarted)
		   || !hasStartSwayTimerStarted) 
		{
			return false;
		}

		return areAllBuildingBlocksStill;
	}

	//Query the blocks that are referenced in the match's array blocks
	IEnumerator GetBlocksFromParse()
	{
		ParseQuery<ParseBuildingBlock> query = new ParseQuery<ParseBuildingBlock>().WhereEqualTo("matchId", AppModel.currentMatch.ObjectId).OrderBy("index");
		var find = query.FindAsync ();
		while (!find.IsCompleted) yield return null;
		if (find != null && !find.IsCanceled && !find.IsFaulted && find.Result != null) {
			refreshing = false;
			refreshImage.transform.rotation = Quaternion.identity;
			result = find.Result;
			AppModel.currentBlocks = result;
			blocksRetrievedFromParse = true;
		}
	}

	// Instantiate building blocks using varible "result"
	// It is up to the caller to check so that result is valid
	IEnumerator InstantiateBlocks()
	{
		// Safety check!
		if(result != null)
		{
			Time.timeScale = 30f;
			// Check whos turn it is
			ParseUser playerTurn = AppModel.currentMatch["playerTurn"] as ParseUser;
			if(!playerTurn.ObjectId.Equals(ParseUser.CurrentUser.ObjectId)) 
			{
				headerText.text = "Waiting for " + AppModel.currentOpponent["displayName"].ToString();
				refreshImage.gameObject.SetActive(true);
				refreshImage.transform.parent.gameObject.SetActive(true);
			}
			else
			{
				headerText.text = "Your turn against " + AppModel.currentOpponent["displayName"].ToString();
				buildingBlockSlotMachine.GenerateNewBuildingBlocks();
				refreshImage.gameObject.SetActive(false);
				refreshImage.transform.parent.gameObject.SetActive(false);
			}

			// List of all instantiated building blocks
			List<GameObject> instantiatedBuildingBlocks = new List<GameObject>();
			// Note: result should be ordered by index (ascending)
			List<GameObject> newBuildingBlocksList = new List<GameObject>();
			foreach(var item in result)
			{
				//Instantiate block from prefab
				ParseBuildingBlock pb = item as ParseBuildingBlock;
				Quaternion rotation = Quaternion.identity;
				Vector3 eulerAngles = rotation.eulerAngles;
				eulerAngles.z = pb.rotZ;
				rotation.eulerAngles = eulerAngles;
				GameObject newBuildingBlock = Instantiate (buildingBlocks[(int) pb.GetBuildingBlockType()], new Vector3(pb.posX, pb.posY, -1f), rotation) as GameObject; 
				//Store Parse Object ID in prefab class
				BuildingBlock newBuildingBlockScript = newBuildingBlock.GetComponentInChildren<BuildingBlock>();
				newBuildingBlockScript.objectId = pb.ObjectId;
				// Set building block to released (not swinging)
				newBuildingBlockScript.isHeldAtStart = false;
				newBuildingBlocksList.Add(newBuildingBlockScript.gameObject);
				// Set velocity to 0
				Rigidbody2D rb = newBuildingBlock.GetComponentsInChildren<Rigidbody2D>()[1];
				rb.velocity = Vector2.zero;
				rb.angularVelocity = 0.0f;
				rb.isKinematic = false;
				rb.Sleep();
				nextBuildingBlockIndex++; // Add to next building block index so that we know the index for the next building block we place in the scene
				yield return new WaitForSeconds(.01f);
				foreach(GameObject go in newBuildingBlocksList)
				{
					//Debug.Log (go.GetComponent<Rigidbody2D>().velocity.y);
					while(!MathUtilities.IsApproximately(go.GetComponent<Rigidbody2D>().velocity.x, 0.0f) || 
					      !MathUtilities.IsApproximately(go.GetComponent<Rigidbody2D>().velocity.y, 0.0f) ||
					      Mathf.Abs(go.GetComponent<Rigidbody2D>().angularVelocity) > 0.000000001f)
					{
						yield return null;
					}
				}
			}
			Time.timeScale = 1f;
		}
	}

	//Submit
	public void SubmitSceneToParse()
	{
		Debug.Log ("Submitting!");
		StartCoroutine ("SubmitSceneToParse_Local");
	}

	// Store all game objects with the BuildingBlock-tag using Parse
	IEnumerator SubmitSceneToParse_Local()
	{
		List<ParseBuildingBlock> newBlocks = new List<ParseBuildingBlock> ();
		var gameObjectBlocks = GameObject.FindGameObjectsWithTag ("BuildingBlock");
		foreach (var gameObjectBlock in gameObjectBlocks) 
		{
			bool blockExists = false;
			//iterate all blocks, if a block does not exist then add it to newblocks array, otherwise update its values
			foreach (var block in AppModel.currentBlocks)
			{
				if (block.ObjectId == gameObjectBlock.GetComponent<BuildingBlock>().objectId){
					if(block.posX != gameObjectBlock.transform.position.x || block.posY != gameObjectBlock.transform.position.y 
					   || block.rotZ != gameObjectBlock.transform.rotation.z)
					{
						block.posX = gameObjectBlock.transform.position.x;
						block.posY = gameObjectBlock.transform.position.y;
						block.rotZ = gameObjectBlock.transform.rotation.eulerAngles.z;
						block.SaveAsync().ContinueWith(t => {
						});
					}
					blockExists = true;
					break;
				}
			}
			if(blockExists)
				continue;
			//add the block as a new item
			ParseBuildingBlock newBlock = new ParseBuildingBlock ();
			newBlock.matchId = AppModel.currentMatch.ObjectId;
			newBlock.posX = gameObjectBlock.transform.position.x;
			newBlock.posY = gameObjectBlock.transform.position.y;
			newBlock.rotZ = gameObjectBlock.transform.rotation.eulerAngles.z;
			newBlock.index = gameObjectBlock.GetComponent<BuildingBlock>().index;
			newBlock.SetBuildingBlockType (gameObjectBlock.GetComponent<BuildingBlock>().buildingBlockType);
			newBlocks.Add (newBlock);
		}
		
		//if newblocks array has objects, upload them 
		if (newBlocks.Count () > 0) {
			newBlocks.SaveAllAsync().ContinueWith(t => {
			});
		}
		
		//change turn for the match object, then return to matches screen
		AppModel.currentMatch ["playerTurn"] = AppModel.currentOpponent;
		var updateTurn = AppModel.currentMatch.SaveAsync ();
		while (!updateTurn.IsCompleted) yield return null;
		if(!updateTurn.IsCanceled && !updateTurn.IsFaulted)
		{
			headerText.text = "Waiting for " + AppModel.currentOpponent["displayName"].ToString();
			refreshImage.gameObject.SetActive(true);
		}

		// Activate back button
		GameObject.Find("BackButton").GetComponent<Button>().interactable = true;
	}

	public void Button_SwitchUser_Clicked()
	{
		if(!debugPhysics2D)
		{
			return;
		}
		refreshing = true;
		StartCoroutine ("RefreshScene", true);
	}

	// Back button clicked
	// OPEN POINT: Move?
	public void Button_Back_Clicked()
	{
		Application.LoadLevel ("MyMatches");
	}

	public void Button_OnPointerDown()
	{
		isUserInteractingWithGUI = true;
	}

	public void Button_OnPointerUp()
	{
		isUserInteractingWithGUI = false;
	}

	public void Button_Refresh_Clicked()
	{
		refreshing = true;
		StartCoroutine ("RefreshScene", false);
	}

	IEnumerator RefreshScene(bool changeUser)
	{
		var gameObjectBlocks = GameObject.FindGameObjectsWithTag ("BuildingBlock");
		foreach(var gameObjectBlock in gameObjectBlocks) 
		{
			Destroy(gameObjectBlock);
		}
		if(changeUser)
		{
			Task<ParseUser> user;
			if(ParseUser.CurrentUser["displayName"].ToString() == "Adam")
				user = ParseUser.LogInAsync ("Emil", "emil123");
			else
				user = ParseUser.LogInAsync ("Adam", "adam123");
			
			while (!user.IsCompleted)
				yield return null;
			if(ParseUser.CurrentUser == null) 
			{
				Debug.LogError("Not able to login with user");
			}
		}
		headerText.text = "Loading blocks...";
		ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("Match").WhereEqualTo("objectId", AppModel.currentMatch.ObjectId);
		var find = query.FirstAsync ();
		while (!find.IsCompleted) yield return null;
		if (!find.IsCanceled && !find.IsFaulted) 
		{
			ResetSceneVaribles();
			AppModel.currentMatch = find.Result;
			Application.LoadLevel (Application.loadedLevelName);
		}
	}

	// Use this for initialization
	void Start() 
	{
		if(!debugPhysics2D)
		{
			switchUserButton.gameObject.SetActive(false);
			switchUserButton.enabled = false;
		}
		else
		{
			switchUserButton.gameObject.SetActive(true);
			switchUserButton.enabled = true;
		}

		ResetSceneVaribles();
		// Create the scene
		StartCoroutine("GetBlocksFromParse"); // From parse
	}

	IEnumerator EndGame()
	{
		AppModel.currentMatch["status"] = "finished";
		AppModel.currentMatch["playerVictor"] = AppModel.currentOpponent;
		var updateMatch = AppModel.currentMatch.SaveAsync();
		while (!updateMatch.IsCompleted) yield return null;
		if (!updateMatch.IsCanceled && !updateMatch.IsFaulted) {
			Debug.Log("match ended successfully, victor is: " + AppModel.currentOpponent["displayName"].ToString());
			headerText.text = "You lost, fuck off!";
			//TODO: delete all building blocks with this matchId. probably need to use Parse Cloud job
		}
		// Activate back button
		GameObject.Find("BackButton").GetComponent<Button>().interactable = true;
	}

	// Update is called once per frame
	void Update() 
	{
		if(refreshing) 
		{
			refreshImage.transform.Rotate(0, 0, 600 * Time.deltaTime * -1, Space.World);
		}

		if(blocksRetrievedFromParse)
		{
			StartCoroutine ("InstantiateBlocks");
			blocksRetrievedFromParse = false;
		}

		// Check if game is over
		if(gameOver && !hasMatchEnded)
		{
			hasMatchEnded = true;
			StartCoroutine("EndGame");
		}

		// Start timer counting down towards max sway time.
		if(buildingBlockSlotMachine.AreAllBuildingBlocksPlaced() && !hasStartSwayTimerStarted)
		{
			StartSwayTimer();
		}

		// Check if round is over for current player
		if(buildingBlockSlotMachine.AreAllBuildingBlocksPlaced() && 
		   GetNumberOfBuildingBlocks() != 0 &&
		   AreAllBuildingBlocksStill() &&
		   !dataSubmittedToParse && 
		   !gameOver && 
		   hasStartSwayTimerStarted)
		{
			SubmitSceneToParse();
			dataSubmittedToParse = true;
		}
	}
}
