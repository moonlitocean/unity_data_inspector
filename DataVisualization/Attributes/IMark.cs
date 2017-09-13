using System;

namespace DataTools
{
	public interface IMark{}
	public class UnixTimestampAttribute : Attribute, IMark { }
}
