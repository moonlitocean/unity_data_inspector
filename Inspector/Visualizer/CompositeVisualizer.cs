using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DataInspector
{
	internal class CompositeVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			if (data == null)
				GUITools.LabelField("null [" + type.Name + "]");
			else
				GUITools.LabelField(type.Name);
			GUILayout.FlexibleSpace();
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var fields = new List<FieldInfo>();

			if (inspector.options.showStaticFields)
			{
				AppendFields(fields, type, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (inspector.options.showNonPublicFields)
					AppendFields(fields, type, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			}

			AppendFields(fields, type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (inspector.options.showNonPublicFields)
				AppendFields(fields, type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

			if (inspector.options.sortFields)
				fields.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

			bool changed = false;
			foreach (var fieldInfo in fields)
			{
				changed |= InspectField(inspector, path, ref data, fieldInfo, "");
			}

			if (inspector.options.showProperties)
			{
				foreach (
					PropertyInfo propertyInfo in
					type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					changed |= InspectProperty(inspector, path, ref data, propertyInfo);
				}
			}
			return changed;
		}

		// 添加相关Fields。但是特别的是，基类的属性在派生类之前
		private static void AppendFields(List<FieldInfo> fields, Type type, BindingFlags bindingFlags)
		{
		    if (type == null)
		        return;
			if (type.BaseType != typeof (object) && type != typeof (object))
			{
				AppendFields(fields, type.BaseType, bindingFlags);
			}
			fields.AddRange(type.GetFields(bindingFlags | BindingFlags.DeclaredOnly));
		}

		private static bool InspectField(Inspector inspector, string path, ref object data, FieldInfo fieldInfo,
			string prefix)
		{
			object value = fieldInfo.GetValue(data);
			FieldInfo info = fieldInfo;
			bool inwritable = info.IsInitOnly || info.IsLiteral;
			Type valueType = value != null ? value.GetType() : info.FieldType;

			bool changed = false;
			object changedvalue = null;
			var mark = TypeTools.GetAttribute<IMark>(info);
			if (inwritable)
				inspector.Inspect(prefix + fieldInfo.Name, path + "." + fieldInfo.Name, value, valueType, mark);
			else
				inspector.Inspect(prefix + fieldInfo.Name, path + "." + fieldInfo.Name, value, valueType, mark, v =>
				{
					changed = true;
					changedvalue = v;
				});

			if (changed)
			{
				info.SetValue(data, changedvalue);
			}
			return changed;
		}

		private static bool InspectProperty(Inspector inspector, string path, ref object data,
			PropertyInfo propertyInfo)
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

				return inspector.Inspect(propertyInfo.Name, path + "." + propertyInfo.Name, value);
			}
			else
			{
				GUITools.LabelField(propertyInfo.Name, "unreadable");
				return false;
			}
		}
	}
}