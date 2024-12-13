using JetBrains.Annotations;
using System;
using UnityEngine;

namespace COM3D2API.UI
{
	internal sealed class CustomCategoryButton
	{
		internal readonly string Name;
		internal readonly Action OnClickAction;
		internal readonly Action OnCloseAction;

		[CanBeNull]
		internal readonly Action<bool> OnViewAction;

		[CanBeNull]
		internal readonly Action<GameObject> ButtonCreatedCallback;

		[CanBeNull]
		public GameObject Instance { get; internal set; }

		public CustomCategoryButton(string name, Action onClickAction, Action onCloseAction, [CanBeNull] Action<bool> onViewAction = null, [CanBeNull] Action<GameObject> buttonCreatedCallback = null)
		{
			Name = name;
			OnClickAction = onClickAction;
			OnCloseAction = onCloseAction;
			OnViewAction = onViewAction;
			ButtonCreatedCallback = buttonCreatedCallback;
		}
	}
}