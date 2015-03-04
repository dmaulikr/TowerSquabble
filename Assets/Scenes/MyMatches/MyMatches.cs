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

		if (AppModel.currentUserName == null || AppModel.currentUserName.Length < 1)
			Application.LoadLevel ("Login");

		userText = GameObject.Find ("WelcomeText").GetComponent<Text> ();
		userText.text = "WELCOME " + AppModel.currentDisplayName.ToUpper();
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
		Debug.Log ("finding new match");
		StartCoroutine ("FindMatch");
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

	//Find a new match to join. If no waiting match is found, a new one will be created
	IEnumerator FindMatch()
	{
		//get count of matches waiting
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject> ("Match").WhereNotEqualTo ("player1", AppModel.currentUserName).WhereEqualTo ("status", "waiting");
		var find = mainQuery.CountAsync ();	
		while (!find.IsCompleted) yield return null;
		int resultCount = find.Result;

		if (resultCount > 0) {
			Debug.Log ("found " + resultCount + " matches, picking random one");
			var skipNumber = Random.Range (0, resultCount);
			mainQuery = new ParseQuery<ParseObject> ("Match").WhereNotEqualTo ("player1", AppModel.currentUserName).WhereEqualTo ("status", "waiting").Limit (1).Skip(skipNumber);
			var retrieve = mainQuery.FindAsync();
			while (!retrieve.IsCompleted) yield return null;
			IEnumerable<ParseObject> foundMatches = retrieve.Result;
			if (foundMatches.Count () > 0) {
				var match = foundMatches.ElementAt(0);
				Debug.Log("trying to join random match " + match.ObjectId + " where " + match["player1"] + " is waiting");
				match.Increment("matchLock");
				var save = match.SaveAsync();
				
				while (!save.IsCompleted) yield return null;
				mainQuery = ParseObject.GetQuery("Match");
				var getMatch = mainQuery.GetAsync(match.ObjectId);
				
				while (!getMatch.IsCompleted) yield return null;	
				ParseObject lockedMatch = getMatch.Result;
				if(int.Parse(lockedMatch["matchLock"].ToString()) <= 1){
					Debug.Log("successfully locked match " + match.ObjectId);
					match["player2"] = AppModel.currentUserName;
					match["player2DisplayName"] = AppModel.currentDisplayName;
					match["status"] = "active";
					match["turn"] = AppModel.currentUserName;
					var updateMatch = match.SaveAsync();
					while (!updateMatch.IsCompleted) yield return null;
					if(!updateMatch.IsCanceled && !updateMatch.IsFaulted){
						Debug.Log("successfully activated match, player2:s turn");
						StartCoroutine("GetMatches");
					}
				}
				else{
					Debug.Log("match lock failed, create a new game...");
					StartCoroutine("CreateMatch");
				}
			}
			else {
				Debug.Log("selected game not found, create a new game...");
				StartCoroutine("CreateMatch");
			}
		}
		else{
			Debug.Log("no waiting matches found, create a new game...");
			StartCoroutine("CreateMatch");
		}
	}

	//Create a new match with status waiting
	IEnumerator CreateMatch(){
		ParseObject newMatch = new ParseObject("Match");
		newMatch["player1"] = AppModel.currentUserName.ToString();
		newMatch ["player1DisplayName"] = AppModel.currentDisplayName.ToString ();
		newMatch["status"] = "waiting";
		newMatch ["matchLock"] = 0;
		var saveNewMatch = newMatch.SaveAsync();
		while (!saveNewMatch.IsCompleted) yield return null;
		if (!saveNewMatch.IsCanceled || !saveNewMatch.IsFaulted) {
			Debug.Log ("new match created");
			StartCoroutine ("GetMatches");
		} 
		else {
			Debug.Log("error creating new match");
		}
	}

	//Retrieve all matches for current player
	IEnumerator GetMatches(){
		//get matches where user is player1
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject>("Match").WhereEqualTo("player1", AppModel.currentUserName);
		var find = mainQuery.FindAsync ();	
		while (!find.IsCompleted) yield return null;
		if (find.IsCanceled || find.IsFaulted) {
			Debug.Log(find.Exception.InnerExceptions[0]);
		}
		IEnumerable<ParseObject> results = find.Result;
		//get matches where user is player2
		mainQuery = new ParseQuery<ParseObject>("Match").WhereEqualTo("player2", AppModel.currentUserName);
		find = mainQuery.FindAsync ();
		while (!find.IsCompleted) yield return null;
		//merge the two results
		results = results.Concat(find.Result);
		//sort the results by status
		results = results.Where(x => x["status"].ToString() == "waiting").Concat(results.Where(x => x["status"].ToString() == "active")).Concat(results.Where(x => x["status"].ToString() == "finished"));

		//remove all existing matchbuttons
		GameObject[] matchButtons = GameObject.FindGameObjectsWithTag ("MatchButton");
		foreach (GameObject g in matchButtons) 
		{
			Destroy(g);
		}

		refreshing = false;
		int counter = 0;
		int initialY = 0;
		int newScrollContainerY = -990;
		var scrollContainer = GameObject.Find("MatchesScrollContent").GetComponent<RectTransform>();
		scrollContainer.sizeDelta = new Vector2(200, 1980);

		foreach(ParseObject p in results){
			//Get opponentUserName name
			string opponentUserName = "";
			string opponentDisplayName = "";
			if(p["player1"].ToString() == AppModel.currentUserName)
			{
				if(p.ContainsKey("player2"))
				{
					opponentUserName = p["player2"].ToString();
					opponentDisplayName = p["player2DisplayName"].ToString();
				}
			}
			else
			{
				opponentUserName = p["player1"].ToString();
				opponentDisplayName = p["player1DisplayName"].ToString();
			}

			//Add friend button test
			GameObject addFriendButtonInstance = Instantiate(AddFriendButton, new Vector3(455,initialY,0), transform.rotation) as GameObject;
			addFriendButtonInstance.transform.SetParent(GameObject.Find ("Matches").transform, false);
			addFriendButtonInstance.tag = "MatchButton";

			//Store oppponent on add friend button
			AddFriendButtonScript afbs = addFriendButtonInstance.GetComponent<AddFriendButtonScript>(); 
			afbs.userName = opponentUserName;
			
			//Add click event to add friend button
			Button addFriendButton = addFriendButtonInstance.GetComponent<Button>();
			addClickEvent(addFriendButton, "AddFriend");

			//Instantiate button
			GameObject matchButtonInstance = Instantiate(MatchButton, new Vector3(0,initialY,0), transform.rotation) as GameObject;
			matchButtonInstance.transform.SetParent(GameObject.Find ("Matches").transform, false);
			matchButtonInstance.tag = "MatchButton";
			Text matchButtonText = matchButtonInstance.GetComponentInChildren<Text>();
			//Set button text based on status
			if(p["status"].ToString() == "waiting")
			{
				matchButtonText.text = "Searching for opponent...";
			}
			else if(p["status"].ToString() == "active")
			{
				if(p["turn"].ToString() == AppModel.currentUserName)
					matchButtonText.text = "Your turn against " + opponentDisplayName;
				else
					matchButtonText.text = "Waiting for " + opponentDisplayName;
				
				//Store match object and current opponentUserName on button
				MatchButtonScript mbs = matchButtonInstance.GetComponent<MatchButtonScript>(); 
				mbs.opponentUserName = opponentUserName;
				mbs.opponentDisplayName = opponentDisplayName;
				mbs.parseObject = p;
				
				//Add click event
				Button matchButton = matchButtonInstance.GetComponent<Button>();
				addClickEvent(matchButton, "Match");
			}
			else if(p["status"].ToString() == "finished")
			{
				if(p["victor"].ToString() == AppModel.currentUserName)
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
		AppModel.currentOpponentUserName = mbs.opponentUserName.ToString ();
		AppModel.currentOpponentDisplayName = mbs.opponentDisplayName.ToString ();
		AppModel.currentMatch = mbs.parseObject;
		Application.LoadLevel ("Match");
	}

	public void addFriendButtonClicked(Button b)
	{
		AddFriendButtonScript afbs = b.GetComponent<AddFriendButtonScript> ();
		string opponentUserName = afbs.userName;
		FriendHelper.DoCoroutine(FriendHelper.AddFriend(opponentUserName));
	}
}
