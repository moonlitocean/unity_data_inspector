using System;
using FF.Unity;

internal class UINodeVisualizor : DataVisualizor
{
    public override bool IsFoldable()
    {
        return true;
    }

    public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
    {
        UINode node = data as UINode;
        if (node == null)
            return false;

	    bool changed = false;
        visualization.Inspect("ID", path + ".ID", node.cmd.ID.ToString());
		changed |= visualization.Inspect("Cmd", path + ".cmd", node.cmd);
		changed |= visualization.Inspect("Children", path + ".Children", node.childList);
		return changed;
    }
}
