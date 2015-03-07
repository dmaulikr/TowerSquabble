using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parse;
using Facebook;
using Facebook.MiniJSON;

public class FacebookHelper : MonoBehaviour {

	public void Button_SignInFB_Clicked()
	{
		FB.Init(SetInit, OnHideUnity);
	}

	private void SetInit()
	{
		if (FB.IsLoggedIn) 
		{
			StartCoroutine("ParseLogin");
		}
		else
		{
			FBLogin();
		}
	}

	private void FBLogin()
	{
		FB.Login ("user_about_me", AuthCallback);
	}

	private void AuthCallback(FBResult result)
	{
		if (FB.IsLoggedIn) 
		{
			Debug.Log("user logged in successfully");
			//check if Parse is logged in
			if (ParseUser.CurrentUser == null) 
			{
				//if not, log in with Parse
				StartCoroutine("ParseLogin");
			} 
			else 
			{
				Debug.Log("Parse user is already logged in");
				Application.LoadLevel("MyMatches");
			}
		} 
		else 
		{
			//TODO: FB login failure
			Debug.Log("user is logged out from FB or operation failed");
		}
	}

	private IEnumerator ParseLogin() {
		if (FB.IsLoggedIn) 
		{
			//login with Parse
			var loginTask = ParseFacebookUtils.LogInAsync(FB.UserId, 
			                                              FB.AccessToken, 
			                                              DateTime.Now);
			while (!loginTask.IsCompleted) yield return null;
			//parse login completed, check results
			if (loginTask.IsFaulted || loginTask.IsCanceled) 
			{
				//TODO: Parse login failure
				foreach(var e in loginTask.Exception.InnerExceptions) 
				{
					ParseException parseException = (ParseException) e;
					Debug.Log("ParseLogin: error message " + parseException.Message);
					Debug.Log("ParseLogin: error code: " + parseException.Code);
				}
			} 
			else 
			{
				Debug.Log("successfully logged into parse with FB account");
				//call FB api to get facebook name and update parse user display name if it is new or modified
				FB.API("/me", HttpMethod.GET, FBAPICallback);
			}
		}
	}

	private void FBAPICallback(FBResult result)
	{
		if (!String.IsNullOrEmpty(result.Error)) {
			Debug.Log ("FBAPICallback: Error getting user info: + "+ result.Error);
			ParseUser.LogOut();
		} else {
			//got user profile info, extract name
			var resultObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			string name;
			object objectForKey;
			if (resultObject.TryGetValue("name", out objectForKey)) {
				name = (string)objectForKey;
			} else {
				name = "anonymous";
			}
			StartCoroutine("saveUserProfile", name);
		}
	}

	private IEnumerator saveUserProfile(string name) {
		var user = ParseUser.CurrentUser;
		user["displayName"] = name;
		//save if there have been any updates
		if (user.IsKeyDirty("displayName")) {
			var saveTask = user.SaveAsync();
			while (!saveTask.IsCompleted) yield return null;
			Debug.Log("successfully updated displayName");

			AppModel.LoginWithUser(ParseUser.CurrentUser);
		}
		else
		{
			AppModel.LoginWithUser(ParseUser.CurrentUser);
		}
	}

	private void OnHideUnity(bool isGameShown)
	{
		if(!isGameShown)
		{
			Time.timeScale = 0;
		}
		else
		{
			Time.timeScale = 1;
		}
	}
}
