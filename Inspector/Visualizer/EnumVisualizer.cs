using System;

namespace DataInspector
{
	internal class EnumVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return false;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var e = data as Enum;
			if (e == null)
				return false;
			if (TypeTools.GetAttribute<FlagsAttribute>(type) != null)
				return ApplyValueIfNotEqual(ref data, GUITools.EnumMaskField(name, e));
			else
				return ApplyValueIfNotEqual(ref data, GUITools.EnumPopup(name, e));
		}
	}
}