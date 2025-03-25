using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace COM3D2API.UI
{
	public static partial class EditModeApi
	{
		private static readonly List<CustomSubMenu> SubmenusToCreate = new List<CustomSubMenu>();

		public static void InjectSubMenu(string name, SceneEditInfo.EMenuCategory category, SceneEditInfo.CCateNameType.EType menuType)
		{
			SubmenusToCreate.Add(new CustomSubMenu(name, category, menuType));
		}

		private static partial class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_PartsType))]
			private static void OnPartsTypeChange(ref SceneEdit.SCategory __0)
			{
				var category = __0;
				foreach (var submenu in SubmenusToCreate.Where(d => d.Category == category.m_eCategory))
				{
					if (__0.m_listPartsType.Exists(m => m.m_ePartsType.Equals(submenu.Name)))
					{
						continue;
					}

					//var partsType = new SceneEdit.SPartsType();

					//__0.m_listPartsType.Add(new SceneEdit.SPartsType(submenu.MenuType, mp, submenu.Name, submenu.Name, false, false, false));
				}
			}
			/*
			[HarmonyPostfix, HarmonyPatch(typeof(SceneEditInfo), nameof(SceneEditInfo.dicPartsTypePair_), MethodType.Getter)]
			private static SceneEdit.SPartsType onFetchMpnPartsType(ref Dictionary<MPN, SceneEditInfo.CCateNameType> __result)
			{
				__result[MPN.null_mpn] = new SceneEditInfo.CCateNameType
				{

				};
			}
			*/
		}
	}
}