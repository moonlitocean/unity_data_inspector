using System;

namespace DataInspector
{
	public interface IMark{}
	public class UnixTimestampAttribute : Attribute, IMark { }
}
