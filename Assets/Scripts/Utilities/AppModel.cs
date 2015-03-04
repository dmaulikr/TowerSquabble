using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

static public class AppModel {
	static public string currentOpponentUserName;
	static public string currentOpponentDisplayName;
	static public string currentUserName;
	static public string currentDisplayName;

	static public ParseObject currentMatch;
	static public IEnumerable<ParseBuildingBlock> currentBlocks;

	static public void LoginWithUser(ParseUser user)
	{
		AppModel.currentUserName = user.Username.ToString();
		AppModel.currentDisplayName = user["displayName"].ToString();
		Application.LoadLevel("MyMatches");
	}
}
