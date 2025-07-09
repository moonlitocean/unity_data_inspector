using System;
using System.Collections;
using UnityEngine;

namespace DataInspector
{
	internal class ArrayVisualizer : CollectionVisualizerBase
	{
		public override bool HasCustomCreator(Type type, IMark mark)
		{
			return true;
		}

		public override object CustomCreateInstance(Type type, IMark mark)
		{
			return Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);
		}

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
			return ((IList)collection)[(int)key];
		}

		public override void Set(object collection, object key, object value)
		{
			((IList)collection)[(int)key] = value;
		}

		public override Type ValueType(object collection, object key)
		{
			return collection.GetType().GetElementType();
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
					(list[index], list[index-1]) = (list[index-1], list[index]);
				}
			}

			// Move Down
			if(GUILayout.Button("\u25bc", GUILayout.Width(20)))
			{
				if (index < count - 1)
				{
					Inspector.DropFocus_SkipValueChangeOneFrame();
					(list[index], list[index + 1]) = (list[index + 1], list[index]);
				}
			}

			// 暂时不支持
			//using (GUITools.Color(Color.green))
			//{
			//	// Insert Up
			//	if (GUILayout.Button("\u25b2+", GUILayout.Width(30)))
			//	{
			//		Inspector.DropFocus_SkipValueChangeOneFrame();
			//		list.Insert(index, Activator.CreateDefaultInstance(collection.GetType().GetElementType()));
			//	}

			//	// Insert Down
			//	if (GUILayout.Button("\u25bc+", GUILayout.Width(30)))
			//	{
			//		Inspector.DropFocus_SkipValueChangeOneFrame();
			//		list.Insert(index+1, Activator.CreateDefaultInstance(collection.GetType().GetElementType()));
			//	}
			//}

			//using (GUITools.Color(Color.red))
			//{
			//	if (GUILayout.Button("x", GUILayout.Width(20)))
			//	{
			//		return TypeTools.RemoveAt((Array)collection, index);
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
			return TypeTools.ResizeArray(collection as Array, size);
		}
	}
}