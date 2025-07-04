using UnityEngine;

namespace CodeAnimator
{
	[System.Serializable]
	public class SpanSelector
	{
		public string searchText;
		public int searchNumber;
		public TextSearchType searchType;
		public CaseSensitive Sensitive;
	}

	public enum TextSearchType
	{
		FirstTextMatch,
		AllTextMatches,
		LastTextMatch,
		Line,
		Column,
		HTMLClass,
		HTMLId,
	}

	public enum CaseSensitive
	{
		CaseSensitive,
		NotSensitive
	}
}