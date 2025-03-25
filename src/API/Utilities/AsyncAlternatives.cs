using System.Threading.Tasks;
using UnityEngine;

namespace COM3D2API.Utilities
{
	/// <summary>
	/// General thread-safe equivalents of functions in COM.
	/// </summary>
	public static class AsyncAlternatives
	{
		/// <summary>
		/// Thread-safe texture creation. Creates a Texture2D in a background thread and uses the main thread for functions that cannot occur outside of it.
		/// </summary>
		/// <param name="data">Image data</param>
		/// <param name="placeHolder">Texture to load data onto.</param>
		/// <returns></returns>
		/// <remarks>This should be called by the thread you wish to perform work on as it runs on the current thread.</remarks>
		public static async Task<Texture2D> CreateTexture2DAsync(byte[] data, int width, int height, TextureFormat format, Texture2D placeHolder = null)
		{
			// Validate format
			if (format != TextureFormat.DXT1 && format != TextureFormat.DXT5 &&
				format != TextureFormat.ARGB32 && format != TextureFormat.RGB24)
			{
				Debug.LogError($"Unsupported texture format: {format}");
				return null;
			}

			if (data == null || data.Length == 0)
			{
				Debug.LogError("Texture data is invalid.");
				return null;
			}

			placeHolder ??= new Texture2D(0,0);

			// Threaded creation for formats with raw data
			if (format == TextureFormat.DXT1 || format == TextureFormat.DXT5)
			{
				placeHolder.Reinitialize(width, height, format, false);

				var texture = new Texture2D(width, height, format, false);
				texture.LoadRawTextureData(data);
				texture.Apply();

				// Handle RenderTexture and ReadPixels on the main thread
				return await UnityMainThreadDispatcher.Instance.EnqueueAsync(() =>
				{
					var active = RenderTexture.active;
					var tempRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

					Graphics.Blit(texture, tempRT);

					var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
					RenderTexture.active = tempRT;
					result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
					result.Apply();

					RenderTexture.ReleaseTemporary(tempRT);
					RenderTexture.active = active;
					Object.Destroy(texture);
					return result;
				});
			}
			else
			{
				// Handle LoadImage on the main thread
				return await UnityMainThreadDispatcher.Instance.EnqueueAsync(() =>
				{
					placeHolder.Reinitialize(width, height, format, false);
					placeHolder.LoadImage(data);
					return placeHolder;
				});
			}
		}
	}
}