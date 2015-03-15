using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Parse;

public class FindMatch : MonoBehaviour {

	public GameObject ChallengeFriendButton;

	// Use this for initialization
	void Start () {
		StartCoroutine("RetrieveFriends");
	}

	//Find a new match to join. If no waiting match is found, a new one will be created
	IEnumerator FindRandomMatch()
	{
		//get count of matches waiting
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject> ("Match").WhereNotEqualTo ("playerOne", ParseUser.CurrentUser).WhereEqualTo ("status", "waiting");
		var find = mainQuery.CountAsync ();	
		while (!find.IsCompleted) yield return null;
		int resultCount = find.Result;
		
		if (resultCount > 0) {
			Debug.Log ("found " + resultCount + " matches, picking random one");
			var skipNumber = Random.Range (0, resultCount);
			mainQuery = new ParseQuery<ParseObject> ("Match").WhereNotEqualTo ("playerOne", ParseUser.CurrentUser).WhereEqualTo ("status", "waiting").Limit (1).Skip(skipNumber);
			var retrieve = mainQuery.FindAsync();
			while (!retrieve.IsCompleted) yield return null;
			IEnumerable<ParseObject> foundMatches = retrieve.Result;
			if (foundMatches.Count () > 0) {
				var match = foundMatches.ElementAt(0);
				Debug.Log("trying to join random match " + match.ObjectId + " where " + match["playerOne"] + " is waiting");
				match.Increment("matchLock");
				var save = match.SaveAsync();
				
				while (!save.IsCompleted) yield return null;
				mainQuery = ParseObject.GetQuery("Match");
				var getMatch = mainQuery.GetAsync(match.ObjectId);
				
				while (!getMatch.IsCompleted) yield return null;	
				ParseObject lockedMatch = getMatch.Result;
				if(int.Parse(lockedMatch["matchLock"].ToString()) <= 1){
					Debug.Log("successfully locked match " + match.ObjectId);
					match["playerTwo"] = ParseUser.CurrentUser;
					//match["player2DisplayName"] = AppModel.currentDisplayName;
					match["status"] = "active";
					match["playerTurn"] = ParseUser.CurrentUser;
					var updateMatch = match.SaveAsync();
					while (!updateMatch.IsCompleted) yield return null;
					if(!updateMatch.IsCanceled && !updateMatch.IsFaulted){
						Debug.Log("successfully activated match, player2:s turn");
						//StartCoroutine("GetMatches");
						Application.LoadLevel("MyMatches");
					}
					else
					{
						Debug.Log("error adding current user as player 2 for match..");
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
		newMatch["playerOne"] = ParseUser.CurrentUser;
		newMatch["status"] = "waiting";
		newMatch ["matchLock"] = 0;
		var saveNewMatch = newMatch.SaveAsync();
		while (!saveNewMatch.IsCompleted) yield return null;
		if (!saveNewMatch.IsCanceled || !saveNewMatch.IsFaulted) {
			Debug.Log ("new match created");
			//StartCoroutine ("GetMatches");
			Application.LoadLevel("MyMatches");
		} 
		else {
			Debug.Log("error creating new match");
			Application.LoadLevel("MyMatches");
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

	public IEnumerator RetrieveFriends(){
		//get count of matches waiting
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject> ("FriendRelationship").Include("friend").WhereEqualTo ("player", ParseUser.CurrentUser);
		var find = mainQuery.FindAsync ();	
		while (!find.IsCompleted) yield return null;
		IEnumerable<ParseObject> results = find.Result;

		int counter = 0;
		int initialY = 0;
		int newScrollContainerY = -990;
		var scrollContainer = GameObject.Find("FindMatchScrollContent").GetComponent<RectTransform>();
		scrollContainer.sizeDelta = new Vector2(200, 1980);
		
		foreach(ParseObject p in results){
			//Get opponentUserName name
			ParseUser playerFriend = p["friend"] as ParseUser;
			
			//Instantiate button
			GameObject challengeFriendButtonInstance = Instantiate(ChallengeFriendButton, new Vector3(0,initialY,0), transform.rotation) as GameObject;
			challengeFriendButtonInstance.transform.SetParent(GameObject.Find ("Friends").transform, false);
			Text challengeFriendButtonText = challengeFriendButtonInstance.GetComponentInChildren<Text>();
			//Set button text based on status
			challengeFriendButtonText.text = "Challenge " + playerFriend["displayName"];

			//Store match object and current opponentUserName on button
			ChallengeFriendButtonScript cfbs = challengeFriendButtonInstance.GetComponent<ChallengeFriendButtonScript>(); 
			cfbs.opponent = playerFriend;
				
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
	}



	private void addClickEvent(Button b)
	{
		b.onClick.AddListener(() => challengeFriendButtonClicked(b));
	}
	
	public void challengeFriendButtonClicked(Button b) 
	{
		ChallengeFriendButtonScript cfbs = b.GetComponent<ChallengeFriendButtonScript>();
		ParseUser playerChallenged = cfbs.opponent;
		Debug.Log("challenging " + playerChallenged.Username );
		Debug.Log("to be implemented");
		StartCoroutine("ChallengeFriend", playerChallenged);
	}

	public void Button_FindMatch_Clicked()
	{
		Debug.Log ("finding new match");
		StartCoroutine ("FindRandomMatch");
	}

	public void Button_Back_Clicked()
	{
		Application.LoadLevel ("MyMatches");
	}
}
