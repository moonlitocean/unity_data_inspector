using System;
using UnityEditor;

internal class AtomDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		object result;
		EditorGUIUtility.labelWidth = DataVisualization.LabelWidth;
		if (data == null)
		{
			EditorGUILayout.LabelField(name, "null");
			result = null;
		}
		else if (data is int)
		{
			result = EditorGUILayout.IntField(name, (int)data);
		}
		else if (data is byte)
		{
			result = (byte)EditorGUILayout.IntField(name, (byte)data);
		}
		else if (data is long)
		{
			result = ParseLong(EditorGUILayout.TextField(name, data.ToString()), (long)data);
		}
		else if (data is float)
		{
			result = EditorGUILayout.FloatField(name, (float)data);
		}
		else if (data is double)
		{
			result = EditorGUILayout.DoubleField(name, (double) data);
		}
		else if (data is string)
		{
			result = EditorGUILayout.TextField(name, (string)data);
		}
		else if (data is bool)
		{
			result = EditorGUILayout.Toggle(name, (bool)data);
		}
		else if (data is IntPtr)
		{
			EditorGUILayout.LongField(name, ((IntPtr)data).ToInt64());
			result = data;
		}
		else if (data is uint)
		{
			EditorGUILayout.LongField(name, ((uint) data));
			result = data;
		}
		else
		{
			throw new NotImplementedException(type.Name);
		}

		return ApplyData(ref data, result);
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
}