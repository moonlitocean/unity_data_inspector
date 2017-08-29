using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal class GameObjectVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		var obj = data as Object;
		if (obj != null)
		{
			EditorGUILayout.LabelField(string.Format("{0}[{1}]", obj.name, data.GetType().Name));
		}
		return false;
	}

	private bool CanInspect(PropertyInfo p)
	{
		if (p == null)
			return false;
		var attrs = p.GetCustomAttributes(typeof(ObsoleteAttribute), false);
		if (attrs.Length == 0)
			return true;
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		var go = data as GameObject;

		bool changed = false;
		foreach (Component comp in go.GetComponents<Component>())
		{
			changed |= visualization.Inspect(comp.GetType().Name, path + "." + comp.GetType().Name, comp);
		}

		foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (propertyInfo.CanRead && CanInspect(propertyInfo))
			{
				object value = propertyInfo.GetValue(go, null);
				if (value is Component)
					continue;

				changed |= visualization.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, value);
			}
			else
			{
				changed |= visualization.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, "unreadable");
			}
		}
		return changed;
	}
}