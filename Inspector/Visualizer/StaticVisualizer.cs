using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DataInspector
{
	internal class StaticVisualizer : CollectionVisualizerBase
	{
		private class TypeData
		{
			public BindingFlags flags;
			public bool hasProperties;

			public string[] memberNames;
			public Dictionary<string, MemberInfo> members;
		}
		private static readonly Dictionary<Type, TypeData> cached = new Dictionary<Type, TypeData>();

		private static BindingFlags GetBindingFlags(Inspector.Options options)
		{
			var flags = BindingFlags.Static | BindingFlags.Public;
			if (options.showNonPublicFields)
				flags |= BindingFlags.NonPublic;
			return flags;
		}

		public override bool ShowSize()
		{
			return false;
		}

		public override object[] Keys(Inspector.Options options, object collection)
		{
			return GetMembers((Type)collection, options).memberNames;
		}

		public override object Get(object collection, object key)
		{
			MemberInfo member = GetMember(collection, key);

			if (member is FieldInfo)
				return ((FieldInfo) member).GetValue(null);
			else if (member is PropertyInfo)
				return GetProperty((PropertyInfo)member);
			
			return null;
		}

		public override void Set(object collection, object key, object value)
		{
			MemberInfo member = GetMember(collection, key);

			if (member is FieldInfo)
				((FieldInfo)member).SetValue(null, value);
			else if (member is PropertyInfo)
				SetProperty(value, (PropertyInfo)member);
		}

		private static object GetProperty(PropertyInfo member)
		{
			if (!member.CanRead)
				return "not readable";
			try
			{
				return member.GetValue(null, null);
			}
			catch (Exception)
			{
				return "read failed";
			}
		}

		private static void SetProperty(object value, PropertyInfo member)
		{
			if (!member.CanWrite)
				return;
			try
			{
				member.SetValue(null, value, null);
			}
			catch(Exception){}
		}


		private static MemberInfo GetMember(object collection, object key)
		{
			TypeData data;
			if (!cached.TryGetValue((Type)collection, out data))
				return null;

			MemberInfo member;
			if (!data.members.TryGetValue((string)key, out member))
				return null;

			return member;
		}

		private static TypeData GetMembers(Type type, Inspector.Options options)
		{
			if (!cached.ContainsKey(type))
				cached.Add(type, new TypeData());
			var data = cached[type];

			BindingFlags flags = GetBindingFlags(options);
			if (data.flags != flags || data.hasProperties != options.showProperties)
				UpdateMembers(data, type, flags, options.showProperties);
			return data;
		}

		private static void UpdateMembers(TypeData data, Type type, BindingFlags flags, bool showProperties)
		{
			List<MemberInfo> members = new List<MemberInfo>();
			members.AddRange(type.GetFields(flags));
			if (showProperties) members.AddRange(type.GetProperties(flags));

			data.members = new Dictionary<string, MemberInfo>(members.Count);
			foreach (var m in members)
				data.members[m.Name] = m;
			data.memberNames = members.Select(o => o.Name).ToArray();

			data.flags = flags;
			data.hasProperties = showProperties;
		}
	}
}