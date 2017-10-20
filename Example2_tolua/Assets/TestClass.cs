using LuaInterface;

#pragma warning disable 414

public class TestClass
{
	public static LuaState state;
	static TestClass()
	{
		state = new LuaState();	
		state.Start();
		state.DoString(@"
			_data = {
				_num = 12,
				_float = 20.2,
				_bool = true,
				_string = 'hello world!',
				_table_int = {10, 20, 30, 40, 50},
				_table = { _name = 'MyName', _x = 10, _y = 20, _subtable = { _u = 30, _v = 40 } },
			}
		");
	}
}
