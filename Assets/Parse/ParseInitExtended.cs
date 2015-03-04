using UnityEngine;
using System.Collections;
using Parse;

public class ParseInitExtended : MonoBehaviour {
	void Awake()
	{
		ParseObject.RegisterSubclass<ParseBuildingBlock>();
	}
}
