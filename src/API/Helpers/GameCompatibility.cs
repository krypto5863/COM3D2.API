using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.SceneManagement;

namespace COM3D2API.Helpers
{
	internal static class GameCompatibility
	{
		private static bool _isAllAges;
		private static Version _version;

		internal static void Init()
		{
			_isAllAges = Product.isPublic;
			var currentVersion = GameUty.GetBuildVersionText();
			if (VersionHelper.TryParse(currentVersion, out _version) == false)
			{
				return;
			}

			SceneManager.sceneLoaded += SceneLoaded;
		}

		private static void SceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (arg0.name.Equals("SceneTitle") == false)
			{
				return;
			}

			SceneManager.sceneLoaded -= SceneLoaded;
			CheckVersions();
		}

		private static void CheckVersions()
		{
			var incompatiblePlugins = new List<Tuple<string, string>>();
			foreach (var attribute in GetAllAttributes())
			{
				Com3D2Api.Logger.LogDebug($"{attribute.PluginName} reports it needs version {attribute.MinimumVersion2}");

				if (_isAllAges && attribute.AllAges == false)
				{
					incompatiblePlugins.Add(new Tuple<string, string>(attribute.PluginName, "Does not support non-18+ versions."));
					continue;
				}

				var activeVersion = _version.Major == 2 ? attribute.MinimumVersion2 : attribute.MinimumVersion3;

				if (string.IsNullOrEmpty(activeVersion))
				{
					incompatiblePlugins.Add(new Tuple<string, string>(attribute.PluginName, $"Does not support {_version.Major}.0+"));
					continue;
				}

				if (VersionHelper.TryParse(activeVersion, out var properVersion) == false)
				{
					continue;
				}

				if (properVersion.Minor == _version.Minor && properVersion.Build > _version.Build)
				{
					incompatiblePlugins.Add(new Tuple<string, string>(attribute.PluginName, $"Requires version {activeVersion} at minimum."));
					continue;
				}

				if (properVersion.Minor > _version.Minor)
				{
					incompatiblePlugins.Add(new Tuple<string, string>(attribute.PluginName, $"Requires version {activeVersion} at minimum."));
				}
			}

			if (incompatiblePlugins.Count <= 0)
			{
				return;
			}

			var message = new StringBuilder();
			message.AppendLine("The following plugins require newer or different version:");
			foreach (var incompatible in incompatiblePlugins)
			{
				message.AppendLine($"{incompatible.item1}: {incompatible.item2}");
			}

			Com3D2Api.Logger.LogError(message.ToString().Trim());
		}

		private static IEnumerable<VersionRequirementsAttribute> GetAllAttributes()
		{
			var allTypes = new List<Type>();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					// Try to get all types in the assembly
					allTypes.AddRange(assembly.GetTypes());
				}
				catch (ReflectionTypeLoadException ex)
				{
					// Log the error or handle it silently if preferred
					COM3D2API.Com3D2Api.Logger.LogError($"Failed to load types from assembly: {assembly.FullName}");

					// You can log the loader exceptions here if needed
					foreach (var loaderException in ex.LoaderExceptions)
					{
						COM3D2API.Com3D2Api.Logger.LogError(loaderException.Message);
					}

					// Skip this assembly and continue with the rest
					continue;
				}
			}

			foreach (var type in allTypes)
			{
				var attributes = type.GetCustomAttributes(typeof(VersionRequirementsAttribute), false);

				if (attributes.Length <= 0)
				{
					continue;
				}

				var attribute = (VersionRequirementsAttribute)attributes[0];
				yield return attribute;
			}
		}
	}
}