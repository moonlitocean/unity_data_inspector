using System;
using UnityEditor;

namespace DataInspector
{
	// 容器：可以用于Dictionary，List等按下表索引的类型
	internal abstract class BaseContainerVisualizer : VisualizerBase, GUIContainerTools.IDictParser
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			if (data == null)
			{
				EditorGUILayout.LabelField("null");
				return false;
			}

			EditorGUILayout.LabelField("Count: " + Size(data));
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			if (data == null)
				return false;

			if (Resizable(data))
			{
				int size = Math.Max(0, EditorGUILayout.DelayedIntField("Size", Size(data)));
				if (size != Size(data))
				{
					data = Resize(data, size);
					return true;
				}
			}

			return GUIContainerTools.EditDict(inspector, path, data, this);
		}

		//------------------------------------------------------
		// 通过实现下面的成员方法，来定义容器的行为

		// 必须功能：访问和遍历
		public abstract int Size(object collection);
		public abstract object[] Keys(object collection);
		public abstract object Get(object collection, object key);
		public abstract bool Set(object collection, object key, object value);
		public abstract Type ValueType(object collection);

		// 可选功能：Resizable
		public virtual bool Resizable(object collection) { return false; }					// 如果Resizable为false，则不需要实现Resize
		public virtual object Resize(object collection, int size){return collection;}		// 返回新大小的容器（可以是自己）
	}
}

