using System;
using System.Globalization;
using UnityEditor;

namespace DataInspector
{
	internal class UnixTimeStampVisualizer : VisualizerBase
	{
		private static readonly DateTime CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

		public override string GetLabelPostfix(Inspector inspector, object data, Type type)
		{
			return " [" + TimeZone.CurrentTimeZone.StandardName + "]";
		}

		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var timestamp = (int) data;
			DateTime time = CTIME_BEGIN.AddSeconds(timestamp).ToLocalTime();
			EditorGUILayout.LabelField(time.ToString(CultureInfo.InvariantCulture));
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var timestamp = (int) data;
			return ApplyValueIfNotEqual(ref data, EditorGUILayout.IntField("Unix Time Stamp", timestamp));
		}
	}
}