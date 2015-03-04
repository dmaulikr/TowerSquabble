using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Parse;
using System.Collections.Generic;
using System.Linq;

//using UnityEditor; // for debug

public class MatchManager : MonoBehaviour {
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
	private bool dataSubmittedToParse = false; // Was data submitted to Parse this round?
	// Reference to header text (OPEN POINT: Move?)
	public Text headerText;
	// Has match ended?
	private bool hasMatchEnded = false;
	// An index added to a building block that keeps track of in what order the buildingblock was put in the scene
	public int nextBuildingBlockIndex = 0;

	// Timer varibles
	private float swayTimerStarted = 0f;
	public float swayMaxWaitTime = 10f;
	private bool hasStartSwayTimerStarted = false;

	// Is user interacting with GUI?
	public bool isUserInteractingWithGUI = false;

	// Add stuff here that is going to be initalized/reseted
	// when scene is loaded or when it is the logged in player's turn again
	private void ResetSceneVaribles()
	{
		hasStartSwayTimerStarted = false;
		// Init game over varible
		gameOver = false;
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
		// Get all building blocks in the scene
		GameObject[] buildingBlocks = GameObject.FindGameObjectsWithTag("BuildingBlock");
		// Loop through the rigidbodies of all the building blocks and check their velocity
		foreach(GameObject g in buildingBlocks)
		{
			if(!Mathf.Approximately(g.GetComponent<Rigidbody2D>().velocity.x,0.0f) || !Mathf.Approximately(g.GetComponent<Rigidbody2D>().velocity.y,0.0f))
			{
				// Check if sway timer has passed max time
				if(Time.time > (swayTimerStarted + swayMaxWaitTime)) 
				{
					// Set velocity to 0 for all building blocks in the scene
					foreach(GameObject go in buildingBlocks)
					{
						// OPEN POINT: Check velocity: if large, do not set it to 0
						go.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
						go.GetComponent<Rigidbody2D>().angularVelocity = 0f;
					}
					Debug.Log ("Max sway time!");
				}
				return false;
			}
		}

		return true;
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
			// Check whos turn it is
			if(AppModel.currentMatch ["turn"].ToString () != AppModel.currentUserName.ToString()) 
			{
				headerText.text = "Waiting for " + AppModel.currentOpponentDisplayName;
				refreshImage.gameObject.SetActive(true);
				refreshImage.transform.parent.gameObject.SetActive(true);
			}
			else
			{
				headerText.text = "Your turn against " + AppModel.currentOpponentDisplayName;
				buildingBlockSlotMachine.GenerateNewBuildingBlocks();
				refreshImage.gameObject.SetActive(false);
				refreshImage.transform.parent.gameObject.SetActive(false);
			}

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
				newBuildingBlockScript.HasCollided = true;
				newBuildingBlocksList.Add(newBuildingBlockScript.gameObject);
				// Set velocity to 0
				newBuildingBlock.GetComponentsInChildren<Rigidbody2D>()[1].velocity = Vector2.zero;
				newBuildingBlock.GetComponentsInChildren<Rigidbody2D>()[1].angularVelocity = 0.0f;
				newBuildingBlock.GetComponentsInChildren<Rigidbody2D>()[1].isKinematic = true;
				nextBuildingBlockIndex++; // Add to next building block index so that we know the index for the next building block we place in the scene
				//EditorApplication.isPaused = true;
			}
			foreach(GameObject go in newBuildingBlocksList)
			{
				go.GetComponent<Rigidbody2D>().isKinematic = false;
				yield return null;
			}
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
		AppModel.currentMatch ["turn"] = AppModel.currentOpponentUserName.ToString ();
		var updateTurn = AppModel.currentMatch.SaveAsync ();
		while (!updateTurn.IsCompleted) yield return null;
		if(!updateTurn.IsCanceled && !updateTurn.IsFaulted)
		{
			headerText.text = "Waiting for " + AppModel.currentOpponentDisplayName;
			refreshImage.gameObject.SetActive(true);
		}
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
		StartCoroutine ("RefreshScene");
	}

	IEnumerator RefreshScene()
	{
		var gameObjectBlocks = GameObject.FindGameObjectsWithTag ("BuildingBlock");
		foreach (var gameObjectBlock in gameObjectBlocks) 
		{
			Destroy(gameObjectBlock);
		}
		headerText.text = "Loading blocks...";
		ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("Match").WhereEqualTo("objectId", AppModel.currentMatch.ObjectId);
		var find = query.FirstAsync ();
		while (!find.IsCompleted) yield return null;
		if (!find.IsCanceled && !find.IsFaulted) 
		{
			AppModel.currentMatch = find.Result;
			Application.LoadLevel (Application.loadedLevelName);
		}
	}

	// Use this for initialization
	void Start() 
	{
		ResetSceneVaribles ();
		// Create the scene
		StartCoroutine("GetBlocksFromParse"); // From parse
	}

	IEnumerator EndGame()
	{
		AppModel.currentMatch["status"] = "finished";
		AppModel.currentMatch["victor"] = AppModel.currentOpponentUserName;
		var updateMatch = AppModel.currentMatch.SaveAsync();
		while (!updateMatch.IsCompleted) yield return null;
		if (!updateMatch.IsCanceled && !updateMatch.IsFaulted) {
			Debug.Log("match ended successfully, victor is: " + AppModel.currentDisplayName);
			headerText.text = "You lost, fuck off!";
			//TODO: delete all building blocks with this matchId. probably need to use Parse Cloud job
		}
	}

	// Update is called once per frame
	void Update() 
	{
		if (refreshing) 
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

		if(buildingBlockSlotMachine.AreAllBuildingBlocksPlaced() && !hasStartSwayTimerStarted)
		{
			StartSwayTimer();
		}

		// Check if round is over for current player
		if(buildingBlockSlotMachine.AreAllBuildingBlocksPlaced() && 
		   GetNumberOfBuildingBlocks() != 0 &&
		   AreAllBuildingBlocksStill() &&
		   !dataSubmittedToParse && 
		   !gameOver)
		{
			SubmitSceneToParse();
			dataSubmittedToParse = true;
		}
	}
}
