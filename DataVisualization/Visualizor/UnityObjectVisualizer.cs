using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataTools
{
	internal class UnityObjectVisualizer : DataVisualizer
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
		{
			var obj = data as Object;
			if (obj != null)
				obj = EditorGUILayout.ObjectField(obj, type, true, null);
			else
				obj = EditorGUILayout.ObjectField(null, type, true, null);

			return ApplyValueIfNotEqual(ref data, obj);
		}

		public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
		{
			bool changed = false;
			var go = data as GameObject;
			if (go != null)
			{
				foreach (Component comp in go.GetComponents<Component>())
				{
					changed |= visualization.Inspect(comp.GetType().Name, path + "." + comp.GetType().Name, comp);
				}
			}

			foreach (
				FieldInfo fieldInfo in
					type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				changed |= InspectField(visualization, path, ref data, fieldInfo);
			}

			foreach (
				PropertyInfo propertyInfo in
					type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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

		private static bool InspectProperty(DataVisualization visualization, string path, ref object data,
			PropertyInfo propertyInfo)
		{
			if (propertyInfo.CanRead && CanInspect(propertyInfo))
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
				EditorGUILayout.LabelField(propertyInfo.Name, "unreadable");
				return false;
			}
		}

		private static bool CanInspect(PropertyInfo p)
		{
			if (p == null)
				return false;
			var attrs = p.GetCustomAttributes(typeof (ObsoleteAttribute), false);
			if (attrs.Length == 0)
				return true;
			return false;
		}
	}
}