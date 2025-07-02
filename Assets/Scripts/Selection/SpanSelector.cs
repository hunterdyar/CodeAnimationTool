namespace CodeAnimator
{
	[System.Serializable]
	public class SpanSelector
	{
		public string searchText;
		public TextSearchType searchType;
		public CaseSensitive Sensitive;
	}

	public enum TextSearchType
	{
		FirstTextMatch,
		AllTextMatches,
		LastTextMatch,
		HTMLClass,
		HTMLId,
	}

	public enum CaseSensitive
	{
		CaseSensitive,
		NotInsensitive
	}
}