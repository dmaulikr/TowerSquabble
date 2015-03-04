using UnityEngine;
using System.Collections;

public class GUIActionManager : MonoBehaviour
{
	public void GoToScene_MainMenu()
	{
		Application.LoadLevel("MainMenu");
	}

	public void GoToScene_SelectGame()
	{
		Application.LoadLevel("SelectGame");
	}

	public void GoToScene_LevelOne()
	{
		Application.LoadLevel("LevelOne");
	}
}
