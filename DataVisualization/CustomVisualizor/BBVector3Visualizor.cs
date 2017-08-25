using System;
using FF.Editor;
using FF;

internal class Vector3DataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        using (GUITools.LayoutVertical())
        {
            var vector3Data = data as BBVector3;
            if (vector3Data == null)
                return false;
            bool changed = false;
            changed |= DrawMetadataUtility.EditField(vector3Data, "useBlackBoardData");
            if (vector3Data.useBlackBoardData)
            {
                changed |= DrawMetadataUtility.EditField(vector3Data, "blackBoardDataName");
            }
            else
            {
                vector3Data.blackBoardDataName = null;
                changed |= DrawMetadataUtility.EditField(vector3Data, "value");
            }
            return changed;
        }
    }
}