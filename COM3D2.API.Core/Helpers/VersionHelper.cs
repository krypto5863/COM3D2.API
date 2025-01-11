using System;

namespace COM3D2API.Helpers
{
	public static class VersionHelper
	{
		public static bool TryParse(string input, out Version result)
		{
			result = null;

			if (string.IsNullOrEmpty(input))
				return false;

			try
			{
				result = new Version(input);
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (FormatException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}
		}
	}
}