using System;

namespace DataInspector
{
	internal class PrimitiveVisualizer : VisualizerBase
	{
		// Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			object result;
			if (data == null)
			{
				GUITools.LabelField(name, "null");
				result = null;
			}
			else if (data is bool)
			{
				result = (bool)GUITools.Toggle(name, (bool)data);
			}
			else if (data is byte)
			{
				result = (byte)GUITools.IntField(name, (byte)data);
			}
			else if (data is sbyte)
			{
				result = (sbyte)GUITools.IntField(name, (sbyte)data);
			}
			else if (data is short)
			{
				result = (short)GUITools.IntField(name, (short)data);
			}
			else if (data is ushort)
			{
				result = (ushort)GUITools.IntField(name, (ushort)data);
			}
			else if (data is int)
			{
				result = (int)GUITools.IntField(name, (int)data);
			}
			else if (data is uint)
			{
				result = (uint)GUITools.LongField(name, (uint)data);
			}
			else if (data is long)
			{
				result = (long)ParseLong(GUITools.TextField(name, data.ToString()), (long)data);
			}
			else if (data is ulong)
			{
				result = (ulong)ParseULong(GUITools.TextField(name, data.ToString()), (ulong)data);
			}
			else if (data is IntPtr)
			{
				GUITools.TextField(name, data.ToString());
				result = data;
			}
			else if (data is UIntPtr)
			{
				GUITools.TextField(name, data.ToString());
				result = data;
			}
			else if (data is char)
			{
				result = (char)ParseChar(GUITools.TextField(name, data.ToString()), (char) data);
			}
			else if (data is float)
			{
				result = (float)GUITools.FloatField(name, (float)data);
			}
			else if (data is double)
			{
				result = (double)GUITools.DoubleField(name, (double)data);
			}
			else
			{
				throw new NotImplementedException(type.Name);
			}

			return ApplyValueIfNotEqual(ref data, result);
		}

		private long ParseLong(string text, long defaultLong)
		{
			try
			{
				return long.Parse(text);
			}
			catch (Exception)
			{
				return defaultLong;
			}
		}

		private ulong ParseULong(string text, ulong defaultLong)
		{
			try
			{
				return ulong.Parse(text);
			}
			catch (Exception)
			{
				return defaultLong;
			}
		}

		private char ParseChar(string text, char defaultChar)
		{
			if (!string.IsNullOrEmpty(text))
				return text[0];
			else
				return defaultChar;
		}
	}
}