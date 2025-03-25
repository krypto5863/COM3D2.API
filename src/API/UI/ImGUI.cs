using UnityEngine;

namespace COM3D2API.UI
{
	/// <summary>
	/// Contains general useful functions for working with ImGUI.
	/// </summary>
	public class ImGUI
	{
		/// <summary>
		/// Can be used after calling <see cref="GetClickFixClickThrough"/> to check if the input was a left click, right click, or middle mouse input./>
		/// </summary>
		public static int LastMouseButtonUp { private set; get; } = -1;

		/// <summary>
		/// Should be called at the very end of an OnGUI call in order to fix click-through. This function also updates <see cref="LastMouseButtonUp"/>, so you can still handle clicks differently.
		/// </summary>
		/// <param name="windowRect">The root <see cref="Rect"/> of your ImGUI window.</param>
		/// <remarks>
		/// This function should only be called if you're actively drawing your GUI. If the GUI is not being drawn, this should not be called.
		/// </remarks>
		public static void GetClickFixClickThrough(Rect windowRect)
		{
			LastMouseButtonUp = Input.GetMouseButtonUp(0) ? 0 : Input.GetMouseButtonUp(1) ? 1 : Input.GetMouseButtonUp(2) ? 2 : -1;

			if ((Input.mouseScrollDelta.y != 0 || LastMouseButtonUp >= 0) && IsMouseOnGUI(windowRect))
			{
				Input.ResetInputAxes();
			}
		}

		/// <summary>
		/// Useful for checking if your mouse is actively within a <see cref="Rect"/>.
		/// </summary>
		/// <param name="windowRect">The root <see cref="Rect"/> of your ImGUI window.</param>
		/// <returns>True if the mouse is within the <see cref="windowRect"/>. Otherwise, false.</returns>
		/// <remarks>This does not check if the GUI is active or not, this must be taken into account.</remarks>
		public static bool IsMouseOnGUI(Rect windowRect)
		{
			var point = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return windowRect.Contains(point);
		}
	}
}