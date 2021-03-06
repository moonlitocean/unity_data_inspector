﻿using System;
using UnityEngine;

namespace DataInspector
{
	public abstract class VisualizerBase
	{
		// 是否使用了自定义了构造器。如果返回true，则需要实现 CustomCreateInstance
		public virtual bool HasCustomCreator(Type type, IMark mark)
		{
			return false;
		}

		// 
		// Normally, the label is field name.
		// If not null, field will be appended with the returned postfix
		public virtual string GetLabelPostfix(Inspector inspector, object data, Type type)
		{
			return null;
		}

		// OnGUI中回调。用GUI显示或编辑自己的单行数据值，并返回数据是否被修改。
		// 
		// GUI上下文：水平排列，其左侧会是自己的名称，当自己为引用类型时右侧会有一个+/-按钮。
		// 应当正好产生一个 GUI 控件，这样可以维持一行两列的整齐排布。
		// 在不产生控件的情况下，应当产生一个 flexibleSpace (正如本基类做的一样），以便保证右侧的 +/- 按钮靠右对齐
		public virtual bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			GUILayout.FlexibleSpace();
			return false;
		}

		// OnGUI中回调。表示本类型是否有子内容（一般是多行显示）
		//
		// 若为true，则有两种情况。
		// 默认情况下，子内容被折叠，同时在自己单行数据行左侧提供展开按钮，允许展开
		// 在AlwaysShowChildren的情况下，不显示展开按钮，同时总是显示子内容。
		public virtual bool HasChildren() { return false; }

		// 是否不提供折叠，总是显示所有子内容。字段很少的小结构可以启用该功能。
		public virtual bool AlwaysShowChildren() { return false; }

		// OnGUI中回调
		// 
		// 用于展现子内容，典型的用途为：容器展示其数据，自定义数据结构展示其成员
		// GUI上下文：垂直排列，多行控件。
		//
		// 一般递归对每个成员调用 inspector.InspectContent() 
		//
		// 返回数据是否被修改
		public virtual bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			return false;
		}

		public virtual object CustomCreateInstance(Type type, IMark mark)
		{
			throw new NotImplementedException();
		}

		// 将data改为income。
		//
		// data必须与income同类型，并且是valueType，否则行为是未定义的。
		// 如果使用 Equals 判断 data 与 income 不相同，则使用 income 覆盖 data
		// 否则保持data不变。
		// 
		// 返回： 是否发生了覆盖操作
		//
		// 注意： 若子字段修改不改变 Equals，则本函数不产生效果。例如 DateTime.Kind 不会影响equality。
		protected bool ApplyValueIfNotEqual(ref object data, object income)
		{
			if (data == null)
			{
				if (income == null)
					return false;

				data = income;
				return true;
			}
			else
			{
				var changed = !data.Equals(income);
				data = income;
				return changed;
			}
		}
	}
}
