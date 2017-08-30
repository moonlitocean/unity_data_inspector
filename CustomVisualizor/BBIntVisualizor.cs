using FF;
using FF.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class BBIntVisualizor : DataVisualizor
{
    public override bool IsFoldable()
    {
        return false;
    }

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        using (GUITools.LayoutHorizontal())
        {
            var intData = data as BBInt;
            if (intData == null)
                return false;
            bool changed = false;
            changed |= DrawMetadataUtility.EditField(intData, "useBlackBoardData");
            if (intData.useBlackBoardData)
            {
                changed |= DrawMetadataUtility.EditField(intData, "blackBoardDataName");
            }
            else
            {
                intData.blackBoardDataName = null;
                changed |= DrawMetadataUtility.EditField(intData, "value");
            }
            return changed;
        }
    }
}
