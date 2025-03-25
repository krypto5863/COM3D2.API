using JetBrains.Annotations;
using System;
using UnityEngine;

namespace COM3D2API.UI
{
	internal sealed class CustomInjectedSlider
	{
		internal readonly string TargetPartsType;
		internal readonly string Name;
		internal readonly Func<float> UpdateValue;
		internal readonly Action<float> OnValueChanged;
		internal readonly string NumberFormat;
		internal readonly float MaxValue;
		internal readonly float MinValue;

		[CanBeNull]
		internal readonly Action<GameObject> SliderCreatedCallback;

		[CanBeNull]
		public GameObject Instance { get; internal set; }

		public CustomInjectedSlider(string name, string targetPartsType, Action<float> onValueChanged, Func<float> updateValue, float minValue, float maxValue, string numberFormat, [CanBeNull] Action<GameObject> sliderCreatedCallback = null)
		{
			Name = name;
			TargetPartsType = targetPartsType;
			UpdateValue = updateValue;
			OnValueChanged = onValueChanged;
			NumberFormat = numberFormat;
			MaxValue = maxValue;
			MinValue = minValue;
			SliderCreatedCallback = sliderCreatedCallback;
		}
	}
}