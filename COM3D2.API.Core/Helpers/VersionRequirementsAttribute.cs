using System;

namespace COM3D2API
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class VersionRequirementsAttribute : Attribute
	{
		public string PluginName { get; }
		public bool AllAges { get; }
		public string MinimumVersion2 { get; }
		public string MinimumVersion3 { get; }

		public VersionRequirementsAttribute(string pluginName, string minimumVersion2 = null, string minimumVersion3 = null, bool allAges = true)
		{
			PluginName = pluginName;
			MinimumVersion2 = minimumVersion2;
			MinimumVersion3 = minimumVersion3;
			AllAges = allAges;
		}
	}
}