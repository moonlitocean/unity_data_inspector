using System;
using FF;
using FF.Editor;

internal class FloatDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        using (GUITools.LayoutHorizontal())
        {
            var floatData = data as BBFloat;
            if (floatData == null)
                return false;
            bool changed = false;
            changed |= DrawMetadataUtility.EditField(floatData, "useBlackBoardData");
            if (floatData.useBlackBoardData)
            {
                changed |= DrawMetadataUtility.EditField(floatData, "blackBoardDataName");
            }
            else
            {
                floatData.blackBoardDataName = null;
                changed |= DrawMetadataUtility.EditField(floatData, "value");
            }
            return changed;
        }
    }
}