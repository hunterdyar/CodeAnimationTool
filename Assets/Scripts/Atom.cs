namespace CodeAnimator
{
	/// <summary>
	/// An atom is a single character of text that we pull from a font atlas. THis means it's a character and style (bold, italic, etc).
	/// </summary>
	[System.Serializable]
	public struct Atom
	{
		public char Character;
		public FontStyle FontStyle;

		public Atom(char character, FontStyle normal)
		{
			Character  = character;
			FontStyle = normal;
		}
	}

	public enum FontStyle
	{
		Normal,
		Italic,
		Bold,
	}
}