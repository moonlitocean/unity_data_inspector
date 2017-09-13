using System;
using UnityEditor;

internal class PrimitiveVisualizer : DataVisualizer
{
	// Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
	public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
	{
		object result;
		if (data == null)
		{
			EditorGUILayout.LabelField(name, "null");
			result = null;
		}
		else if (data is bool)
		{
			result = EditorGUILayout.Toggle(name, (bool)data);
		}
		else if (data is byte)
		{
			result = (byte)EditorGUILayout.IntField(name, (byte)data);
		}
		else if (data is sbyte)
		{
			result = (sbyte)EditorGUILayout.IntField(name, (sbyte)data);
		}
		else if (data is short)
		{
			result = (short)EditorGUILayout.IntField(name, (short)data);
		}
		else if (data is ushort)
		{
			result = (ushort) EditorGUILayout.IntField(name, (ushort) data);
		}
		else if (data is int)
		{
			result = EditorGUILayout.IntField(name, (int) data);
		}
		else if (data is uint)
		{
			EditorGUILayout.LongField(name, (uint) data);
			result = data;
		}
		else if (data is long)
		{
			result = ParseLong(EditorGUILayout.TextField(name, data.ToString()), (long) data);
		}
		else if (data is ulong)
		{
			result = ParseULong(EditorGUILayout.TextField(name, data.ToString()), (ulong) data);
		}
		else if (data is IntPtr)
		{
			result = (IntPtr)ParseLong(EditorGUILayout.TextField(name, data.ToString()), (long)data);
		}
		else if (data is UIntPtr)
		{
			result = (UIntPtr)ParseULong(EditorGUILayout.TextField(name, data.ToString()), (ulong) data);
		}
		else if (data is char)
		{
			result = ParseChar(EditorGUILayout.TextField(name, data.ToString()), (char)data);
		}
		else if (data is float)
		{
			result = EditorGUILayout.FloatField(name, (float) data);
		}
		else if (data is double)
		{
			result = EditorGUILayout.DoubleField(name, (double) data);
		}
		else
		{
			throw new NotImplementedException(type.Name);
		}

		return ApplyValueIfNotEqual(ref data, result);
	}

	private long ParseLong(string text, long defaultLong)
	{
		try
		{
			return long.Parse(text);
		}
		catch (Exception)
		{
			return defaultLong;
		}
	}

	private ulong ParseULong(string text, ulong defaultLong)
	{
		try
		{
			return ulong.Parse(text);
		}
		catch (Exception)
		{
			return defaultLong;
		}
	}

	private char ParseChar(string text, char defaultChar)
	{
		if (!string.IsNullOrEmpty(text))
			return text[0];
		else
			return defaultChar;
	}
}