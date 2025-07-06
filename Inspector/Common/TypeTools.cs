using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataInspector
{
	internal static class TypeTools
	{
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
				else
				{
					resized.SetValue(CreateInstance(elemType), i);
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

		public static List<Type> GetDerivedTypeWithDefaultCtor(Type baseType)
		{
			var derived = new List<Type>();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				try
				{
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						if (baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract &&
							type.GetConstructor(Type.EmptyTypes) != null)
						{
							derived.Add(type);
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					// ignore Load Exception 
				}
			}

			return derived;
		}
	}
}

