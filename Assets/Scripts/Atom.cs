namespace CodeAnimator
{
	[System.Serializable]
	public struct Atom
	{
		public char Character;
		public TextStyle CharacterStyle;

		public Atom(char character, TextStyle normal)
		{
			Character  = character;
			CharacterStyle = normal;
		}
	}

	public enum TextStyle
	{
		Normal,
		Italic,
		Bold,
	}
}