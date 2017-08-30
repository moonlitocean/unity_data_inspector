using System;
using FF;
using FF.Editor;

internal class FFQuaternionVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        using (GUITools.LayoutVertical("Box"))
        {
            var quat = (FFQuaternion) data;
            bool changed = DrawMetadataUtility.EditFFQuaternion(name, ref quat);
            if (changed)
                data = quat;
            return changed;
        }
    }
}