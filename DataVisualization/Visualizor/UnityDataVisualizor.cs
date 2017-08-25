using System;
using UnityEditor;
using UnityEngine;

internal class UnityDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		if (data is Color)
		{
			return ApplyData(ref data, EditorGUILayout.ColorField(name, (Color)data));
		}
		else if(data is Vector2)
		{
			return ApplyData(ref data,  EditorGUILayout.Vector2Field(name, (Vector2)data));
		}
		else if (data is Vector3)
		{
			return ApplyData(ref data,  EditorGUILayout.Vector3Field(name, (Vector3)data));
		}
		else if (data is Vector4)
		{
			return ApplyData(ref data,  EditorGUILayout.Vector4Field(name, (Vector4)data));
		}
		else if (data is Bounds)
		{
			return ApplyData(ref data,  EditorGUILayout.BoundsField(name, (Bounds)data));
		}
		else if (data is AnimationCurve)
		{
			return ApplyData(ref data,  EditorGUILayout.CurveField(name, (AnimationCurve)data));
		}
		else if (data is Rect)
		{
			return ApplyData(ref data,  EditorGUILayout.RectField(name, (Rect)data));
		}
		
		throw new NotImplementedException(type.ToString());
	}
}