using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeAnimator
{
	[CreateAssetMenu(fileName = "FontAtlas", menuName = "CodeAnim/FontAtlas", order = 0)]
	public class FontAtlas : ScriptableObject
	{
		[SerializeField] Texture2D atlas;
		[SerializeField] int cellsPerRow = 16;
		[SerializeField] int cellsPerColumn = 16;
		[SerializeField] string charsetAsString;
		[SerializeField] private TextureFormat spriteCreationFormat;


		public bool TryGetSprite(char character, Font font, out Sprite sprite)
		{
			var index = charsetAsString.IndexOf(character);

			if (index < 0)
			{
				sprite = null;
				return false;
			}
			//is index two too many because of... utf-8??? what the fudge>
			var row =  cellsPerColumn - Mathf.FloorToInt((index/2) / cellsPerColumn) - 1;
			var column = ((index/2) % cellsPerRow);
			int cellWidth = atlas.width / cellsPerRow;
			int cellHeight =  atlas.height / cellsPerColumn;
			var y = row * cellHeight;
			var x = column * cellWidth;
			
			//with that, we can make the sprite.
			var s = new Texture2D(cellWidth,cellHeight, spriteCreationFormat, false);

			var rect = font.GetLetterRect(character);
			
			s.CopyPixels(atlas,0,0,x,y,cellWidth,cellHeight,0,0,0);
			s.Apply();
			sprite = Sprite.Create(s,rect, new Vector2(0.5f,0.5f));
			return true;
		}

		
		private void OnValidate()
		{
			if (!SystemInfo.SupportsTextureFormat(spriteCreationFormat))
			{
				Debug.LogError($"System does not support texture format: {spriteCreationFormat}");
			}
		}
	}
}