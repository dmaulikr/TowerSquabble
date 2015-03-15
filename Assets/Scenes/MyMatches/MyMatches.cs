using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Parse;

public class MyMatches : MonoBehaviour {

	public Image refreshImage;
	private bool refreshing = false;
	private bool refreshingInProgress = false;

	public GameObject MatchButton;
	public GameObject AddFriendButton;
	Text userText;

	// Use this for initialization
	void Start () 
	{
		refreshing = true;

		if (ParseUser.CurrentUser == null)
			Application.LoadLevel ("Login");

		userText = GameObject.Find ("WelcomeText").GetComponent<Text> ();
		userText.text = "WELCOME " + ParseUser.CurrentUser["displayName"].ToString().ToUpper();
		StartCoroutine ("GetMatches");
	}

	void Update ()
	{
		if (refreshing) 
		{
			if(!refreshingInProgress)
			{
				//remove all existing matchbuttons
				GameObject[] matchButtons = GameObject.FindGameObjectsWithTag ("MatchButton");
				foreach (GameObject g in matchButtons) 
				{
					Destroy(g);
				}
				//set interactable to false on all buttons
				IEnumerable<Button> buttons = GameObject.FindObjectsOfType<Button>();
				foreach(Button b in buttons)
				{
					b.interactable = false;
				}
				// TODO: Set images alpha similar to when buttons are not interactable
				refreshingInProgress = true;
			}
			refreshImage.transform.Rotate(0, 0, 600 * Time.deltaTime * -1, Space.World);
		}
		else if(refreshingInProgress && !refreshing)
		{
			refreshImage.transform.rotation = new Quaternion(0,0,0,0);
			IEnumerable<Button> buttons = GameObject.FindObjectsOfType<Button>();
			foreach(Button b in buttons)
			{
				b.interactable = true;
			}
			refreshingInProgress = false;
		}
	}

	public void Button_FindMatch_Clicked()
	{
		Application.LoadLevel("FindMatch");
	}

	public void Button_SignOut_Clicked()
	{
		ParseUser.LogOut();
		Application.LoadLevel("Login");
	}

	public void Button_Refresh_Clicked()
	{
		refreshing = true;
		StartCoroutine ("GetMatches");
	}

	//Retrieve all matches for current player
	IEnumerator GetMatches(){
		//retrieve existing friends
		List<string> existingFriends = new List<string>();
		ParseQuery<ParseObject> friendsQuery = new ParseQuery<ParseObject>("FriendRelationship").Include("friend").WhereEqualTo("player", ParseUser.CurrentUser);
		var findFriends = friendsQuery.FindAsync ();	
		while (!findFriends.IsCompleted) yield return null;
		if (findFriends.IsCanceled || findFriends.IsFaulted) {
			Debug.Log(findFriends.Exception.InnerExceptions[0]);
		}
		else{
			IEnumerable<ParseObject> friendResults = findFriends.Result;
			foreach (ParseObject p in friendResults) 
			{
				ParseUser user = p["friend"] as ParseUser;
				existingFriends.Add(user["username"].ToString());
			}
		}

		//remove all existing matchbuttons
		GameObject[] matchButtons = GameObject.FindGameObjectsWithTag ("MatchButton");
		foreach (GameObject g in matchButtons) 
		{
			Destroy(g);
		}

		//get matches where user is player1
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject>("Match").Include("playerOne,playerTwo,playerTurn").WhereEqualTo("playerOne", ParseUser.CurrentUser);
		var find = mainQuery.FindAsync ();	
		while (!find.IsCompleted) yield return null;
		if (find.IsCanceled || find.IsFaulted) {
			Debug.Log(find.Exception.InnerExceptions[0]);
		}
		IEnumerable<ParseObject> results = find.Result;
		//get matches where user is player2
		mainQuery = new ParseQuery<ParseObject>("Match").Include("playerOne,playerTwo,playerTurn").WhereEqualTo("playerTwo", ParseUser.CurrentUser);
		find = mainQuery.FindAsync ();
		while (!find.IsCompleted) yield return null;
		//merge the two results
		results = results.Concat(find.Result);
		//sort the results by status
		results = results.Where(x => x["status"].ToString() == "waiting").Concat (results.Where(x => x["status"].ToString() == "challenging")).Concat(results.Where(x => x["status"].ToString() == "active")).Concat(results.Where(x => x["status"].ToString() == "finished"));

		//reset variables for these results
		refreshing = false;
		int counter = 0;
		int initialY = 0;
		int newScrollContainerY = -990;
		var scrollContainer = GameObject.Find("MatchesScrollContent").GetComponent<RectTransform>();
		scrollContainer.sizeDelta = new Vector2(200, 1980);

		//iterate results
		foreach(ParseObject p in results){
			//Get opponent
			ParseUser opponent = null;
			string opponentUserName = "";
			string opponentDisplayName = "";
			ParseUser playerOne = p["playerOne"] as ParseUser;
			if(playerOne.ObjectId.Equals(ParseUser.CurrentUser.ObjectId))
			{
				if(p.ContainsKey("playerTwo"))
				{
					opponent = p["playerTwo"] as ParseUser;
					opponentUserName = opponent.Username.ToString();
					opponentDisplayName = opponent["displayName"].ToString();
				}
			}
			else
			{
				opponent = p["playerOne"] as ParseUser;
				opponentUserName = opponent.Username.ToString();
				opponentDisplayName = opponent["displayName"].ToString();
			}

			//Add friend button
			if(p["status"].ToString() != "waiting" && !existingFriends.Contains(opponentUserName))
			{
				GameObject addFriendButtonInstance = Instantiate(AddFriendButton, new Vector3(455,initialY,0), transform.rotation) as GameObject;
				addFriendButtonInstance.transform.SetParent(GameObject.Find ("Matches").transform, false);
				addFriendButtonInstance.tag = "MatchButton";

				//Store oppponent on add friend button
				AddFriendButtonScript afbs = addFriendButtonInstance.GetComponent<AddFriendButtonScript>(); 
				afbs.userName = opponentUserName;
				
				//Add click event to add friend button
				Button addFriendButton = addFriendButtonInstance.GetComponent<Button>();
				addClickEvent(addFriendButton, "AddFriend");
			}

			//Instantiate match button
			GameObject matchButtonInstance = Instantiate(MatchButton, new Vector3(0,initialY,0), transform.rotation) as GameObject;
			matchButtonInstance.transform.SetParent(GameObject.Find ("Matches").transform, false);
			matchButtonInstance.tag = "MatchButton";
			Text matchButtonText = matchButtonInstance.GetComponentInChildren<Text>();
			//Set button text based on status
			if(p["status"].ToString() == "waiting")
			{
				matchButtonText.text = "Searching for opponent...";
			}
			else if(p["status"].ToString() == "challenging")
			{
				matchButtonText.text = "Challenging (TBI)";
				StartCoroutine("RejectMatch", p);
			}
			else if(p["status"].ToString() == "active")
			{
				ParseUser playerTurn = p["playerTurn"] as ParseUser;
				if(playerTurn.ObjectId.Equals(ParseUser.CurrentUser.ObjectId))
					matchButtonText.text = "Your turn against " + opponentDisplayName;
				else
					matchButtonText.text = "Waiting for " + opponentDisplayName;
				
				//Store match object and current opponentUserName on button
				MatchButtonScript mbs = matchButtonInstance.GetComponent<MatchButtonScript>(); 
				mbs.opponent = opponent;
				mbs.matchObject = p;
				
				//Add click event
				Button matchButton = matchButtonInstance.GetComponent<Button>();
				addClickEvent(matchButton, "Match");
			}
			else if(p["status"].ToString() == "finished")
			{
				ParseUser playerVictor = p["playerVictor"] as ParseUser;
				if(playerVictor.ObjectId.Equals(ParseUser.CurrentUser.ObjectId))
					matchButtonText.text = "You won against " + opponentDisplayName + "!";
				else
					matchButtonText.text = "You lost against " + opponentDisplayName;
			}
			counter++;
			initialY -= 180;
			newScrollContainerY -= 180;
			if(counter > 5)
			{
				var currentScrollContainerHeight = scrollContainer.sizeDelta.y;
				scrollContainer.sizeDelta = new Vector2(200, currentScrollContainerHeight + 180);
			}
		}
		scrollContainer.position = new Vector3(scrollContainer.position.x, newScrollContainerY, scrollContainer.position.z);
	}

	private IEnumerator AcceptMatch(ParseObject match)
	{
		match.Increment("matchLock");
		match["status"] = "active";
		match["playerTurn"] = match["playerTwo"];
		var save = match.SaveAsync();
		while (!save.IsCompleted) yield return null;
		if(!save.IsCanceled && !save.IsFaulted)
		{
			Debug.Log("Match accepted!");
			refreshing = true;
			StartCoroutine("GetMatches");
		}
	}

	private IEnumerator RejectMatch(ParseObject match)
	{
		var save = match.DeleteAsync();
		while (!save.IsCompleted) yield return null;
		if(!save.IsFaulted && !save.IsCanceled)
		{
			Debug.Log("Match rejected");
			refreshing = true;
			StartCoroutine("GetMatches");
		}
	}

	void addClickEvent(Button b, string buttonType)
	{
		if(buttonType.Equals("Match"))
		{
			b.onClick.AddListener(() => matchButtonClicked(b));
		}
		else
		{
			b.onClick.AddListener(() => addFriendButtonClicked(b));
		}
	}

	public void matchButtonClicked(Button b) 
	{
		MatchButtonScript mbs = b.GetComponent<MatchButtonScript>();
		AppModel.currentOpponent = mbs.opponent;
		AppModel.currentMatch = mbs.matchObject;
		Application.LoadLevel ("Match");
	}

	public void addFriendButtonClicked(Button b)
	{
		AddFriendButtonScript afbs = b.GetComponent<AddFriendButtonScript> ();
		string opponentUserName = afbs.userName;
		FriendHelper.DoCoroutine(FriendHelper.AddFriend(opponentUserName));
		b.gameObject.SetActive(false);
	}
}
