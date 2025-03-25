using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2API.UI
{
	public static partial class EditModeApi
	{
		private static readonly List<CustomInjectedSlider> SlidersToCreate = new List<CustomInjectedSlider>();

		private static readonly List<GameObject> ActiveSliders = new List<GameObject>();

		public static void InjectSlider(string name, string partsType, Action<float> onValueChanged, Func<float> updateValue, float minValue = 0f, float maxValue = 100f, string numberFormat = "##0", Action<GameObject> onSliderCreated = null)
		{
			SlidersToCreate.Add(new CustomInjectedSlider(name, partsType, onValueChanged, updateValue, minValue, maxValue, numberFormat, onSliderCreated));
		}

		private static void MakeSliders(SceneEdit.SPartsType partsTypeMenu)
		{
			var grid = GameObject
				.Find("UI Root/ScrollPanel-Slider/Scroll View/UIGrid")
				.GetComponentInChildren<UIGrid>();

			foreach (var customInjectedSlider in SlidersToCreate)
			{
				if (!customInjectedSlider.TargetPartsType.Equals(partsTypeMenu.m_ePartsType))
				{
					continue;
				}

				var result = CreateSlider(grid.gameObject, customInjectedSlider);
				customInjectedSlider.SliderCreatedCallback?.Invoke(result);
				ActiveSliders.Add(result);
			}

			grid.Reposition();

			UpdateMenuScrollPosition(partsTypeMenu);
		}

		private static GameObject CreateSlider(GameObject parentGrid, CustomInjectedSlider injectedSlider)
		{
			var @object = Resources.Load("SceneEdit/MainMenu/Prefab/Slider");
			var gameObject = NGUITools.AddChild(parentGrid, @object as GameObject);

			var gcSlider = gameObject.GetComponentsInChildren<UISlider>(true)[0];
			gcSlider.onChange.Clear();

			gcSlider.value = injectedSlider.UpdateValue();

			var childObject = UTY.GetChildObject(gameObject, "CategoryTitle/Name");
			var gcLabel = childObject.GetComponent<UILabel>();
			gcLabel.color = Color.yellow;
			gcLabel.text = injectedSlider.Name;

			var childObject2 = UTY.GetChildObject(gameObject, "CategoryTitle/Number/Value");
			var gcValue = childObject2.GetComponent<UILabel>();
			//gcValue.text = "000";

			var numberObject = childObject2.transform.parent;
			NGUITools.AddWidgetCollider(numberObject.gameObject);
			var input = numberObject.GetOrAddComponent<UIInput>();
			input.label = gcValue;
			input.validation = UIInput.Validation.Float;
			input.activeTextColor = gcValue.color;
			input.caretColor = gcValue.color;
			input.selectionColor = Color.grey;
			input.selectionColor.a = 0.33f;

			input.onSubmit.Add(new EventDelegate(() =>
			{
				if (!float.TryParse(input.value, out var value))
				{
					input.value = gcSlider.value.ToString(injectedSlider.NumberFormat);
				}

				gcSlider.value = Extensions.Normalize(value, injectedSlider.MinValue, injectedSlider.MaxValue); ;
			}));

			gcSlider.onChange.Add(new EventDelegate(() =>
			{
				var value = Mathf.Lerp(injectedSlider.MinValue, injectedSlider.MaxValue, gcSlider.value);
				input.value = value.ToString(injectedSlider.NumberFormat);
				injectedSlider.OnValueChanged(value);
			}));

			gameObject.name = injectedSlider.Name;

			injectedSlider.Instance = gameObject;

			return gameObject;
		}

		private static void UpdateMenuScrollPosition(SceneEdit.SPartsType partsTypeMenu)
		{
			var sceneEdit = SceneEdit.Instance;
			sceneEdit.m_Panel_SliderItem.ResetScrollPos(0f);

			if (!SceneEdit.Instance.backup_category_scroll_pos_.TryGetValue(partsTypeMenu.m_strPartsTypeName,
				    out var value))
			{
				return;
			}

			if (sceneEdit.m_Panel_MenuItem.gcScrollBar.transform.parent.gameObject.activeSelf)
			{
				sceneEdit.m_Panel_MenuItem.gcScrollBar.value = value;
			}
			if (sceneEdit.m_Panel_SetItem.gcScrollBar.transform.parent.gameObject.activeSelf)
			{
				sceneEdit.m_Panel_SetItem.gcScrollBar.value = value;
			}
		}

		private static void ClearSliders()
		{
			foreach (var gameObject in ActiveSliders)
			{
				Object.DestroyImmediate(gameObject);
			}
		}

		private static partial class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_MenuItem))]
			private static void OnMenuItemsChanging(ref SceneEdit.SPartsType __0)
			{
				if (__0.m_eType != SceneEditInfo.CCateNameType.EType.Slider)
				{
					return;
				}

				MakeSliders(__0);
			}

			[HarmonyPrefix,
			 HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_MenuItem)),
			 HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_PartsType))]
			private static void OnMenuItemsChanging() => ClearSliders();
		}
	}
}