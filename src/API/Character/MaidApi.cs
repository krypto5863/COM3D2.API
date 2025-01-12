using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace COM3D2API.Character
{
	public static class MaidApi
	{
		internal static List<MaidControllerManager> ControllerTypes = new List<MaidControllerManager>();

		internal static void Init()
		{
			RegisterHooks();

			SaveDataExtended.Events.PresetBeingSaved += SaveDataExtOnPresetBeingSaved;
			SaveDataExtended.Events.PresetLoaded += SaveDataExtOnPresetLoaded;

			SaveDataExtended.Events.SaveBeingSaved += SaveDataExtOnSaveBeingSaved;
			SaveDataExtended.Events.SaveLoaded += SaveDataExtOnSaveLoaded;
		}

		private static void SaveDataExtOnPresetBeingSaved(Maid maid, CharacterMgr.PresetType presetType)
		{
			foreach (var manager in ControllerTypes)
			{
				foreach (var controller in manager.ControllerInstances)
				{
					controller.OnMaidSavingInternal(maid, presetType);
				}
			}
		}

		private static void SaveDataExtOnPresetLoaded(Maid maid, CharacterMgr.PresetType presetType)
		{
			foreach (var manager in ControllerTypes)
			{
				foreach (var controller in manager.ControllerInstances)
				{
					controller.OnMaidBeingLoadedInternal(maid, presetType);
				}
			}
		}

		private static void SaveDataExtOnSaveBeingSaved()
		{
			foreach (var manager in ControllerTypes)
			{
				foreach (var controller in manager.ControllerInstances)
				{
					controller.OnMaidSavingInternal();
				}
			}
		}

		private static void SaveDataExtOnSaveLoaded()
		{
			foreach (var manager in ControllerTypes)
			{
				foreach (var controller in manager.ControllerInstances)
				{
					controller.OnMaidBeingLoadedInternal();
				}
			}
		}

		public static void RegisterCharacterController<TMaidController>(string guid) where TMaidController : MaidController
		{
			ControllerTypes.Add(new MaidControllerManager(guid, typeof(TMaidController)));
		}

		internal static void RegisterHooks() => Harmony.CreateAndPatchAll(typeof(Hooks));

		private static class Hooks
		{
			private static IEnumerable<MaidProp> _dirtyMaidProps;

			[HarmonyPostfix, HarmonyPatch(typeof(Maid), nameof(Maid.Initialize))]
			private static void MaidCreated(ref Maid __instance)
			{
				if (__instance.boNPC || __instance.boMAN)
				{
					return;
				}

				foreach (var type in ControllerTypes)
				{
					type.GetOrAddController(__instance);
				}
			}

			/*
			[HarmonyPostfix, HarmonyPatch(typeof(Maid), nameof(Maid.Uninit))]
			private static void MaidUnInit(ref Maid __instance)
			{
				foreach (var manager in ControllerTypes)
				{
					foreach (var controller in manager.ControllerInstances)
					{
						controller.OnMaidDeactivatedInternal(__instance);
					}
				}
			}
			*/

			[HarmonyPrefix, HarmonyPatch(typeof(Maid), nameof(Maid.AllProcProp))]
			private static void MaidPropsUpdating(ref Maid __instance)
			{
				_dirtyMaidProps = __instance.m_aryMaidProp
					.Where(m => m.boDut || m.boTempDut)
					.ToArray();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Maid), nameof(Maid.AllProcProp))]
			private static void MaidPropsUpdated(ref Maid __instance)
			{
				if (!_dirtyMaidProps.Any())
				{
					return;
				}

				foreach (var manager in ControllerTypes)
				{
					foreach (var controller in manager.ControllerInstances)
					{
						controller.OnMaidPropChangedInternal(__instance, _dirtyMaidProps);
					}
				}

				_dirtyMaidProps = null;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Maid), nameof(Maid.AllProcPropSeq))]
			private static void MaidPropsSeqLoaded(ref Maid __instance)
			{
				if (__instance.IsAllProcPropBusy)
				{
					return;
				}

				foreach (var manager in ControllerTypes)
				{
					foreach (var controller in manager.ControllerInstances)
					{
						controller.OnMaidRefreshedInternal(__instance);
					}
				}
			}

			[HarmonyPrefix, HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset), typeof(bool))]
			private static void MaidPresetLoading(ref Maid __0)
			{
				foreach (var manager in ControllerTypes)
				{
					foreach (var controller in manager.ControllerInstances)
					{
						controller.OnMaidLoadingInternal(__0);
					}
				}
			}
		}
	}
}