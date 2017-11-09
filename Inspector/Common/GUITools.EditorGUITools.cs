#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataInspector
{
	public static partial class GUITools
	{
		private class EditorGUITools : IGUITools
		{
			public bool Foldout(bool foldout, string content)
			{
				var rect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 16f, 16f);
				return EditorGUI.Foldout(rect, foldout, content, true);
			}

			public void LabelField(string label)
			{
				EditorGUILayout.LabelField(label);
			}

			public void LabelField(string label, string label2)
			{
				EditorGUILayout.LabelField(label, label2);
			}

			public int IntField(string label, int value)
			{
				return EditorGUILayout.DelayedIntField(label, value);
			}

			public long LongField(string label, long value)
			{
				return EditorGUILayout.LongField(label, value);
			}

			public Enum EnumMaskField(string label, Enum enumValue)
			{
				return EditorGUILayout.EnumMaskField(label, enumValue);
			}

			public Enum EnumPopup(string label, Enum enumValue)
			{
				return EditorGUILayout.EnumPopup(label, enumValue);
			}

			public string TextField(string text)
			{
				return EditorGUILayout.TextField(text);
			}

			public string TextField(string label, string text)
			{
				return EditorGUILayout.TextField(label, text);
			}

			public Object ObjectField(Object obj, Type objType)
			{
				return EditorGUILayout.ObjectField(obj, objType, true);
			}

			public bool Toggle(string label, bool value)
			{
				return EditorGUILayout.Toggle(label, value);
			}

			public float FloatField(string label, float value)
			{
				return EditorGUILayout.FloatField(label, value);
			}

			public double DoubleField(string label, double value)
			{
				return EditorGUILayout.DoubleField(label, value);
			}

			public void SetLabelWidth(int labelWidth)
			{
				GUITools.labelWidth = labelWidth;
				EditorGUIUtility.labelWidth = labelWidth;
			}

			public int GetIndentLevel()
			{
				return EditorGUI.indentLevel;
			}

			public IDisposable Indent()
			{
				return new GUIEditorIndent();
			}

			private class GUIEditorIndent : IDisposable
			{
				public GUIEditorIndent()
				{
					++EditorGUI.indentLevel;
					++indentLevel;
				}

				public void Dispose()
				{
					--EditorGUI.indentLevel;
					--indentLevel;
				}
			}
		}
	}
}
#endif