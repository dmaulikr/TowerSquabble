using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildingBlockSlotMachine : MonoBehaviour 
{
	// Prefabs
	public GameObject[] listOfBuildingBlocks;
	// Building block buttons
	public BuildingBlockButton[] buildingBlockButtons;
	// Flag letting us know if we are currently spinning buttons
	private bool spinningButtons = false;
	private bool hasGeneratedBuildingBlocks = false;

	private int blocksUsed = 0;

	// Are all the building blocks placed in the scene?
	public bool AreAllBuildingBlocksPlaced()
	{
		bool noBlocksLeft = true;
		for(int i = 0; i < buildingBlockButtons.Length; ++i)
		{
			if(buildingBlockButtons[i].CurrentBuildingBlockType != ParseBuildingBlock.Type.none)
			{
				return false;
			}
		}
		return noBlocksLeft && !spinningButtons && hasGeneratedBuildingBlocks;
	}

	// Generate new building blocks
	public void GenerateNewBuildingBlocks()
	{
		spinningButtons = true;
		for(int i = 0; i < buildingBlockButtons.Length; ++i)
		{
			// Generate the new type for each button
			buildingBlockButtons[i].CurrentBuildingBlockType = (ParseBuildingBlock.Type) Random.Range(0,4);
			// Set each button to interactable, but with flag isSpinning and stopSpinning to false
			buildingBlockButtons[i].SetInteractable(true);
			buildingBlockButtons[i].SetIsSpinAnimationInProgress(true);
			buildingBlockButtons[i].SetStopSpinning(false);
			buildingBlockButtons[i].SetIsSpinning(true);
			buildingBlockButtons[i].ResetTiledImagePosition();
		}

		// Start coroutine to spin buttons / slots
		StartCoroutine(SpinButtons());
	}

	// Spin the buttons
	IEnumerator SpinButtons()
	{
		// Get time at start of spin
		float startTime = Time.time;
		float timeBetweenStops = .5f;
		int slotsStopped = 0;
		while(true)
		{
			yield return null;

			// Stop slot/button?
			if(startTime + (timeBetweenStops * (slotsStopped + 1)) <= Time.time)
			{
				buildingBlockButtons[slotsStopped].SetStopSpinning(true);
				buildingBlockButtons[slotsStopped].CalculateOffsetToNextCurrentType(buildingBlockButtons[slotsStopped].spriteImage.rectTransform.anchoredPosition.y);
				++slotsStopped;
			}

			// Are we done with the animation?
			int numButtonsDone = 0;
			// Loop through each button and spin it
			foreach(BuildingBlockButton bbt in buildingBlockButtons)
			{
				bbt.Spin();
				if(!bbt.IsSpinning())
				{
					numButtonsDone++;
				}
			}

			if(numButtonsDone >= buildingBlockButtons.Length)
			{
				// We are done!
				break;
			}
		}

		// Set all buttons to not spinning
		for(int i = 0; i < buildingBlockButtons.Length; ++i)
		{
			buildingBlockButtons[i].SetIsSpinAnimationInProgress(false);
		}
		spinningButtons = false;
		hasGeneratedBuildingBlocks = true;
	}

	public GameObject GetBuildingBlockPrefab(ParseBuildingBlock.Type type)
	{
		if(type == ParseBuildingBlock.Type.crate)
		{
			return listOfBuildingBlocks[0];
		}
		else if(type == ParseBuildingBlock.Type.triangle)
		{
			return listOfBuildingBlocks[1];
		}
		else if(type == ParseBuildingBlock.Type.circle)
		{
			return listOfBuildingBlocks[2];
		}
		else if(type == ParseBuildingBlock.Type.pillar)
		{
			return listOfBuildingBlocks[3];
		}
		else
		{
			return null;
		}
	}

	public void BuildingBlockReleased(int index)
	{
		//When player drops the first block, notice that current move has begun
		if (blocksUsed == 0) 
		{
			GameObject.Find("BackButton").GetComponent<Button>().interactable = false;
		}
		//When player drops the third block, remove information about current move
		else if(blocksUsed > 1)
		{
			GameObject.Find("BackButton").GetComponent<Button>().interactable = true;
		}
		blocksUsed++;

		buildingBlockButtons[index].CurrentBuildingBlockType = ParseBuildingBlock.Type.none;
		buildingBlockButtons[index].SetInteractable(false);
	}
}
