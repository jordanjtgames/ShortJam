using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curves : MonoBehaviour
{
	public static float EaseOutCubic(float x)
	{
		return 1f - Mathf.Pow(1f - x, 3f);
	}
	
	public static float EaseInOutQuint(float x) {
		return x < 0.5f ? 16f * x * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
	}
}
