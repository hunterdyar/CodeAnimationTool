using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeAnimator
{
	public static class TextUtility
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
	}
}