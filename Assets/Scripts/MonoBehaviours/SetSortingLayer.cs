using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour
{
	public string sortingLayerName;		// The name of the sorting layer the particles should be set to.
	public int orderInLayer;
	public float simulateTime = 0F;		// Only used if positive value is assigned to it
	
	void Start ()
	{
		// Set the sorting layer & order in layer of the particle system.
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = sortingLayerName;
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = orderInLayer;

		// Set it for all the children as well
		ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < ps.Length; ++i)
		{
			ps[i].GetComponent<Renderer>().sortingLayerName = sortingLayerName;
			ps[i].GetComponent<Renderer>().sortingOrder = orderInLayer;
		}

		if(simulateTime > 0F)
		{
			GetComponent<ParticleSystem>().Simulate(simulateTime);
			GetComponent<ParticleSystem>().Play();
		}
	}
}
