using UnityEngine;
using System.Collections;

public static class RoundFloat{

	public static float RoundToFloat(float num,float fl)
	{
		return Mathf.Round(num/fl)*fl;
	}

	public static float FloorToFloat(float num,float fl)
	{
		return Mathf.Floor(num/fl)*fl;
	}

	public static float CeilToFloat(float num,float fl)
	{
		return Mathf.Ceil(num/fl)*fl;
	}

}
