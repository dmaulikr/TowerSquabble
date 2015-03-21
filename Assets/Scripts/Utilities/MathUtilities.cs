using UnityEngine;
using System.Collections;

public static class MathUtilities 
{
	/* IsApproximately()
	 * Unities version of Approximately is using Mathf.epsilon as the tolerance.
	 * In this version we can control the tolerance.
	 */
	public static bool IsApproximately(float a, float b) 
	{
		var tolerance = 0.0001f;
		return Mathf.Abs(a - b) < tolerance;
	}
}
