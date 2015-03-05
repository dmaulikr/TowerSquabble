using UnityEngine;
using System.Collections;

public class BuildingBlockFloorTrigger : MonoBehaviour {
	public int maxNumberOfBuildingBlocksOnFloor = 3;
	private int numberOfBuildingBlocksInTrigger = 0;

	// Refrence to the level manager
	private MatchManager matchManager;

	void Start()
	{
		matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
		numberOfBuildingBlocksInTrigger = 0;
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if(col.tag == "BuildingBlock")
		{
			numberOfBuildingBlocksInTrigger++;
			if(numberOfBuildingBlocksInTrigger > maxNumberOfBuildingBlocksOnFloor)
			{
				// Game is over
				matchManager.gameOver = true;
			}
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		if(col.tag == "BuildingBlock")
		{
			numberOfBuildingBlocksInTrigger--;
		}
	}
}
