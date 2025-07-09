using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataInspector
{
	public static partial class GUITools
	{
#if UNITY_EDITOR
		private static readonly IGUITools editorTools = new EditorGUITools();
#endif
		private static readonly IGUITools gameTools = new GameGUITools();
		private static int indentLevel;
		private static int indentOffset = 15;
		private static int toggleExtraIndent = 7;
		private static int labelWidth;
		private static IGUITools tools = gameTools;
		private static FieldInfo cached_s_SkinMode;

		internal interface IGUITools
		{
			bool Foldout(bool foldout, string content);
			void LabelField(string label);
			void LabelField(string label, string label2);
			int IntField(string label, int value);
			long LongField(string label, long value);
			Enum EnumMaskField(string label, Enum enumValue);
			Enum EnumPopup(string label, Enum enumValue);
			string TextField(string text);
			string TextField(string label, string text);
			Object ObjectField(Object obj, Type objType);
			bool Toggle(string label, bool value);
			float FloatField(string label, float value);
			double DoubleField(string label, double value);
			void SetLabelWidth(int labelWidth);
			int GetIndentLevel();
			IDisposable Indent();
		}

		public static void Setup()
		{
#if UNITY_EDITOR
			if (IsInEditor())	tools = editorTools;
			else				tools = gameTools;
#endif
		}

		public static bool IsInEditor()
		{
			if(cached_s_SkinMode == null)
				cached_s_SkinMode = typeof(GUIUtility).GetField("s_SkinMode", BindingFlags.NonPublic | BindingFlags.Static);
			return cached_s_SkinMode != null && (int)cached_s_SkinMode.GetValue(null) != 0;
		}

		public static IDisposable Scroll(ref Vector2 scroll)
		{
			return new GUIScroll(ref scroll);
		}

		public static IDisposable HorizontalScope()
		{
			return new GUILayout.HorizontalScope();
		}

		public static IDisposable HorizontalScope(int width)
		{
			return new GUILayout.HorizontalScope(GUILayout.Width(width));
		}

		public static IDisposable HorizontalScope(GUIStyle style)
		{
			return new GUILayout.HorizontalScope(style);
		}

		public static IDisposable VerticalScope()
		{
			return new GUILayout.VerticalScope();
		}

		public static IDisposable VerticalScope(int width)
		{
			return new GUILayout.VerticalScope(GUILayout.Width(width));
		}

		public static IDisposable VerticalScope(GUIStyle style)
		{
			return new GUILayout.VerticalScope(style);
		}

		public static IDisposable VerticalScope(GUIStyle style, params GUILayoutOption[] options)
		{
			return new GUILayout.VerticalScope(style, options);
		}

		public static IDisposable Color(Color color)
		{
			return new GUIColor(color);
		}

		public static IDisposable Enabled(bool enabled)
		{
			return new GUIEnabled(enabled);
		}

		public static bool Foldout(bool foldout, string content)		{ return tools.Foldout(foldout, content);}
		public static void LabelField(string label)						{ tools.LabelField(label);}
		public static void LabelField(string label, string label2)		{ tools.LabelField(label, label2);}
		public static int IntField(string label, int value)				{ return tools.IntField(label, value);}
		public static long LongField(string label, long value)			{ return tools.LongField(label,value);}
		public static Enum EnumMaskField(string label, Enum enumValue)	{ return tools.EnumMaskField(label, enumValue);}
		public static Enum EnumPopup(string label, Enum enumValue)		{ return tools.EnumPopup(label, enumValue);}
		public static string TextField(string text)						{ return tools.TextField(text);}
		public static string TextField(string label, string text)		{ return tools.TextField(label, text);}
		public static Object ObjectField(Object obj, Type objType)		{ return tools.ObjectField(obj, objType);}
		public static bool Toggle(string label, bool value)				{ return tools.Toggle(label, value);}
		public static float FloatField(string label, float value)		{ return tools.FloatField(label, value);}
		public static double DoubleField(string label, double value)	{ return tools.DoubleField(label, value);}
		public static void SetLabelWidth(int labelWidth)				{ tools.SetLabelWidth(labelWidth);}
		public static int GetIndentLevel()								{ return tools.GetIndentLevel();}
		public static IDisposable Indent()								{ return tools.Indent();}

		//////////////////////////////////////////////////////////////////////////////////////
		//
		private class GUIScroll : IDisposable
		{
			public GUIScroll(ref Vector2 scroll)
			{
				scroll = GUILayout.BeginScrollView(scroll);
			}

			public void Dispose()
			{
				GUILayout.EndScrollView();
			}
		}

		private class GUIColor : IDisposable
		{
			private Color orgColor;
			public GUIColor(Color color)
			{
				orgColor = GUI.color;
				GUI.color = color;
			}

			public void Dispose()
			{
				GUI.color = orgColor;
			}
		}

		private class GUIEnabled: IDisposable
		{
			private bool orgEnable;
			public GUIEnabled(bool enabled)
			{
				orgEnable = GUI.enabled;
				GUI.enabled = enabled;
			}

			public void Dispose()
			{
				GUI.enabled = orgEnable;
			}
		}
	}
}