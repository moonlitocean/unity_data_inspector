using System;
using System.Reflection;
using FF;

public class InspectorMethod
{
	public readonly string name;
	public readonly MethodInfo method;
	public readonly object bean;
	public readonly object[] param;

	public InspectorMethod(string name, MethodInfo method, object bean)
	{
		this.name = name;
		this.method = method;
		this.bean = bean;
		param = TypeUtil.BuildDefaultParamList(method);
	}

	public void Invoke()
	{
		try
		{
			method.Invoke(bean, param);
		}
		catch (Exception e)
		{
			LogMan.Warning("Failed: {0}", e);
		}
	}
}