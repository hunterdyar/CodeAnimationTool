using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeAnimator
{
	//Superclass that we actually interface with.
	public class TextRenderer : MonoBehaviour
	{
		public Font Font => font;
		[SerializeField] Font font;
		[TextArea,SerializeField] private string inputText;
		private string _processedText;
		private Dictionary<Vector2Int, AtomRenderer> startingPosRenderers = new Dictionary<Vector2Int, AtomRenderer>();
		private int[] LineLengths;
		private Span[] LineSpans;
		private Span[] ColumnSpans;
		
		//spans are separate, and are just lists of various atoms.
		//We can also search processed text 
		public void Start()
		{
			RenderText();
			
		}

		public void RenderText()
		{
			Clear();
			string spaces = "";
			for (int i = 0; i < font.gapsPerTab; i++)
			{
				spaces += " ";
			}
			_processedText = inputText.Replace("\t", spaces);
			var lines = _processedText.Split('\n');
			int x = 0;
			int y = 0;
			LineSpans = new Span[lines.Length];
			LineLengths = new int[lines.Length];
			ColumnSpans = new Span[lines.Max(x=>x.Length)];
			
			for (int r = 0; r < lines.Length; r++)
			{
				var line = lines[r];
				LineLengths[r] = line.Length;
				LineSpans[r]  = new Span();
				for (int c = 0; c < line.Length; c++)
				{
					if (ColumnSpans[x] == null)
					{
						ColumnSpans[x] = new Span();
					}
					
					var character = line[c];
					
					if (NoRender(character))
					{
						continue;
					}

					if (character == '\t')
					{
						x += Font.gapsPerTab;
						continue;
					}

					if (character == ' ')
					{
						x += 1;
						continue;
					}

					var atom = new Atom(character, TextStyle.Normal);
					var argo = new GameObject();
					argo.transform.SetParent(transform);
					argo.name = character.ToString();
					var ar=argo.AddComponent<AtomRenderer>();
					ar.Init(atom, x, y, this);
					startingPosRenderers.Add(new Vector2Int(x, y), ar);
					
					//register appropriate spans.
					LineSpans[r].AddAtom(ar);
					ColumnSpans[x].AddAtom(ar);
					
					x++;
				}

				y++;
				x = 0;
			}
			
		}

		public Span GetSpanForRange(int startRow, int startColumn, int endRow, int endColumn)
		{
			var s = new Span();
			var r = startRow;
			while (r <= endRow)
			{
				var c = r == startRow ? startColumn : 0;
				var end = r == endRow ? endColumn : LineLengths[r];
				while (c <= end)
				{
					if (startingPosRenderers.TryGetValue(new Vector2Int(r, c), out AtomRenderer value))
					{
						s.AddAtom(value);
					}
					c++;
				}
				r++;
			}

			return s;
		}
		private bool NoRender(char character)
		{
			return character == '\r';
		}

		private void Clear()
		{
			foreach (var atom in startingPosRenderers.Values)
			{
				Destroy(atom.gameObject);
			}

			startingPosRenderers.Clear();
		}

		public void SetColor(Span span, Color color)
		{
			foreach (var atom in span.Atoms)
			{
				atom.SetColor(color);
			}
		}
	}
}