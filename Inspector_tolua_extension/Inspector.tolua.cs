using LuaInterface;

namespace DataInspector
{
	partial class Inspector
	{
		public static void OpenToLua()
		{
			OnRegisterDefaultVisualizers["tolua"] =
				o => o.SetVisualizer(typeof (LuaTable), new LuaTableVisualizer());
		}
	}
}
