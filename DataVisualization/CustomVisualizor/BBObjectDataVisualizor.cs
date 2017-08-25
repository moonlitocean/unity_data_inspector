using System;
using FF.Editor;
using FF;
using Metadata;

internal class BBObjectDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        var objectData = data as BBObjectData;
        if (objectData == null)
            return false;
        bool changed = false;
        using (GUITools.LayoutVertical())
        {
            changed |= DrawMetadataUtility.EditField(objectData, "useBlackBoardData");
            if (objectData.useBlackBoardData)
            {
                changed |= DrawMetadataUtility.EditField(objectData, "blackBoardDataName");
            }
            else
            {
                objectData.blackBoardDataName = null;
                if (objectData.value == null)
                    objectData.value = new ObjectData();
                DrawMetadataUtility.ShowObjectPicker(objectData.value, (objdata) =>
                {
                    changed = true;
                });

            }
        }
        return changed;
    }
}