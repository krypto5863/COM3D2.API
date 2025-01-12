using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2API
{
	/// <summary>
	/// API for adding buttons to the SystemShortcut menu (gear icon)
	/// </summary>
	public static class SystemShortcutAPI
	{
		private static bool _didSystemShortcutHook;
		private static readonly List<ButtonData> ButtonsToCreate = new List<ButtonData>();
		private static SystemShortcut _systemShortcutInstance;
		private static GameObject _configButton;
		private static UISprite _uiShortcutSprite;
		private static BoxCollider _uiBoxCollider;
		private static UIGrid _uiShortcutGrid;
		private static UILabel _tooltipLabel;
		private static UISprite _tooltipSprite;

		internal static void RegisterHooks() => Harmony.CreateAndPatchAll(typeof(Hooks));

		/// <summary>
		/// Add a button to the SystemShortcut menu
		/// </summary>
		/// <param name="name">Name of the button</param>
		/// <param name="onClickEvent">Event to trigger when button is clicked</param>
		/// <param name="tooltipText">Text to show on mouse over</param>
		/// <param name="textureBytes">byte array containing image data for the icon</param>
		public static void AddButton(string name, Action onClickEvent, string tooltipText, byte[] textureBytes = null)
		{
			//If the SystemShortcut.Awake hook has already run add the button immediately, otherwise save the data which will run when the hook does
			if (_didSystemShortcutHook)
				CreateButton(name, onClickEvent, tooltipText, textureBytes);
			else
				ButtonsToCreate.Add(new ButtonData(name, onClickEvent, tooltipText, textureBytes));
		}

		/// <summary>
		/// Create a copy of a SystemShortcut button and wire it up
		/// </summary>
		private static void CreateButton(string name, Action onClickEvent, string tooltipText, byte[] textureBytes)
		{
			try
			{
				//Duplicate the config button
				var buttonCopy = Object.Instantiate(_configButton, _uiShortcutGrid.transform, true);
				buttonCopy.name = name;

				//Replace the onClick event
				var button = buttonCopy.GetComponent<UIButton>();
				button.onClick.Clear();
				EventDelegate.Add(button.onClick, () => onClickEvent());

				//Replace the UIEventTrigger events
				var uiEventTrigger = buttonCopy.GetComponent<UIEventTrigger>();
				uiEventTrigger.onHoverOver.Clear();
				uiEventTrigger.onHoverOut.Clear();
				uiEventTrigger.onDragStart.Clear();
				EventDelegate.Add(uiEventTrigger.onHoverOver, () => ShowTooltip(tooltipText));
				EventDelegate.Add(uiEventTrigger.onHoverOut, HideTooltip);
				EventDelegate.Add(uiEventTrigger.onDragStart, HideTooltip);

				//Add the icon
				if (textureBytes != null)
				{
					var tex = new Texture2D(32, 32, TextureFormat.BC7, false);
					tex.LoadImage(textureBytes);

					//Hide the original sprite
					var uiSprite = buttonCopy.GetComponent<UISprite>();
					uiSprite.type = UIBasicSprite.Type.Filled;
					uiSprite.fillAmount = 0.0f;

					//Add the texture
					var uiTexture = NGUITools.AddWidget<UITexture>(buttonCopy);
					uiTexture.material = new Material(uiTexture.shader)
					{
						mainTexture = tex
					};
					uiTexture.MakePixelPerfect();
				}

				//Makes the base sprite bigger so it fits our new stuff
				_uiShortcutSprite.width += (int)_uiShortcutGrid.cellWidth;
				_uiShortcutGrid.Reposition();
				//Updates the collider so the menu doesn't hide improperly.
				NGUITools.UpdateWidgetCollider(_uiShortcutSprite.gameObject);
			}
			catch (Exception ex)
			{
				//Catch and show the error manually so errors in a single plugin don't break all the others
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Custom tooltip method because SystemShortcut.VisibleExplanation sends the text through LocalizationManager.GetTranslation and returns nothing if not found, apparently
		/// </summary>
		/// <param name="text">Tooltip text</param>
		private static void ShowTooltip(string text)
		{
			_tooltipLabel.text = text;
			_tooltipLabel.width = 0;
			_tooltipLabel.MakePixelPerfect();
			_tooltipSprite.width = _tooltipLabel.width + 15;
			_tooltipSprite.gameObject.SetActive(true);
		}

		/// <summary>
		/// Hide the tooltip
		/// </summary>
		private static void HideTooltip() => _tooltipSprite.gameObject.SetActive(false);

		private static class Hooks
		{
			[HarmonyPatch(typeof(SystemShortcut), "Start")]
			[HarmonyPatch(typeof(SystemShortcut), "OnActiveSceneChanged")]
			[HarmonyPostfix]
			private static void UpdateCollider(SystemShortcut __instance)
			{
				//Kiss is actually retarded and they do some weird manual adjustment of the collider. We override their work after.
				NGUITools.UpdateWidgetCollider(_uiShortcutSprite.gameObject);
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SystemShortcut), "Awake")]
			private static void SystemShortcut_Awake(SystemShortcut __instance)
			{
				_systemShortcutInstance = __instance;

				try
				{
					_uiBoxCollider = _systemShortcutInstance.GetComponentInChildren<BoxCollider>();
					_uiShortcutSprite = _systemShortcutInstance.GetComponentInChildren<UISprite>();
					_configButton = _systemShortcutInstance.transform.Find("Base/Grid/Config").gameObject;
					_uiShortcutGrid = _systemShortcutInstance.GetComponentInChildren<UIGrid>();
					_tooltipLabel = (UILabel)Traverse.Create(_systemShortcutInstance).Field("m_labelExplanation").GetValue();
					_tooltipSprite = (UISprite)Traverse.Create(_systemShortcutInstance).Field("m_spriteExplanation").GetValue();

					foreach (var button in ButtonsToCreate)
						CreateButton(button.Name, button.OnClickEvent, button.TooltipText, button.TextureBytes);
					ButtonsToCreate.Clear();

					_didSystemShortcutHook = true;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		internal class ButtonData
		{
			public readonly string Name;
			public readonly Action OnClickEvent;
			public readonly string TooltipText;
			public readonly byte[] TextureBytes;

			public ButtonData(string name, Action onClickEvent, string tooltipText, byte[] textureBytes)
			{
				Name = name;
				OnClickEvent = onClickEvent;
				TooltipText = tooltipText;
				TextureBytes = textureBytes;
			}
		}
	}
}