using System.Collections.Generic;

namespace CodeAnimator
{
	public class Span
	{
		private List<AtomRenderer> atoms = new List<AtomRenderer>();
		public IEnumerable<AtomRenderer> Atoms => atoms;
		public void AddAtom(AtomRenderer atom)
		{
			atoms.Add(atom);
		}

		public void AddSpan(Span span)
		{
			if (span == null || span.Atoms == null)
			{
				return;
			}

			foreach (var atom in span.Atoms)
			{
				atoms.Add(atom);
			}
		}
	}
}