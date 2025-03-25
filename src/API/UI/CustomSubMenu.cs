using JetBrains.Annotations;
using UnityEngine;

namespace COM3D2API.UI
{
	public class CustomSubMenu
	{
		internal readonly string Name;
		internal readonly SceneEditInfo.EMenuCategory Category;
		internal readonly SceneEditInfo.CCateNameType.EType MenuType;

		[CanBeNull]
		internal readonly GameObject ButtonInstance;

		public CustomSubMenu(string name, SceneEditInfo.EMenuCategory category, SceneEditInfo.CCateNameType.EType menuType)
		{
			Name = name;
			Category = category;
			MenuType = menuType;
		}
	}
}