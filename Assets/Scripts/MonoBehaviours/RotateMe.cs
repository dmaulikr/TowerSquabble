using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour 
{
	public float speed = 3.0f;
	public bool rotateLeft = true;
	private Vector3 rot;

	void Start()
	{
		if(rotateLeft)
			rot = Vector3.forward;
		else
			rot = Vector3.back;
	}

	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(rot * Time.deltaTime * speed);	
	}
}
