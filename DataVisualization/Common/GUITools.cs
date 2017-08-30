using System;
using UnityEditor;
using UnityEngine;

namespace FF.Editor
{
	public static class GUITools
	{
		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick)
		{
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 16f, 16f);
			return EditorGUI.Foldout(rect, foldout, content, toggleOnLabelClick);
		}

		public static IDisposable Indent()
		{
			return new GUIEditorIndent();
		}

		public static IDisposable Scroll(ref Vector2 scroll)
		{
			return new GUIScroll(ref scroll);
		}

		private class GUIEditorIndent : IDisposable
		{
			public GUIEditorIndent()
			{
				++EditorGUI.indentLevel;
			}

			public void Dispose()
			{
				--EditorGUI.indentLevel;
			}
		}

		private class GUIScroll : IDisposable
		{
			public GUIScroll(ref Vector2 scroll)
			{
				scroll = EditorGUILayout.BeginScrollView(scroll);
			}

			public void Dispose()
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}
}