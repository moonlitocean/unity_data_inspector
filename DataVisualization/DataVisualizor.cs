using System;
using FF;

public abstract class DataVisualizor
{
	public abstract bool IsFoldable();

	// 是否使用了自定义了构造器。如果返回true，则需要实现 CustomCreateInstance
	public virtual bool HasCustomCreator(IEditorMark mark)
	{
		return false;
	}

	public virtual bool AlwaysFoldout()
	{
		return true;
	}

	public virtual bool HasContent(object data, Type type)
	{
		return true;
	}

	public virtual bool InspectRoot(string name, ref object data, Type type)
	{
		return false; 
	}

	public virtual bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		return false;
	}

	public virtual object CustomCreateInstance(IEditorMark mark)
	{
		throw new NotImplementedException();
	}


	// 返回 data 是否被修改
	protected bool ApplyData(ref object data, object income)
	{
		if (data == null)
		{
			if (income == null)
				return false;
			
			data = income;
			return true;
		}
		else
		{
			var changed = !data.Equals(income);
			data = income;
			return changed;
		}
	}
}