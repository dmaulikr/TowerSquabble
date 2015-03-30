using UnityEngine;
using System.Collections;

public class BuildingBlock : MonoBehaviour 
{
	// Parse varibles
	public string objectId;
	//public enum BuildingBlockType { none = -1, crate = 0, triangle = 1, circle = 2, pillar = 3 };
	public ParseBuildingBlock.Type buildingBlockType;
	// An index that keeps track of the order of when blocks were put in the scene (do not modify in this script)
	public int index = 0;

	// Flashing particle ring
	public GameObject flashingRing;
	// Release boundaries game object
	public GameObject releaseBoundaries;

	// Flags
	private bool isReleased = false;
	private bool notReleasedAndTouchingCollider = true;
	// Is this building block held at start or not (default to true)
	public bool isHeldAtStart = true;

	// Swing varibles
	public float swingTime = 1.5F;
	private float lastSwing;
	private bool firstSwing = true;
	// Varible telling us if the building block should swing or not (at all)
	public bool shouldSwing = false;

	// Refrence to the level manager
	private MatchManager matchManager;

	// Accessors
	public bool NotReleasedAndTouchingCollider { get { return notReleasedAndTouchingCollider; } set { notReleasedAndTouchingCollider = value; } }

	private void Release()
	{
		SetGravityScale(1F);
		Enable2DCollider(true);
		flashingRing.GetComponent<Renderer>().enabled = false;
		if(transform.parent != null)
		{
			// Destroy hinge joint
			Destroy(gameObject.GetComponent<HingeJoint2D>());
			// Destroy parent, but not this game object
			GameObject parent;
			parent = transform.parent.gameObject;
			transform.parent = null;
			Destroy(parent);
		}
		// Destroy release boundaries game object
		if(releaseBoundaries != null)
		{
			Destroy(releaseBoundaries);
		}
	}

	public bool IsReleased()
	{
		return isReleased;
	}
	
	// Set if this building block is released or not
	public void SetIsReleased(bool release)
	{
		isReleased = release;
	}

	// Enable the 2D collider of this game object
	void Enable2DCollider(bool enable)
	{
		gameObject.GetComponent<Collider2D>().enabled = enable;
	}

	// Set gravity scale of the 2D rigidbody
	void SetGravityScale(float fValue)
	{
		gameObject.GetComponent<Rigidbody2D>().gravityScale = fValue;
	}

	public void DestroyToolObject()
	{
		// Destroy the object.
		Destroy (gameObject);
	}
	
	public void AddForceToTool(Vector2 V2ForceToAdd)
	{
		V2ForceToAdd.y = 0F;
		gameObject.GetComponent<Rigidbody2D>().AddForce(V2ForceToAdd * 10F);
	}

	// Make the building block swin back and forth using a hinge joint
	public void Swing()
	{
		if(!shouldSwing)
		{
			return;
		}

		if(gameObject.GetComponent<HingeJoint2D>() != null)
		{
			if(lastSwing + swingTime < Time.time)
			{
				JointMotor2D motor = gameObject.GetComponent<HingeJoint2D>().motor;	
				motor.motorSpeed *= -1F;
				gameObject.GetComponent<HingeJoint2D>().motor = motor;
				lastSwing = Time.time;
				if(firstSwing) // After the first swing we want to double the swing time
				{
					swingTime *= 2;
					firstSwing = false;
				}
			}
		}
	}

	// Use this for initialization
	void Start () 
	{
		// Find the match manager and make the reference point to it
		matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();

		if(isHeldAtStart)
		{
			SetGravityScale(0F);
			Enable2DCollider(false);
			flashingRing.GetComponent<Renderer>().enabled = true;
			SetIsReleased(false);
			
			// Set color to white for flashing ring
			Color whiteAlpha = Color.white;
			whiteAlpha.a = .4F;
			flashingRing.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.color = whiteAlpha;

			if(shouldSwing)
			{
				// Swing stuff
				lastSwing = Time.time;
				swingTime /= 2; // We set swing time to half for the first swing
				firstSwing = true;
			}
			else
			{
				gameObject.GetComponent<HingeJoint2D>().useMotor = false;
			}
		}
		else
		{
			Release();
		}
	}

	// Update is called once per frame
	void Update() 
	{
		if(isReleased)
		{
			Release();
		}
		else
		{
			Swing();
		}

		// Set flashing ring color depending on if it is touching a collider or not
		if(notReleasedAndTouchingCollider) 
		{
			Color colWhiteAlpha = Color.red;
			colWhiteAlpha.a = .4F;
			flashingRing.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.color = colWhiteAlpha;
		} 
		else 
		{
			Color colWhiteAlpha = Color.white;
			colWhiteAlpha.a = .4F;
			flashingRing.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.color = colWhiteAlpha;
		}
	}
}
