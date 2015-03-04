using System;
using UnityEngine;
using Parse;

[ParseClassName("ParseBuildingBlock")]
public class ParseBuildingBlock : ParseObject 
{

	public ParseBuildingBlock()
	{
	}
	
	public enum Type
	{ 
		none = -1,
		crate = 0,
		triangle = 1,
		circle = 2,
		pillar = 3
	};

	[ParseFieldName ("matchId")]
	public string matchId
	{
		get { return GetProperty<string>("matchId"); }
		set { SetProperty<string>(value, "matchId"); }
	}

	[ParseFieldName ("posX")]
	public float posX
	{
		get { return GetProperty<float>("posX"); }
		set { SetProperty<float>(value, "posX"); }
	}

	[ParseFieldName ("posY")]
	public float posY
	{
		get { return GetProperty<float>("posY"); }
		set { SetProperty<float>(value, "posY"); }
	}

	[ParseFieldName ("rotZ")]
	public float rotZ
	{
		get { return GetProperty<float>("rotZ"); }
		set { SetProperty<float>(value, "rotZ"); }
	}

	//No direct use, should be private
	[ParseFieldName ("type")]
	public string type
	{
		get { return GetProperty<string>("type"); }
		set { SetProperty<string>(value, "type"); }
	}

	public Type GetBuildingBlockType(){
		Type t = (Type)Enum.Parse (typeof(Type), type);
		return t;
	}

	public void SetBuildingBlockType(Type t){
		type = t.ToString();
	}

	[ParseFieldName ("index")]
	public int index
	{
		get { return GetProperty<int>("index"); }
		set { SetProperty<int>(value, "index"); }
	}
}
