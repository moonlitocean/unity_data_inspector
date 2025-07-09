using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataInspector
{
	internal class ListVisualizer : CollectionVisualizerBase
	{
		public override int Size(object collection)
		{
			return ((ICollection)collection).Count;
		}

		public override object[] Keys(Inspector.Options options, object collection)
		{
			object[] keys = new object[((ICollection)collection).Count];
			for (int i = 0; i < keys.Length; ++i)
				keys[i] = i;
			return keys;
		}

		public override object Get(object collection, object key)
		{
			return ((IList) collection)[(int) key];
		}

		public override void Set(object collection, object key, object value)
		{
			((IList)collection)[(int)key] = value;
		}

		public override Type ValueType(object collection, object key)
		{
			return TypeTools.FindGenericParamType(collection.GetType(), typeof(List<>), 0);
		}

		public override object OnGUIDrawElemExtraButtons(object collection, object key)
		{
			EventType type = Event.current.type;
			IList list = (IList)collection;
			int index = (int)key;
			int count = list.Count;

			// Move Up
			if (GUILayout.Button("\u25b2", GUILayout.Width(20)))
			{
				if (index > 0)
				{
					Inspector.DropFocus_SkipValueChangeOneFrame();
					(list[index], list[index - 1]) = (list[index - 1], list[index]);
				}
			}

			// Move Down
			if (GUILayout.Button("\u25bc", GUILayout.Width(20)))
			{
				if (index < count - 1)
				{
					Inspector.DropFocus_SkipValueChangeOneFrame();
					(list[index], list[index + 1]) = (list[index + 1], list[index]);
				}
			}

			using (GUITools.Color(Color.green))
			{
				// Insert Up
				if (GUILayout.Button("\u25b2+", GUILayout.Width(30)))
				{
					Inspector.DropFocus_SkipValueChangeOneFrame();
					list.Insert(index, TypeTools.CreateDefaultInstance(ValueType(collection, key)));
				}

				// Insert Down
				if (GUILayout.Button("\u25bc+", GUILayout.Width(30)))
				{
					Inspector.DropFocus_SkipValueChangeOneFrame();
					list.Insert(index + 1, TypeTools.CreateDefaultInstance(ValueType(collection, key)));
				}
			}

			//using (GUITools.Color(Color.red))
			//{
			//	if (GUILayout.Button("x", GUILayout.Width(20)))
			//	{
			//		list.RemoveAt(index);
			//	}
			//}

			return collection;
		}

		public override bool Resizable(object collection)
		{
			return true;
		}

		public override object Resize(object collection, int size)
		{
			IList list = (IList)collection;

			var elemType = ValueType(collection, 0);
			while (list.Count > size)
			{
				list.RemoveAt(list.Count - 1);
			}
			while (list.Count < size)
			{
				if (elemType.IsClass)
					list.Add(null);
				else
					list.Add(TypeTools.CreateDefaultInstance(elemType));
			}
			return list;
		}
	}
}