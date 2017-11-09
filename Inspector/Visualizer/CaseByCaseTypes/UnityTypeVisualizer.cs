using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DataInspector
{
	internal class UnityTypeVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return GUITools.IsInEditor() == false;
		}

		public override bool AlwaysShowChildren()
		{
			return GUITools.IsInEditor() == false;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			if (GUITools.IsInEditor())
			{
#if UNITY_EDITOR
				if (data is Color)
				{
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.ColorField(name, (Color)data));
				}
				else if (data is Vector2)
				{
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector2Field(name, (Vector2)data));
				}
				else if (data is Vector3)
				{
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector3Field(name, (Vector3)data));
				}
				else if (data is Vector4)
				{
					// TODO: Unity的Vector4Field实现是错误的，indentLevel 属性配合异常
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.Vector4Field(name, (Vector4)data));
				}
				else if (data is Bounds)
				{
					// TODO: Unity的BoundsField实现是错误的，indentLevel 属性配合异常
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.BoundsField(name, (Bounds)data));
				}
				else if (data is AnimationCurve)
				{
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.CurveField(name, (AnimationCurve)data));
				}
				else if (data is Rect)
				{
					return ApplyValueIfNotEqual(ref data, EditorGUILayout.RectField(name, (Rect)data));
				}
#endif
				throw new NotImplementedException(type.ToString());
			}
			else
			{
				if (data is Bounds || data is AnimationCurve || data is Rect)
				{
					GUITools.LabelField(name, data.ToString());
				}
				else
				{
					GUITools.LabelField(name);
				}
				return false;
			}
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			if (GUITools.IsInEditor())
			{
				return base.InspectChildren(inspector, path, ref data, type);
			}
			else
			{
				return inspector.GetSpecialVisualizer(Inspector.SpecialVisualizer.Composite)
					.InspectChildren(inspector, path, ref data, type);
			}
		}
	}
}