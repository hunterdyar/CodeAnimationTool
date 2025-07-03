using System.Collections.Generic;
using System.Linq;
using System.Web;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CodeAnimator
{
	public static class Utility
	{
		public static bool NoRender(char character)
		{
			return character == '\r';
		}

		public static string HtmlDecode(string textNodeInnerText)
		{
			return HttpUtility.HtmlDecode(textNodeInnerText);
		}

		public static Span GetFirstSubSequence(List<AtomRenderer> source, char[] search)
		{
			Span s = new Span();
			if (search.Length > source.Count)
			{
				return s; //empty
			}

			var e = Enumerable.Range(0, source.Count - search.Length + 1)
				.Select(a => source.Skip(a).Take(search.Length))
				.FirstOrDefault(a => a.Select(x=>x.Atom.Character).SequenceEqual(search));
			if (e != null)
			{
				foreach (var a in e)
				{
					s.AddAtom(a);
				}
			}

			return s;
		}

		public static Span GetLastSubSequence(List<AtomRenderer> source, char[] search)
		{
			Span s = new Span();
			if (search.Length > source.Count)
			{
				return s; //empty
			}

			var e = Enumerable.Range(0, source.Count - search.Length + 1)
				.Select(a => source.Skip(a).Take(search.Length))
				.LastOrDefault(a => a.Select(x => x.Atom.Character).SequenceEqual(search));
			if (e != null)
			{
				foreach (var a in e)
				{
					s.AddAtom(a);
				}
			}

			return s;
		}

		public static List<Span> GetAllSubSequence(List<AtomRenderer> source, char[] search)
		{
			if (search.Length > source.Count)
			{
				return new List<Span>(); //empty
			}

			var e = Enumerable.Range(0, source.Count - search.Length + 1)
				.Select(a => source.Skip(a).Take(search.Length))
				.Where(a => a.Select(x => x.Atom.Character).SequenceEqual(search));

			var results = new List<Span>();
			
			if (e != null)
			{
				foreach (var a in e)
				{
					var s = new Span();
					foreach (var x in a)
					{
						s.AddAtom(x);
					}
					results.Add(s);
				}
			}
			
			return results;
		}

		public static bool TryParseRGBColor(string input, out Color color)
		{
			input = input.Trim().ToLower();
			if(input.StartsWith("rgb"))
			{
				//rgb(r,g,b)
				var clean = input.Replace("rgb", string.Empty).Replace("(", string.Empty)
					.Replace(")", string.Empty);
				var vals = clean.Split(',');
				if (vals.Length == 3)
				{
					var r = int.Parse(vals[0]) / 255f;
					var g = int.Parse(vals[1]) / 255f;
					var b = int.Parse(vals[2]) / 255f;
					color = new Color(r, g, b);
					return true;
				}
				
			}

			color = Color.white;
			return false;
		}

		public static Color WithAlpha(this Color color, float alpha)
		{
			return new Color(color.r, color.g, color.b, alpha);
		}

		public static Rect GetScreenRect(Camera camera, float z)
		{
			var topLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, z));
			var botRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, z));
			var r = new Rect(topLeft.x, topLeft.y, botRight.x - topLeft.x, botRight.y - topLeft.y);
			return r;
		}
	}
}