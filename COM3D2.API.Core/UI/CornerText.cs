using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2API.UI
{
	internal class CornerText : MonoBehaviour
	{
		internal static CornerText GetOrCreateCornerText()
		{
			var uiRoot = GameObject
				.Find("SystemUI Root")?
				.GetComponent<UIRoot>();

			if (uiRoot == null)
			{
				return null;
			}

			var newCornerText = NGUITools.AddChild<CornerText>(uiRoot.gameObject);

			return newCornerText;
		}

		private const int MaxMessageQueue = 18;
		private const int MessageDisplayLimit = 9;
		private int _overflowCounter;
		private readonly List<Message> _messageQueue = new List<Message>();
		private UILabel _screenText;

		private void Awake()
		{
			_screenText = gameObject.AddComponent<UILabel>();

			var messageWindow = GameObject
				.Find("SystemUI Root")
				.GetComponentsInChildren<Transform>(true)
				.First(so => so && so.gameObject && so.name.Equals("SystemDialog"))
				.gameObject;

			var msgWindowFont = messageWindow.GetComponentInChildren<UILabel>().trueTypeFont;

			var width = UIRoot.GetPixelSizeAdjustment(_screenText.gameObject) * Screen.width;
			var height = UIRoot.GetPixelSizeAdjustment(_screenText.gameObject) * Screen.height;

			_screenText.trueTypeFont = msgWindowFont;
			_screenText.transform.localPosition = new Vector3(20, height * 0.43f, 0);
			_screenText.width = (int)width;
			_screenText.fontSize = 19;
			_screenText.color = Color.white;
			_screenText.pivot = UIWidget.Pivot.TopLeft;
			_screenText.effectStyle = UILabel.Effect.Outline;
			_screenText.alignment = NGUIText.Alignment.Left;
			_screenText.overflowMethod = UILabel.Overflow.ResizeHeight;
			_screenText.supportEncoding = true;
			_screenText.keepCrispWhenShrunk = UILabel.Crispness.Always;

			gameObject.SetActive(false);
		}

		internal void QueueMessage(string message, float duration)
		{
			message = message.Trim();

			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			if (_messageQueue.Count >= MaxMessageQueue)
			{
				_overflowCounter++;
				UpdateText();
				return;
			}

			var existingMessage = _messageQueue.Find(m => m.Text.Equals(message));
			if (existingMessage != null)
			{
				existingMessage.DuplicateCounter += 1;
				existingMessage.Duration = duration;
			}
			else
			{
				_messageQueue.Add(new Message(message, duration));
			}

			UpdateText();

			gameObject.SetActive(true);
		}

		private void Update()
		{
			if (_screenText.isVisible == false || GameMain.Instance.MainCamera.IsFadeStateNon() == false)
			{
				return;
			}

			var timeToSubtract = Time.unscaledDeltaTime;
			var updateText = false;

			for (var i = 0; i < _messageQueue.Count && i < MessageDisplayLimit; i++)
			{
				var message = _messageQueue[i];
				message.Duration -= timeToSubtract;

				if (message.Duration > 0)
				{
					continue;
				}

				_messageQueue.RemoveAt(i);
				updateText = true;

				i--;
			}

			if (updateText)
			{
				UpdateText();
			}

			if (_messageQueue.Count == 0)
			{
				_overflowCounter = 0;
				gameObject.SetActive(false);
			}
		}

		private void UpdateText()
		{
			var newScreenText = string.Empty;

			var firstFewMessages = _messageQueue.Take(MessageDisplayLimit);
			foreach (var message in firstFewMessages)
			{
				newScreenText += message.DisplayText + "\n";
			}
			if (_overflowCounter > 0)
			{
				newScreenText += $"[ffffff]and {_overflowCounter} more messages...[-]";
			}

			//Localize can suck it.
			_screenText.Wrap(newScreenText, out newScreenText);
			_screenText.text = newScreenText;
			_screenText.overflowMethod = UILabel.Overflow.ResizeHeight;
		}
	}
}