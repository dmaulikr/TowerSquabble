using UnityEngine;
using System.Collections;

public static class MathUtilities 
{
	/* IsApproximately()
	 * Unity's version of Approximately is using Mathf.epsilon as the tolerance.
	 * In this version we can control the tolerance.
	 */
	public static bool IsApproximately(float a, float b) 
	{
		var tolerance = 0.00001f;
		return Mathf.Abs(a - b) < tolerance;
	}

	/* IsApproximately()
	 * Unity's version of Approximately is using Mathf.epsilon as the tolerance.
	 * In this version we can control the tolerance.
	 */
	public static bool IsApproximately(float a, float b, float tolerance) 
	{
		return Mathf.Abs(a - b) < tolerance;
	}
}
