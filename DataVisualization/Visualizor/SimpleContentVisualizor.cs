using System;
using System.Collections.Generic;
using System.Reflection;
using FF;

internal class SimpleContentVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		var fields = new List<FieldInfo>();

		if (visualization.options.simpleContent.showClassStaticFields)
		{
			AppendFields(fields, type, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (visualization.options.simpleContent.showNonPublicFields)
				AppendFields(fields, type, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		}

		AppendFields(fields, type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (visualization.options.simpleContent.showNonPublicFields)
			AppendFields(fields, type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		if (visualization.options.simpleContent.sortFields)
		{
			fields.Sort((a, b) => string.Compare(a.Name, b.Name));
		}

		bool changed = false;
		foreach (var fieldInfo in fields)
		{
			changed |= InspectField(visualization, path, ref data, fieldInfo, "");
		}
		return changed;
	}

	// 添加相关Fields。但是特别的是，基类的属性在派生类之前
	private static void AppendFields(List<FieldInfo> fields, Type type, BindingFlags bindingFlags)
	{
		if (type.BaseType != typeof(object))
		{
			AppendFields(fields, type.BaseType, bindingFlags);
		}
		fields.AddRange(type.GetFields(bindingFlags | BindingFlags.DeclaredOnly));
	}

	private static bool InspectField(DataVisualization visualization, string path, ref object data, FieldInfo fieldInfo, string prefix)
	{
		object value = fieldInfo.GetValue(data);
		FieldInfo info = fieldInfo;
		bool inwritable = info.IsInitOnly || info.IsLiteral;
		Type valueType = value != null ? value.GetType() : info.FieldType;

		bool changed = false;
		object changedvalue = null;
		var mark = TypeTools.GetAttribute<IEditorMark>(info);
		if (inwritable)
			visualization.Inspect(prefix + fieldInfo.Name, path + "." + fieldInfo.Name, value, valueType, mark);
		else
			visualization.Inspect(prefix + fieldInfo.Name, path + "." + fieldInfo.Name, value, valueType, mark, v =>
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
}