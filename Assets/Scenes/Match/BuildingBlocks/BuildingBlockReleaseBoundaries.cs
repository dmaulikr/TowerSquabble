using UnityEngine;
using System.Collections;

public class BuildingBlockReleaseBoundaries : MonoBehaviour 
{
	private int triggerCount = 0;
	void OnTriggerEnter2D()
	{
		triggerCount++;
	}
	void OnTriggerExit2D()
	{
		triggerCount--;
	}

	void Update()
	{
		if(triggerCount > 0)
		{
			transform.parent.gameObject.GetComponent<BuildingBlock>().NotReleasedAndTouchingCollider = true;
		}
		else
		{
			transform.parent.gameObject.GetComponent<BuildingBlock>().NotReleasedAndTouchingCollider = false;
		}
	}
}
