using System;
using System.Collections.Generic;

namespace COM3D2API.Character
{
	public sealed class MaidControllerManager
	{
		public readonly string DataId;
		public readonly Type ControllerType;

		public readonly List<MaidController> ControllerInstances = new List<MaidController>();

		public MaidControllerManager(string dataId, Type controllerType)
		{
			DataId = dataId;
			ControllerType = controllerType;
		}

		public MaidController GetOrAddController(Maid maid)
		{
			var maidController = (MaidController)maid.gameObject.AddComponent(ControllerType);
			maidController.ControllerManager = this;
			ControllerInstances.Add(maidController);
			return maidController;
		}
	}
}