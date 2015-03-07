using UnityEngine;
using System.Collections;
using Parse;

public class MatchButtonScript : MonoBehaviour {
	public string gameData;
	public string opponentUserName;
	public string opponentDisplayName;
	public ParseObject parseObject;
	public ParseUser opponent;
}
