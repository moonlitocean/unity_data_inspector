using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DataInspector
{
	internal class UnityObjectVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var obj = data as Object;
			if (obj != null)
				obj = GUITools.ObjectField(obj, type);
			else
				obj = GUITools.ObjectField(null, type);

			return ApplyValueIfNotEqual(ref data, obj);
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			bool changed = false;
			var go = data as GameObject;
			if (go != null)
			{
				foreach (Component comp in go.GetComponents<Component>())
				{
					changed |= inspector.Inspect(comp.GetType().Name, path + "." + comp.GetType().Name, comp);
				}
			}

			foreach (
				FieldInfo fieldInfo in
					type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				changed |= InspectField(inspector, path, ref data, fieldInfo);
			}

			foreach (
				PropertyInfo propertyInfo in
					type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				changed |= InspectProperty(inspector, path, ref data, propertyInfo);
			}
			return changed;
		}

		private static bool InspectField(Inspector inspector, string path, ref object data, FieldInfo fieldInfo)
		{
			object value = fieldInfo.GetValue(data);
			bool changed = false;
			object newValue = null;
			if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
				inspector.Inspect(fieldInfo.Name, path + "." + fieldInfo.Name, value, fieldInfo.FieldType);
			else
				inspector.Inspect(fieldInfo.Name, path + "." + fieldInfo.Name, value, fieldInfo.FieldType, null,
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

		private static bool InspectProperty(Inspector inspector, string path, ref object data,
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

				return inspector.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, value);
			}
			else
			{
				GUITools.LabelField(propertyInfo.Name, "unreadable");
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