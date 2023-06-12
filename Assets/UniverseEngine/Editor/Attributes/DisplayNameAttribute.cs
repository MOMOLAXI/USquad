using System;
using System.Reflection;

namespace UniverseEngine.Editor
{
	public class DisplayNameAttribute : Attribute
	{
		public string DisplayName;

		public DisplayNameAttribute(string name)
		{
			DisplayName = name;
		}
	}
}