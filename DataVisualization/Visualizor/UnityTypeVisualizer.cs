using System;
using UnityEditor;
using UnityEngine;

namespace DataTools
{
	internal class UnityTypeVisualizer : DataVisualizer
	{
		public override bool HasChildren()
		{
			return false;
		}

		public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
		{
			if (data is Color)
			{
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.ColorField(name, (Color) data));
			}
			else if (data is Vector2)
			{
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector2Field(name, (Vector2) data));
			}
			else if (data is Vector3)
			{
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector3Field(name, (Vector3) data));
			}
			else if (data is Vector4)
			{
				// TODO: Unity的Vector4Field实现是错误的，indentLevel 属性配合异常
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector4Field(name, (Vector4) data));
			}
			else if (data is Bounds)
			{
				// TODO: Unity的BoundsField实现是错误的，indentLevel 属性配合异常
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.BoundsField(name, (Bounds) data));
			}
			else if (data is AnimationCurve)
			{
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.CurveField(name, (AnimationCurve) data));
			}
			else if (data is Rect)
			{
				return ApplyValueIfNotEqual(ref data, EditorGUILayout.RectField(name, (Rect) data));
			}

			throw new NotImplementedException(type.ToString());
		}
	}
}