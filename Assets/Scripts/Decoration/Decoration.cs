using System;
using UnityEngine;

namespace CodeAnimator
{
	[Serializable]
	public struct Decoration : IEquatable<Decoration>
	{
		public bool Equals(Decoration other)
		{
			return Underline == other.Underline && UnderlineColor.Equals(other.UnderlineColor) && Highlight == other.Highlight && HighlightColor.Equals(other.HighlightColor) && Outline == other.Outline && OutlineThickness.Equals(other.OutlineThickness);
		}

		public override bool Equals(object obj)
		{
			return obj is Decoration other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Underline, UnderlineColor, Highlight, HighlightColor);
		}

		public static bool operator ==(Decoration left, Decoration right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Decoration left, Decoration right)
		{
			return !left.Equals(right);
		}

		public bool Underline;
		public Color UnderlineColor;
		
		public bool Highlight;
		public Color HighlightColor;

		public bool Outline;
		public Color OutlineColor;
		public float OutlineThickness;

		public static Decoration Lerp(Decoration a, Decoration b, float t)
		{
			Color underlineColor = a.UnderlineColor;
			if (a.Underline == b.Underline)
			{
				underlineColor = Color.Lerp(a.UnderlineColor, b.UnderlineColor,t);
			}else if (b.Underline)
			{
				underlineColor = b.UnderlineColor;
			}
			// else if (a.Underline)
			// {
			// 	underlineColor = a.UnderlineColor;
			// }

			Color highlightColor = a.HighlightColor;
			if (a.Highlight == b.Highlight)
			{
				highlightColor = Color.Lerp(a.HighlightColor, b.HighlightColor, t);
			}
			else if (b.Highlight)
			{
				highlightColor = b.HighlightColor;
			}

			Color outlineColor = a.OutlineColor;
			float outlineThickness = a.OutlineThickness;
			if (a.Outline == b.Outline)
			{
				outlineColor = Color.Lerp(a.OutlineColor, b.OutlineColor, t);
				outlineThickness = Mathf.Lerp(a.OutlineThickness, b.OutlineThickness, t);
			}else if (b.Outline)
			{
				outlineColor = b.OutlineColor;
				outlineThickness = Mathf.Lerp(a.OutlineThickness, b.OutlineThickness, t);
			}//else a...

			return new Decoration()
			{
				Underline = a.Underline || b.Underline,
				UnderlineColor = underlineColor,
				Highlight = a.Highlight || b.Highlight,
				HighlightColor = highlightColor,
				Outline = a.Outline || b.Outline,
				OutlineColor = outlineColor,
				OutlineThickness = outlineThickness,
			};
		}
	}
}