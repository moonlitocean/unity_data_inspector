using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

internal class UnityObjectVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(string name, ref object data, Type type)
	{
		var obj = data as Object;
		if (obj != null)
		{
			EditorGUILayout.ObjectField(string.Format("{0}[{1}]", obj.name, data.GetType().Name), obj, obj.GetType(), true, null);
		}
		return false;
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		bool changed = false;
		foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
		{
			changed |= InspectField(visualization, path, ref data, fieldInfo);
		}

		foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			changed |= InspectProperty(visualization, path, ref data, propertyInfo);
		}
		return changed;
	}

	private static bool InspectField(DataVisualization visualization, string path, ref object data, FieldInfo fieldInfo)
	{
		object value = fieldInfo.GetValue(data);
		bool changed = false;
		object newValue = null;
		if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
			visualization.Inspect(fieldInfo.Name, path + "." + fieldInfo.Name, value, fieldInfo.FieldType);
		else
			visualization.Inspect(fieldInfo.Name, path + "." + fieldInfo.Name, value, fieldInfo.FieldType, null,
				v =>
				{
					changed = true;
					newValue = v;
				});

		if (changed)
		{
			fieldInfo.SetValue(data, newValue);
		}
		return changed;
	}

	private static bool InspectProperty(DataVisualization visualization, string path, ref object data, PropertyInfo propertyInfo)
	{
		if (propertyInfo.CanRead)
		{
			object value;
			try
			{
				value = propertyInfo.GetValue(data, null);
			}
			catch (Exception)
			{
				return false;
			}

			return visualization.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, value);
		}
		else
		{
			return visualization.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, "unreadable");
		}
	}
}