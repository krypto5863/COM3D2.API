using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2API.UI
{
	public static class MessageApi
	{
		private static CornerText _mainCornerText;
		internal static void Init()
		{
			SceneManager.sceneLoaded += InitOnScene;
		}

		private static void InitOnScene(Scene arg0, LoadSceneMode loadSceneMode)
		{
			_mainCornerText = CornerText.GetOrCreateCornerText();

			if (_mainCornerText == null)
			{
				return;
			}

			Object.DontDestroyOnLoad(_mainCornerText);
			SceneManager.sceneLoaded -= InitOnScene;
		}

		public static void QueueCornerText(string message, float messageDuration = 6f) =>
			_mainCornerText.QueueMessage(message, messageDuration);
	}
}