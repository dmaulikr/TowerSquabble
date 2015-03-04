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

	// Flashing ring particle
	public GameObject flashingRing;
	// Release boundaries game object
	public GameObject releaseBoundaries;

	// Flags
	private bool isReleased = false;
	private bool notReleasedAndTouchingCollider = true;
	// Is this building block held at start or not (default to true)
	public bool isHeldAtStart = true;
	// Has the building block collided with any other collider?
	private bool hasCollided = false;

	// Swing varibles
	public float swingTime = 1.5F;
	private float lastSwing;
	private bool firstSwing = true;
	// Varible telling us if the building block should swing or not (at all)
	public bool shouldSwing = false;

	// Accessors
	public bool HasCollided { get { return hasCollided; } set { hasCollided = value; } }
	public bool NotReleasedAndTouchingCollider { get { return notReleasedAndTouchingCollider; } set { notReleasedAndTouchingCollider = value; } }

	private void Release()
	{
		SetGravityScale(1F);
		Enable2DCollider(true);
		flashingRing.renderer.enabled = false;
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
		gameObject.collider2D.enabled = enable;
	}

	// Set gravity scale of the 2D rigidbody
	void SetGravityScale(float fValue)
	{
		gameObject.rigidbody2D.gravityScale = fValue;
	}

	public void DestroyToolObject()
	{
		// Destroy the object.
		Destroy (gameObject);
	}
	
	public void AddForceToTool(Vector2 V2ForceToAdd)
	{
		V2ForceToAdd.y = 0F;
		gameObject.rigidbody2D.AddForce(V2ForceToAdd * 10F);
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

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "BuildingBlock")
		{
			float minForce = 10f;
			float decreaseVelocityBy = 2f;
			float otherMass; // other object's mass
			if (collision.rigidbody)
				otherMass = collision.rigidbody.mass;
			else 
				otherMass = 1000; // static collider means huge mass
			Vector2 force = collision.relativeVelocity * otherMass;
			if(Mathf.Abs(force.x) < minForce || Mathf.Abs(force.y) < minForce)
			{
				Vector2 velo = gameObject.GetComponent<Rigidbody2D>().velocity;
				if(velo.x > 0f)
				{
					float newVel = Mathf.Clamp(velo.x - decreaseVelocityBy, 0f, 100000f);
					velo.x = newVel;
				}
				else
				{
					float newVel = Mathf.Clamp(velo.x + decreaseVelocityBy, -100000f, 0f);
					velo.x = newVel;
				}

				if(velo.y > 0f)
				{
					float newVel = Mathf.Clamp(velo.y - decreaseVelocityBy, 0f, 100000f);
					velo.y = newVel;
				}
				else
				{
					float newVel = Mathf.Clamp(velo.y + decreaseVelocityBy, -100000f, 0f);
					velo.y = newVel;
				}
				gameObject.GetComponent<Rigidbody2D>().velocity = velo;

				//gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0f;
			}
		}
	}

	// Use this for initialization
	void Start () 
	{
		if(isHeldAtStart)
		{
			SetGravityScale(0F);
			Enable2DCollider(false);
			flashingRing.renderer.enabled = true;
			SetIsReleased(false);
			
			// Set color to white for flashing ring
			Color whiteAlpha = Color.white;
			whiteAlpha.a = .4F;
			flashingRing.particleSystem.renderer.material.color = whiteAlpha;

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

	void OnCollision2DEnter()
	{
		if(!hasCollided)
		{
			hasCollided = true;
		}
	}

	void FixedUpdate()
	{
		if(rigidbody2D.velocity.magnitude < .001f && rigidbody2D.angularVelocity < .001f && hasCollided)
		{
			rigidbody2D.Sleep();
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
			flashingRing.particleSystem.renderer.material.color = colWhiteAlpha;
		} 
		else 
		{
			Color colWhiteAlpha = Color.white;
			colWhiteAlpha.a = .4F;
			flashingRing.particleSystem.renderer.material.color = colWhiteAlpha;
		}
	}
}
