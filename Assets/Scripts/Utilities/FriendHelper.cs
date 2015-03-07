using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class FriendHelper : MonoBehaviour {
	
	private static FriendHelper mInstance = null;
	
	private static FriendHelper instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = GameObject.FindObjectOfType(typeof(FriendHelper)) as FriendHelper;
				
				if (mInstance == null)
				{
					mInstance = new GameObject("FriendHelper").AddComponent<FriendHelper>();
				}
			}
			return mInstance;
		}
	}
	
	void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this as FriendHelper;
		}
	}
	
	IEnumerator Perform(IEnumerator coroutine)
	{
		yield return StartCoroutine(coroutine);
		Die();
	}
	 
	public static void DoCoroutine(IEnumerator coroutine)
	{
		instance.StartCoroutine(instance.Perform(coroutine)); //this will launch the coroutine on our instance
	}
	
	void Die()
	{
		mInstance = null;
		Destroy(gameObject);
	}
	
	void OnApplicationQuit()
	{
		mInstance = null;
	}

	public static IEnumerator AddFriend(string userName)
	{
		ParseQuery<ParseUser> mainQuery = new ParseQuery<ParseUser> ("_User").WhereEqualTo ("username",userName);
		var findUser = mainQuery.FindAsync ();	
		while (!findUser.IsCompleted) yield return null;
		IEnumerable<ParseUser> results = findUser.Result;
		
		foreach (ParseUser user in results) 
		{
			ParseObject newFR = new ParseObject("FriendRelationship");
			newFR["player"] = ParseUser.CurrentUser;
			newFR["friend"] = user;
			var addFriend = newFR.SaveAsync();
			while (!addFriend.IsCompleted) yield return null;
			if(!addFriend.IsCanceled && !addFriend.IsFaulted)
			{
				Debug.Log("successfully added " + userName + " as a friend");
			}
			else
			{
				Debug.Log("something went wrong adding + " + userName + " as a friend");
			}
		}
	}

	public static IEnumerator RetrieveFriends(){
		//get count of matches waiting
		ParseQuery<ParseObject> mainQuery = new ParseQuery<ParseObject> ("FriendRelationship").WhereEqualTo ("player", ParseUser.CurrentUser);
		var find = mainQuery.FindAsync ();	
		while (!find.IsCompleted) yield return null;
		IEnumerable<ParseObject> results = find.Result;

		foreach (ParseObject p in results) 
		{
			Debug.Log(p);
		}
	}
}
