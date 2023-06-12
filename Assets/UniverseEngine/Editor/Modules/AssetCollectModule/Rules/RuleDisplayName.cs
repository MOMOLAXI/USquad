
namespace UniverseEngine.Editor
{
	public struct RuleDisplayName
	{
		public string ClassName;
		public string DisplayName;

		public RuleDisplayName(string className, string displayName)
		{
			ClassName = className;
			DisplayName = displayName;
		}

		public static RuleDisplayName None = new("NoneClassName", "NoneDisplayName");
	}
}