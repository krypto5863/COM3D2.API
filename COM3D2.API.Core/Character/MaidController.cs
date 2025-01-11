using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using SaveDataExtended;
using UnityEngine;

namespace COM3D2API.Character
{
	public abstract class MaidController : MonoBehaviour
	{
		public bool Started { get; private set; }
		public MaidControllerManager ControllerManager { get; internal set; }
		public string DataId => ControllerManager.DataId;

		[CanBeNull]
		public Maid Maid
		{
			get; private set;
		}

		[CanBeNull]
		public TBody Body
		{
			get; private set;
		}

		protected virtual void Awake()
		{
			Maid = GetComponent<Maid>();
			Body = GetComponent<TBody>();
		}

		protected virtual void Start()
		{
			Started = true;
			OnMaidBeingLoadedInternal();
		}

		/// <summary>
		/// Get extended data of the current character by using the ID you specified when registering this controller.
		/// </summary>
		public SaveData GetExtendedData()
		{
			if (DataId == null) throw new ArgumentException(nameof(DataId));
			if (Maid == null) throw new ArgumentException(nameof(Maid));

			return Storage.GetMaidDataById(Maid, DataId);
		}

		/// <summary>
		/// Save your custom data to the maid under the ID you specified when registering this controller.
		/// </summary>
		/// <param name="data">Your custom data to be written to the maid. Can be null to remove the data.</param>
		public void SetExtendedData(SaveData data)
		{
			if (DataId == null) throw new ArgumentException(nameof(DataId));
			if (Maid == null) throw new ArgumentException(nameof(Maid));

			Storage.CreateMaidDataWithId(Maid, DataId, data);
		}

		protected virtual void OnMaidRefreshed()
		{
		}

		internal void OnMaidRefreshedInternal()
		{
			if (Maid == null)
			{
				return;
			}
			OnMaidRefreshed();
		}

		internal void OnMaidRefreshedInternal(Maid maid)
		{
			if (maid != Maid)
			{
				return;
			}

			OnMaidRefreshedInternal();
		}

		protected virtual void OnMaidLoading()
		{
		}

		internal void OnMaidLoadingInternal(Maid maid)
		{
			if (maid != Maid)
			{
				return;
			}

			OnMaidLoading();
		}

		protected virtual void OnMaidLoaded(CharacterMgr.PresetType presetType)
		{
		}

		internal void OnMaidBeingLoadedInternal(CharacterMgr.PresetType presetType = CharacterMgr.PresetType.All)
		{
			if (Maid == null)
			{
				return;
			}
			OnMaidLoaded(presetType);
		}

		internal void OnMaidBeingLoadedInternal(Maid maid, CharacterMgr.PresetType presetType)
		{
			if (maid != Maid)
			{
				return;
			}

			OnMaidBeingLoadedInternal(presetType);
		}

		protected virtual void OnMaidSaving(CharacterMgr.PresetType presetType)
		{
		}

		internal void OnMaidSavingInternal()
		{
			if (Maid == null)
			{
				return;
			}
			OnMaidSavingInternal(Maid);
		}

		internal void OnMaidSavingInternal(Maid maid, CharacterMgr.PresetType presetType = CharacterMgr.PresetType.All)
		{
			if (maid != Maid)
			{
				return;
			}

			OnMaidSaving(presetType);
		}

		protected virtual void OnMaidPropChanged(IEnumerable<MaidProp> maidProp)
		{
		}

		internal void OnMaidPropChangedInternal(Maid maid, IEnumerable<MaidProp> maidProp)
		{
			if (maid != Maid)
			{
				return;
			}

			OnMaidPropChanged(maidProp);
		}

		protected virtual void OnDisable()
		{
			OnMaidSaving(CharacterMgr.PresetType.All);
		}

		/// <summary>
		/// Gets all <see cref="MaidProp"/> on the current <see cref="Maid"/> that are contained within the <see cref="CharacterMgr.PresetType"/>.
		/// </summary>
		/// <param name="presetType"></param>
		/// <returns>Collection of relevant <see cref="MaidProp"/> for the given <see cref="CharacterMgr.PresetType"/></returns>
		protected IEnumerable<MaidProp> GetMaidPresetTypeProps(CharacterMgr.PresetType presetType)
		{
			if (Maid == null) yield break; // Exit early if Maid is null

			// Iterate through MaidProps and yield only those that match the preset type
			foreach (var mp in Maid.m_aryMaidProp)
			{
				var isValid = false;

				// Handle each preset type with a switch statement
				switch (presetType)
				{
					case CharacterMgr.PresetType.Body:
						// Body preset has two ranges: 1-80 and 116-123
						if ((mp.idx >= 1 && mp.idx <= 80) || (mp.idx >= 116 && mp.idx <= 123)) isValid = true;
						break;

					case CharacterMgr.PresetType.Wear:
						// Wear preset is a single range: 81-110
						if (mp.idx >= 81 && mp.idx <= 110) isValid = true;
						break;

					default:
						// For other preset types, return all MaidProps
						isValid = true;
						break;
				}

				// Yield the valid MaidProp
				if (isValid) yield return mp;
			}
		}
	}
}