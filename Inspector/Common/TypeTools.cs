using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace DataInspector
{
	internal static class TypeTools
	{
		private static readonly Type DictionaryGenericType = typeof(Dictionary<,>);
		private static readonly Type ListGenericType = typeof(List<>);
		private static readonly Type HashSetGenericType = typeof(HashSet<>);

		////////////////////////////////////
		// list
		public static Type GetListValueType(Type type)
		{
			return FindGenerticAncestor(type, ListGenericType).GetGenericArguments()[0];
		}

		////////////////////////////////////
		// hashset
		public static bool IsSubclassOfHashSet(Type type)
		{
			return FindGenerticAncestor(type, HashSetGenericType) != null;
		}

		public static Type GetHashSetValueType(Type type)
		{
			return FindGenerticAncestor(type, HashSetGenericType).GetGenericArguments()[0];
		}

		////////////////////////////////////
		// dictionary
		public static bool IsSubclassOfDictionary(Type type)
		{
			return FindGenerticAncestor(type, DictionaryGenericType) != null;
		}

		public static Type GetDictionaryValueType(Type type)
		{
			return FindGenerticAncestor(type, DictionaryGenericType).GetGenericArguments()[1];
		}

		public static Type FindGenerticAncestor(Type type, Type wantedType)
		{
			for (Type ancestor = type; ancestor != null; ancestor = ancestor.BaseType)
			{
				if (ancestor.IsGenericType && ancestor.GetGenericTypeDefinition() == wantedType)
					return ancestor;
			}
			return null;
		}

		public static Array ResizeArray(Array array, int newCount)
		{
			if (newCount < 0)  throw new ArgumentException();
			if (array == null) throw new ArgumentNullException("array");

			Type elemType = array.GetType().GetElementType();

			Array resized = Array.CreateInstance(elemType, newCount);
			for (int i = 0; i < resized.Length; ++i)
			{
				if (i < array.Length)
				{
					resized.SetValue(array.GetValue(i), i);
				}
				else
				{
					resized.SetValue(CreateInstance(elemType), i);
				}
			}
			return resized;
		}

		public static object CreateInstance(Type type)
		{
			if (type == typeof(string))
				return "";
			else
				return Activator.CreateInstance(type);
		}

		public static T GetAttribute<T>(MemberInfo info)
			where T : class
		{
			return info.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
		}

		// 获取Field、Property或无参数Method的返回值
		public static T GetValueEx<T>(this object obj, string name)
		{
			if (obj == null)
				return default(T);
			Type t = obj.GetType();
			FieldInfo field = t.GetField(name);
			if (field != null && typeof(T).IsAssignableFrom(field.FieldType))
				return (T)field.GetValue(obj);
			PropertyInfo prop = t.GetProperty(name);
			if (prop != null && prop.CanRead)
			{
				MethodInfo m = prop.GetGetMethod();
				if (m != null && typeof(T).IsAssignableFrom(m.ReturnType))
				{
					return (T)m.Invoke(obj, null);
				}
			}
			MethodInfo method = t.GetMethod(name, Type.EmptyTypes);
			if (method != null && typeof(T).IsAssignableFrom(method.ReturnType))
			{
				return (T)method.Invoke(obj, null);
			}
			return default(T);
		}
	}
}

