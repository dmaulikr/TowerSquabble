using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircusDirectorAnimation : MonoBehaviour 
{
	private Animator animator;
	private List<string> animationNameList;
	private int currentlyPlaying = 0;
	private int clipPlayingLastFrame = 0;

	// Use this for initialization
	void Start () 
	{
		animationNameList = new List<string>();
		animationNameList.Add("Idle");
		animationNameList.Add("Idle2");
		animationNameList.Add("Idle3");
		animationNameList.Add("Idle_PointUp");

		animator = gameObject.GetComponent<Animator> ();
		animator.SetBool(animationNameList[0], false);
		animator.SetBool(animationNameList[1], false);
		animator.SetBool(animationNameList[2], false);
		animator.SetBool(animationNameList[3], true);
	}

	// Update is called once per frame
	void Update () 
	{
		/*bool idle1 = animator.GetBool(animationNameList[0]);
		bool idle2 = animator.GetBool(animationNameList[1]);
		bool idle3 = animator.GetBool(animationNameList[2]);
		bool idle4 = animator.GetBool(animationNameList[3]);*/

		if(animator.GetCurrentAnimatorStateInfo(0).IsName(animationNameList[0]))
		{
			currentlyPlaying = 0;
		}
		else if(animator.GetCurrentAnimatorStateInfo(0).IsName(animationNameList[1]))
		{
			currentlyPlaying = 1;
		}
		else if(animator.GetCurrentAnimatorStateInfo(0).IsName(animationNameList[2]))
		{
			currentlyPlaying = 2;
		}
		else if(animator.GetCurrentAnimatorStateInfo(0).IsName(animationNameList[3]))
		{
			currentlyPlaying = 3;
		}

		if(clipPlayingLastFrame != currentlyPlaying) // A switch between animation clips occurred
		{
			// Switch to another random clip
			int newClipIndex = 0;
			while(newClipIndex == currentlyPlaying)
			{
				newClipIndex = Random.Range(0,animationNameList.Count);
			}
			// Assign bools
			for(int i = 0; i < animationNameList.Count; ++i)
			{
				if(newClipIndex == i)
				{
					animator.SetBool(animationNameList[i], true);
				}
				else
				{
					animator.SetBool(animationNameList[i], false);
				}
			}
		}

		clipPlayingLastFrame = currentlyPlaying;
	}
}
