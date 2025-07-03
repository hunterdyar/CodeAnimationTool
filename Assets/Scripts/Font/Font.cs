using System;
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
		
		private Sprite _highlightSprite;
		private Sprite _underlineSprite;
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

		public Sprite GetUnderlineSprite()
		{
			//todo
			if (TryGetSprite(new Atom('_', FontStyle.Normal), out var sprite))
			{
				return sprite;
			}
			throw new NotImplementedException("Underline not implemented.");
		}

		public Sprite GetHighlightSprite()
		{
			//todo: dictionary lookup against the rect.
			if (_highlightSprite != null)
			{
				return _highlightSprite;
			}
			var blankTex = new Texture2D((int)_defaultLetterRect.width, (int)_defaultLetterRect.height, Atlases[0].atlas.TextureFormat, false);
			for (int x = 0; x < _defaultLetterRect.width; x++)
			{
				for (int y = 0; y < _defaultLetterRect.height; y++)
				{
					blankTex.SetPixel(x,y,Color.white);
				}
			}
			blankTex.Apply();
			
			//7/9ths is just a magic number. 
			float realWidth = _defaultLetterRect.width * (7f/9f);
			
			_highlightSprite = Sprite.Create(blankTex, new Rect(0,0, realWidth,_defaultLetterRect.height), new Vector2(0.5f, 0.5f));
			return _highlightSprite;
		}
	}
}