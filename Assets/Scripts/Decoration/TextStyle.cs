using UnityEngine;

namespace CodeAnimator
{
	/// <summary>
	/// Text Style is effects that is rendered by a single atomRenderer, the one that is rendering the character, that can change.
	/// italic/bold is not here, it's a font style, because (I decided) that those won't be animated.
	///
	/// Alpha is separate from color so we can fade a single span that uses various colors.
	/// well, fine. we can't right now, but I'm getting there.
	/// </summary>
	[System.Serializable]
	public struct TextStyle
	{
		public bool SetColor;
		public Color Color;
		[Range(0f, 1f)]
		public float Alpha;

		public TextStyle(Color color, float alpha)
		{
			this.SetColor = true;
			this.Color = color;
			this.Alpha = alpha;
		}

		public TextStyle(Color color)
		{
			this.SetColor = true;
			this.Color = color;
			this.Alpha = 1.0f;
		}

		public Color GetColorWithAlpha()
		{
			if (Color.a != Alpha)
			{
				return new  Color(Color.r, Color.g, Color.b, Alpha);
			}
			else
			{
				return Color;
			}
		}

		public static TextStyle Lerp(TextStyle a, TextStyle b, float t)
		{
			Color col = Color.Lerp(a.Color, b.Color, t);
			float alpha = Mathf.Lerp(a.Alpha, b.Alpha, t);
			return new TextStyle(col, alpha);
		}
	}
}