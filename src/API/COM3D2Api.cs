﻿using BepInEx;
using BepInEx.Logging;
using COM3D2API.Character;
using COM3D2API.Helpers;
using COM3D2API.UI;
using System.Security;
using System.Security.Permissions;
using COM3D2API.Utilities;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace COM3D2API
{
	/// <summary>
	/// Contains information about the plugin itself
	/// </summary>
	[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
	[BepInDependency(SaveDataExtended.SaveDataExt.Guid)]
	public class Com3D2Api : BaseUnityPlugin
	{
		/// <summary>
		/// GUID of the plugin, use with <see cref="BepInDependency"/>:
		/// <code>[BepInDependency(COM3D2Api.COM3D2Api.PluginGUID)]</code>
		/// </summary>
		public const string PluginGuid = "deathweasel.com3d2.api";

		/// <summary> Name of the plugin </summary>
		public const string PluginName = "COM3D2 API";

		/// <summary> Version of the plugin </summary>
		public const string PluginVersion = "2.3";

		internal new static ManualLogSource Logger;

		private void Awake()
		{
			Logger = base.Logger;

			GameCompatibility.Init();
			SystemShortcutAPI.RegisterHooks();
			MaidApi.Init();
			EditModeApi.Init();
			MessageApi.Init();

			gameObject.GetOrAddComponent<UnityMainThreadDispatcher>();

#if DEBUG
			EditModeApi.InjectSlider("Debug Slider", "body_all_slider", f =>
			{
				Logger.LogDebug($"Slider set to {f}");
			}, () => 0.5f);

			EditModeApi.InjectSubMenu("Debug", SceneEditInfo.EMenuCategory.身体, SceneEditInfo.CCateNameType.EType.Slider);

			EditModeApi.InjectSlider("Debug Slider", "Debug", f =>
			{
				Logger.LogDebug($"Slider set to {f}");
			}, () => 0.5f);

			EditModeApi.InjectSlider("Debug Slider 2", "Debug", f =>
			{
				Logger.LogDebug($"Slider set to {f}");
			}, () => 0.75f);

			EditModeApi.InjectSlider("Debug Slider 3", "Debug", f =>
			{
				Logger.LogDebug($"Slider set to {f}");
			}, () => 0.25f);
#endif
		}

		private void Start()
		{
			Texture.allowThreadedTextureCreation = true;
		}
	}
}