using System;
using UnityEngine;

namespace COM3D2API.Utilities
{
	/// <summary>
	/// A simple class that lets you wait until a condition evaluates to true, and check it at a specific interval to avoid wasting time checking every frame on long waits.
	/// </summary>
	public class TimedWaitUntil : CustomYieldInstruction
	{
		private float _nextCheck;
		private readonly float _checkInterval;
		private readonly Func<bool> _func;
		/// <param name="predicate">The condition to evaluate. Will return on true.</param>
		/// <param name="time">The interval of time between checking the condition.</param>
		public TimedWaitUntil(Func<bool> predicate, float time)
		{
			_func = predicate;
			_checkInterval = time;
			_nextCheck = Time.realtimeSinceStartup + _checkInterval;
		}

		public override bool keepWaiting
		{
			get
			{
				if (!(Time.realtimeSinceStartup > _nextCheck))
				{
					return true;
				}

				if (_func())
				{
					return false;
				}

				_nextCheck = Time.realtimeSinceStartup + _checkInterval;
				return true;
			}
		}
	}
}