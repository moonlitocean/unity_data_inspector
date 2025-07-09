using System;
using System.Globalization;

namespace DataInspector
{
	internal class UnixTimeStampVisualizer : VisualizerBase
	{
		private static readonly DateTime CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

		public override string GetLabelPostfix(Inspector inspector, object data, Type type)
		{
			return " [" + TimeZoneInfo.Local.StandardName + "]";
		}

		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var timestamp = (int) data;
			DateTime time = CTIME_BEGIN.AddSeconds(timestamp).ToLocalTime();
			GUITools.LabelField(time.ToString(CultureInfo.InvariantCulture));
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var timestamp = (int) data;
			return ApplyValueIfNotEqual(ref data, GUITools.IntField("Unix Time Stamp", timestamp));
		}
	}
}