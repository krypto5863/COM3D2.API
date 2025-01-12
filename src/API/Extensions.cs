using System;
using System.Collections.Generic;
using System.Text;

namespace COM3D2API
{
	public static class Extensions
	{
		public static bool TryRemoveAt<T>(this List<T> list, int index)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list)); // Ensure the list isn't null

			if (index >= 0 && index < list.Count)
			{
				list.RemoveAt(index);
				return true;
			}

			return false;
		}

		public static void AddOrReplace<T>(this List<T> list, int index, T newItem)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list)); // Check if the list is null

			if (index >= 0 && index < list.Count)
			{
				// Replace the item if the index is valid
				list[index] = newItem;
			}
			else
			{
				// Add the item if the index is invalid
				list.Add(newItem);
			}
		}

		/// <summary>
		/// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another
		/// specified string according the type of search to use for the specified string.
		/// </summary>
		/// <param name="str">The string performing the replace method.</param>
		/// <param name="oldValue">The string to be replaced.</param>
		/// <param name="newValue">The string replace all occurrences of <paramref name="oldValue"/>.
		/// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue"/> will be removed from the <paramref name="str"/>.</param>
		/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
		/// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>.
		/// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</returns>
		public static string Replace(this string str,
			string oldValue, string newValue,
			StringComparison comparisonType)
		{
			if (str == null)
			{
				throw new ArgumentNullException(nameof(str));
			}
			if (oldValue == null)
			{
				throw new ArgumentNullException(nameof(oldValue));
			}
			if (oldValue.Length == 0)
			{
				throw new ArgumentException("String cannot be of zero length.");
			}
			if (str.Length == 0)
			{
				return str;
			}

			var resultStringBuilder = new StringBuilder(str.Length);

			var isReplacementNullOrEmpty = string.IsNullOrEmpty(newValue);

			const int valueNotFound = -1;
			int foundAt;
			var startSearchFromIndex = 0;
			while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
			{
				var charsUntilReplacement = foundAt - startSearchFromIndex;
				var isNothingToAppend = charsUntilReplacement == 0;
				if (!isNothingToAppend)
				{
					resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacement);
				}

				if (!isReplacementNullOrEmpty)
				{
					resultStringBuilder.Append(newValue);
				}

				startSearchFromIndex = foundAt + oldValue.Length;
				if (startSearchFromIndex == str.Length)
				{
					return resultStringBuilder.ToString();
				}
			}

			var charsUntilStringEnd = str.Length - startSearchFromIndex;
			resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

			return resultStringBuilder.ToString();
		}
	}
}