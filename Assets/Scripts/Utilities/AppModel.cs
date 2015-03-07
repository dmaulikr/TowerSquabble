using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

static public class AppModel {
	static public ParseUser currentOpponent;

	static public ParseObject currentMatch;
	static public IEnumerable<ParseBuildingBlock> currentBlocks;

	static public void LoginWithUser(ParseUser user)
	{
		Application.LoadLevel("MyMatches");
	}
}
