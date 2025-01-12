using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2API.UI
{
	public static class EditModeApi
	{
		internal static void Init()
		{
			RegisterHooks();
		}

		private static void RegisterHooks() => Harmony.CreateAndPatchAll(typeof(Hooks));

		private static readonly List<CustomCategoryButton> CategoryButtonsToCreate = new List<CustomCategoryButton>();

		public static MPN LastKnownMpn { get; private set; } = MPN.null_mpn;

		public static void CreateCategoryButton(string name, Action onClickAction, Action closeCategoryAction, Action<bool> onViewAction = null, Action<GameObject> buttonCreatedCallback = null)
		{
			CategoryButtonsToCreate.Add(new CustomCategoryButton(name, onClickAction, closeCategoryAction, onViewAction, buttonCreatedCallback));
		}

		private static void MakeButtons()
		{
			var grid = GameObject.Find("UI Root/ScrollPanel-Category/Scroll View/UIGrid").GetComponentInChildren<UIGrid>();
			var categoryButton = GameObject.Find("UI Root/ScrollPanel-Category/Scroll View/UIGrid/ButtonCate(Clone)");

			foreach (var customButton in CategoryButtonsToCreate)
			{
				if (customButton.Instance != null)
				{
					continue;
				}

				var newButton = Object.Instantiate(categoryButton, categoryButton.transform.parent);

				customButton.Instance = newButton;

				var buttonEdit = newButton.GetComponentInChildren<ButtonEdit>();
				buttonEdit.m_Category = null;

				var uiLabel = newButton.GetComponentInChildren<UILabel>();
				uiLabel.text = customButton.Name;

				var uiButton = newButton.GetComponentInChildren<UIButton>();
				uiButton.onClick.Clear();
				uiButton.onClick.Add(new EventDelegate(() =>
				{
					CategoryButtonCallback(newButton);
					customButton.OnClickAction();
				}));

				customButton.ButtonCreatedCallback?.Invoke(newButton);
			}

			var allGridChildren = grid.GetChildList();

			foreach (var child in allGridChildren)
			{
				var currentButton = child.GetComponentInChildren<UIButton>();

				foreach (var customButton in CategoryButtonsToCreate)
				{
					if (customButton.Instance?.transform == child)
					{
						continue;
					}

					currentButton.onClick.Add(new EventDelegate( () => DeselectCustomButton(customButton)));
				}
			}

			grid.Reposition();
		}

		private static void DeselectCustomButton(CustomCategoryButton customButton)
		{
			if (customButton.Instance == null)
			{
				return;
			}

			var customUiButton = customButton.Instance.GetComponentInChildren<UIButton>();
			var buttonEdit = customButton.Instance.GetComponentInChildren<ButtonEdit>();

			buttonEdit.m_goFrame.SetActive(false);
			customUiButton.defaultColor = buttonEdit.m_colBtnDefault;

			customButton.OnCloseAction();
		}

		/// <summary>
		/// Handles UI updates when a category button is clicked. It deselects current selections, updates the UI,
		/// and then triggers the user's OnClick callback associated with the button.
		/// </summary>
		/// <param name="button">The clicked button GameObject.</param>
		private static void CategoryButtonCallback(GameObject button)
		{
			// Hide UI elements related to custom parts and categories
			var sceneEdit = SceneEdit.Instance;
			sceneEdit.customPartsWindowVisibleButton.visible = false;
			sceneEdit.m_Panel_Category.VisibleArrow(false); // Hide category arrow
			sceneEdit.CategoryUnSelect(); // Unselect current category
			sceneEdit.colorPaletteMgr.Close(); // Close the color palette

			// Enable the frame on the clicked button and set its alpha to 1 (fully visible)
			var buttonEdit = button.GetComponentInChildren<ButtonEdit>();
			var frameSprite = buttonEdit.m_goFrame.GetComponent<UISprite>();
			frameSprite.enabled = true;
			frameSprite.gameObject.SetActive(true);

			// Update the button's default color to fully visible
			var buttonComponent = button.GetComponentInChildren<UIButton>();
			buttonComponent.defaultColor = new Color(buttonComponent.defaultColor.r, buttonComponent.defaultColor.g, buttonComponent.defaultColor.b, 1f);

			// Reset various panels and toggle off unnecessary UI elements
			sceneEdit.m_maid.OpenMouthLookTooth(false); // Hide open-mouth/teeth details
			HidePanels(sceneEdit); // Custom method to hide all panels
			HideCheckboxes(sceneEdit); // Hide checkboxes and selectors

			if (sceneEdit.m_viewreset.GetVisibleAutoCam())
			{
				sceneEdit.CameraReset();
			}
			sceneEdit.maid.body0.SetMaskMode(TBody.MaskMode.None);
			sceneEdit.SetCameraOffset(SceneEdit.CAM_OFFS.RIGHT);
			BaseMgr<BodyStatusMgr>.Instance.ClosePanel();

			// Update category UI with the new selection and move the arrow to the selected button
			sceneEdit.m_Panel_Category.VisibleArrow(true); // Show category arrow
			sceneEdit.m_Panel_Category.MoveArrow(buttonEdit.gameObject); // Move arrow to the new button

			// Reset the current MPN (menu part number) to null
			LastKnownMpn = sceneEdit.NowMPN;
			sceneEdit.m_nNowMPN = MPN.null_mpn;
		}

		/// <summary>
		/// Hides various panels in the SceneEdit instance.
		/// </summary>
		/// <param name="sceneEdit">The SceneEdit instance to operate on.</param>
		private static void HidePanels(SceneEdit sceneEdit)
		{
			sceneEdit.m_Panel_PartsType.SetActive(false);
			sceneEdit.m_Panel_MenuItem.SetActive(false);
			sceneEdit.m_Panel_SetItem.SetActive(false);
			sceneEdit.m_Panel_SliderItem.SetActive(false);
			sceneEdit.m_Panel_ColorSet.SetActive(false);
			sceneEdit.m_Panel_GroupSet.SetActive(false);
		}

		/// <summary>
		/// Hides checkboxes and selectors in the SceneEdit instance.
		/// </summary>
		/// <param name="sceneEdit">The SceneEdit instance to operate on.</param>
		private static void HideCheckboxes(SceneEdit sceneEdit)
		{
			sceneEdit.m_CheckBoxMayuDrawPriority.gameObject.SetActive(false);
			if (sceneEdit.highlightSelector != null)
			{
				sceneEdit.highlightSelector.visible = false;
			}

			// Close all random and preset panels
			BaseMgr<RandomPresetMgr>.Instance.CloseRandomPresetPanel();
			BaseMgr<PresetMgr>.Instance.ClosePresetPanel();
			BaseMgr<ProfileMgr>.Instance.CloseProfilePanel();
			BaseMgr<CostumePartsEnabledMgr>.Instance.CloseRandomPresetPanel();
		}

		private static class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ToView))]
			private static void ToViewHide()
			{
				foreach (var customButton in CategoryButtonsToCreate)
				{
					customButton.OnViewAction?.Invoke(true);
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.FromView))]
			private static void FromViewShow()
			{
				foreach (var customButton in CategoryButtonsToCreate)
				{
					customButton.OnViewAction?.Invoke(false);
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_Category))]
			private static void CategoryButtonsCreated()
			{
				MakeButtons();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ClickEmulate), typeof(MPN), typeof(bool))]
			private static void CategoryButtonsCreated(bool __result)
			{
				if (__result == false)
				{
					return;
				}

				foreach (var customButton in CategoryButtonsToCreate)
				{
					DeselectCustomButton(customButton);
				}
			}
		}
	}
}