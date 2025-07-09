using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DataInspector
{
	internal static class TypeTools
	{
		private static Dictionary<Type, bool> HasDefaultCtor = new();					// 自身是否有默认构造函数
		private static Dictionary<Type, List<Type>> AssignableDefaultCtorTypes = new();	// 自身及派生类中所有含默认构造函数类

		//////////////////////////////////////////////////////////////////
		// 主要用于找到容器的元素类型
		public static Type FindGenericParamType(Type type, Type generic, int paramIndex)
		{
			var ancestor = FindGenerticAncestor(type, generic);
			if (ancestor == null)
				return null;

			var genericArguments = ancestor.GetGenericArguments();
			if (paramIndex >= genericArguments.Length)
				return null;

			return genericArguments[paramIndex];
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
			if (newCount < 0) throw new ArgumentException();
			if (array == null) throw new ArgumentNullException("array");

			Type elemType = array.GetType().GetElementType();

			Array resized = Array.CreateInstance(elemType, newCount);
			for (int i = 0; i < resized.Length; ++i)
			{
				if (i < array.Length)
				{
					resized.SetValue(array.GetValue(i), i);
				}
				else if(CanCreateDefaultInstance(elemType))
				{
					resized.SetValue(CreateDefaultInstance(elemType), i);
				}
			}
			return resized;
		}

		public static void TrySort(object[] keys)
		{
			try
			{
				Array.Sort(keys);
			}
			catch (Exception)
			{
			}
		}

		// 自身是否可以直接创建 Instance（例如存在默认构造函数）
		// 如果返回 true，则可以用 CreateDefaultInstance() 直接创建 Instance
		//
		// 这里有特殊白名单，如 string
		// 特殊黑名单，如 GameObject  (默认构造函数会在场景上新建一个 New GameObject)
		public static bool CanCreateDefaultInstance(Type type)
		{
			if (!HasDefaultCtor.TryGetValue(type, out bool canCreate))
			{
				if(type == typeof(string))
					canCreate = true;
				else if (type == typeof(GameObject))
					canCreate = false;
				else if (!type.IsInterface && !type.IsAbstract &&
					type.GetConstructor(Type.EmptyTypes) != null)
					canCreate = true;
				else
					canCreate = false;

				HasDefaultCtor[type] = canCreate;
			}

			return canCreate;
		}

		public static object CreateDefaultInstance(Type type)
		{
			if (type == typeof(string))
				return "";
			else
				return Activator.CreateInstance(type);
		}

		// 自身或自己的派生类，是否存在至少一个类有默认构造函数
		public static bool HasDerivedCreateDefaultInstance(Type type)
		{
			if (!AssignableDefaultCtorTypes.TryGetValue(type, out var list))
			{
				list = GetDerivedDefaultInstanceTypes(type);
			}
			
			return list.Count > 0;
		}

		public static List<Type> GetDerivedDefaultInstanceTypes(Type baseType)
		{
			if (!AssignableDefaultCtorTypes.TryGetValue(baseType, out var list))
			{
				list = new List<Type>();
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					try
					{
						Type[] types = assembly.GetTypes();
						foreach (Type type in types)
						{
							if (baseType.IsAssignableFrom(type) && CanCreateDefaultInstance(type))
							{
								list.Add(type);
							}
						}
					}
					catch (ReflectionTypeLoadException)
					{
						// ignore Load Exception 
					}
				}

				AssignableDefaultCtorTypes[baseType] = list;
			}

			return list;
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

