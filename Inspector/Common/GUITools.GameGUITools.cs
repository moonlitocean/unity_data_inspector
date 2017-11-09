using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataInspector
{
	public static partial class GUITools
	{
		private class GameGUITools : IGUITools
		{
			public bool Foldout(bool foldout, string content)
			{
				using (HorizontalScope())
				{
					ShowIndent();
					if (GUILayout.Button(foldout ? "-" : "+ ", GUI.skin.label, GUILayout.Width(10)) ||
						GUILayout.Button(content, GUI.skin.label))
					{
						foldout = !foldout;
					}
					return foldout;
				}
			}

			public void LabelField(string label)
			{
				using (HorizontalScope())
				{
					LayoutIndentedLabel(label);
				}
			}

			public void LabelField(string label, string label2)
			{
				using (HorizontalScope())
				{
					LayoutIndentedLabel(label);
					GUILayout.Label(label2);

				}
			}

			public int IntField(string label, int value)
			{
				return PrimitiveField(label, value, Int32.Parse, 0);
			}

			public long LongField(string label, long value)
			{
				return PrimitiveField(label, value, Int64.Parse, 0);
			}

			public Enum EnumMaskField(string label, Enum enumValue)
			{
				LabelField(label, enumValue.ToString());
				return enumValue;
			}

			public Enum EnumPopup(string label, Enum enumValue)
			{
				LabelField(label, enumValue.ToString());
				return enumValue;
			}

			public string TextField(string text)
			{
				return GUILayout.TextField(text);
			}

			public string TextField(string label, string text)
			{
				using (HorizontalScope())
				{
					LayoutIndentedLabel(label);
					if (text == null)
					{
						GUILayout.Label("null");
						return null;
					}
					return GUILayout.TextField(text);
				}
			}

			public Object ObjectField(Object obj, Type objType)
			{
				LabelField(obj != null ? obj.name : "-");
				return obj;
			}

			public bool Toggle(string label, bool value)
			{
				using (HorizontalScope())
				{
					GUILayout.Space(indentLevel * indentOffset + toggleExtraIndent);
					return GUILayout.Toggle(value, label);
				}
			}

			public float FloatField(string label, float value)
			{
				return PrimitiveField(label, value, float.Parse, 0);
			}

			public double DoubleField(string label, double value)
			{
				return PrimitiveField(label, value, double.Parse, 0);
			}

			public void SetLabelWidth(int labelWidth)
			{
				GUITools.labelWidth = labelWidth;
			}

			public int GetIndentLevel()
			{
				return indentLevel;
			}

			public IDisposable Indent()
			{
				return new GUIEditorIndent();
			}

			private T PrimitiveField<T>(T value, Func<string, T> parse, T @default)
			{
				try
				{
					return parse(TextField(value.ToString()));
				}
				catch (Exception)
				{
					return @default;
				}
			}

			private T PrimitiveField<T>(string label, T value, Func<string, T> parse, T @default)
			{
				using (HorizontalScope())
				{
					LayoutIndentedLabel(label);
					return PrimitiveField(value, parse, @default);
				}
			}

			private void LayoutIndentedLabel(string label)
			{
				ShowIndent();
				GUILayout.Label(label, GUILayout.Width(labelWidth - indentLevel * indentOffset));
			}

			private void ShowIndent()
			{
				GUILayout.Box("", GUI.skin.label, GUILayout.Width(indentLevel * indentOffset));
			}

			//////////////////////////////////////////////////////////////////////////////////////
			// 
			private class GUIEditorIndent : IDisposable
			{
				public GUIEditorIndent()
				{
					++indentLevel;
				}

				public void Dispose()
				{
					--indentLevel;
				}
			}
		}
	}
}