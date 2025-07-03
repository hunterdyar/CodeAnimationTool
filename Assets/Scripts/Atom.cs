namespace CodeAnimator
{
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