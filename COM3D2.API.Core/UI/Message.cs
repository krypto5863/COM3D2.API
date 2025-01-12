namespace COM3D2API.UI
{
	internal class Message
	{
		internal int DuplicateCounter = 1;
		internal readonly string Text;
		internal string DisplayText 
		{
			get
			{
				var counter = DuplicateCounter > 1 ? $"[FFFFFF]x{DuplicateCounter}[-]" : string.Empty ;
				return $"{counter} {Text}";
			}
		}
		internal float Duration { get; set; }

		internal Message(string text, float duration)
		{
			Text = text;
			Duration = duration;
		}
	}
}