using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class SearchPlayer : MonoBehaviour {

	public InputField UsernameInput;
	public GameObject ChallengeFriendButton;
	private bool showStatusText;
	private Text statusText;

	void Start()
	{
		statusText = GameObject.Find ("StatusText").GetComponent<Text>();
	}

	void Update()
	{
		if(showStatusText)
		{
			statusText.enabled = true;
		}
		else
		{
			statusText.enabled = false;
		}
	}

	public void  Username_Input_Changed(string value)
	{
		string username = UsernameInput.text;
		StartCoroutine("FindUser", username);
	}

	private IEnumerator FindUser(string username)
	{
		GameObject [] challengerPlayerButtons = GameObject.FindGameObjectsWithTag("ChallengePlayerButton");
		foreach (GameObject g in challengerPlayerButtons) 
		{
			Destroy(g);
		}
		statusText.text = "Searching...";
		showStatusText = true;

		ParseQuery<ParseUser> query = new ParseQuery<ParseUser>("_User").WhereStartsWith("username", username).WhereNotEqualTo("username",ParseUser.CurrentUser.Username); 
		var find = query.FindAsync();
		while (!find.IsCompleted) yield return null;
		if (find.IsCanceled || find.IsFaulted) {
			Debug.Log(find.Exception.InnerExceptions[0]);
		}
		IEnumerable<ParseUser> results = find.Result;

		//reset variables for these results
		showStatusText = false;
		bool hasResults = false;
		int counter = 0;
		int initialY = 0;
		int newScrollContainerY = -990;
		var scrollContainer = GameObject.Find("PlayersScrollContent").GetComponent<RectTransform>();
		scrollContainer.sizeDelta = new Vector2(200, 1980);

		//iterate results
		foreach(ParseUser user in results)
		{
			hasResults = true;
			GameObject challengeFriendButtonInstance = Instantiate(ChallengeFriendButton, new Vector3(0,initialY,0), transform.rotation) as GameObject;
			challengeFriendButtonInstance.transform.SetParent(GameObject.Find ("Players").transform, false);
			challengeFriendButtonInstance.tag = "ChallengePlayerButton";
			Text challengeFriendButtonText = challengeFriendButtonInstance.GetComponentInChildren<Text>();
			//Set button text based on status
			challengeFriendButtonText.text = "Challenge " + user["displayName"];
			//Store match object and current opponentUserName on button
			ChallengeFriendButtonScript cfbs = challengeFriendButtonInstance.GetComponent<ChallengeFriendButtonScript>(); 
			cfbs.opponent = user;
			
			//Add click event
			Button challengeFriendButton = challengeFriendButtonInstance.GetComponent<Button>();
			addClickEvent(challengeFriendButton);
			
			//increment position
			counter++;
			initialY -= 180;
			newScrollContainerY -= 180;
			if(counter > 5)
			{
				var currentScrollContainerHeight = scrollContainer.sizeDelta.y;
				scrollContainer.sizeDelta = new Vector2(200, currentScrollContainerHeight + 180);
			}
			scrollContainer.position = new Vector3(scrollContainer.position.x, newScrollContainerY, scrollContainer.position.z);
		}

		//if no players found, show text
		if(!hasResults)
		{
			showStatusText = true;
			statusText.text = "No players found...";
		}
	}

	public IEnumerator ChallengeFriend(ParseUser playerChallenged)
	{
		ParseObject newMatch = new ParseObject("Match");
		newMatch["playerOne"] = ParseUser.CurrentUser;
		newMatch["playerTwo"] = playerChallenged;
		newMatch["status"] = "challenging";
		newMatch ["matchLock"] = 0;
		var saveNewMatch = newMatch.SaveAsync();
		while (!saveNewMatch.IsCompleted) yield return null;
		if (!saveNewMatch.IsCanceled || !saveNewMatch.IsFaulted) {
			Debug.Log (playerChallenged.Username + " is challenged!");
			Application.LoadLevel("MyMatches");
		} 
		else {
			Debug.Log("error challenging " + playerChallenged.Username);
			Application.LoadLevel("MyMatches");
		}
	}
	
	private void addClickEvent(Button b)
	{
		b.onClick.AddListener(() => challengeFriendButtonClicked(b));
	}
	
	public void challengeFriendButtonClicked(Button b) 
	{
		ChallengeFriendButtonScript cfbs = b.GetComponent<ChallengeFriendButtonScript>();
		ParseUser playerChallenged = cfbs.opponent;
		Debug.Log("challenging " + playerChallenged.Username);
		StartCoroutine("ChallengeFriend", playerChallenged);
	}

	public void Button_Back_Clicked()
	{
		Application.LoadLevel ("FindMatch");
	}
}
