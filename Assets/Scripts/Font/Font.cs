using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeAnimator
{
	[System.Serializable]
	public class FontAtlasLookup
	{
		public FontAtlas atlas;
		public FontStyle fontStyle;
	}
	[CreateAssetMenu(fileName = "Font", menuName = "CodeAnim/Font", order = 0)]
	public class Font : ScriptableObject
	{
		private Dictionary<Atom, Sprite> fontLookupCache =  new Dictionary<Atom, Sprite>();
		[SerializeField] public FontAtlasLookup[] Atlases;

		[Header("Font Settings")] public int gapsPerTab = 2;
		public Rect DefaultLetterRect => _defaultLetterRect;
		[SerializeField] private Rect _defaultLetterRect;
		public float aspect => _defaultLetterRect.width / _defaultLetterRect.height;
		
		public bool TryGetSprite(Atom atom, out Sprite sprite)
		{
			if (fontLookupCache.TryGetValue(atom, out sprite))
			{
				return true;
			}

			var fal = Atlases.FirstOrDefault(x => x.fontStyle == atom.FontStyle);
			if (fal == null)
			{
				sprite = null;
				return false;
			}

			if (fal.atlas == null)
			{
				sprite = null;
				return false;
			}

			if (fal.atlas.TryGetSprite(atom.Character,this, out sprite))
			{
				fontLookupCache.Add(atom, sprite);
				return true;
			}
			
			sprite = null;
			return false;
		}

		

		public Rect GetLetterRect(char character)
		{
			//todo overrides for specific letters.
			return _defaultLetterRect;
		}
	}
}