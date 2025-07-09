using System;
using UnityEngine;

namespace DataInspector
{
	// 容器：可以用于Dictionary，List等按下表索引的类型
	public abstract class CollectionVisualizerBase : VisualizerBase, GUIContainerTools.IDictParser
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			if (data == null)
			{
				GUITools.LabelField("null");
				GUILayout.FlexibleSpace();
				return false;
			}

			if (ShowSize())
				GUITools.LabelField("Count: " + Size(data));

			GUILayout.FlexibleSpace();
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			if (data == null)
				return false;

			if (Resizable(data))
			{
				var oldSize = Size(data);
				int size = Math.Max(0, GUITools.IntField("Size", oldSize));
				if (size != oldSize)
				{
					data = Resize(data, size);
					return true;
				}
			}

			return GUIContainerTools.EditDict(inspector, path, data, this);
		}

		//------------------------------------------------------
		// 通过实现下面的成员方法，来定义容器的行为

		// 必须功能：访问、遍历，以及获得容器中元素的声明类型
		public abstract object[] Keys(Inspector.Options options, object collection);
		public abstract object Get(object collection, object key);
		public abstract void Set(object collection, object key, object value);

		// 获得元素的声明类型。常规容器返回元素类型与key无关； Static Class 容器返回各member的类型
		public abstract Type ValueType(object collection, object key);

		// 可选功能：绘制元素的额外操作按钮。例如数组元素的移动。
		public virtual object OnGUIDrawElemExtraButtons(object collection, object key) { return collection; }

		// 可选功能：Size
		public virtual bool ShowSize() {return true;}
		public virtual int Size(object collection) { return 0; }

		// 可选功能：Resizable
		public virtual bool Resizable(object collection) { return false; }					// 如果Resizable为false，则不需要实现Resize
		public virtual object Resize(object collection, int size){return collection;}		// 返回新大小的容器（可以是自己）
	}
}

