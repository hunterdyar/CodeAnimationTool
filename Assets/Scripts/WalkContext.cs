using System;
using System.Collections.Generic;
using System.Text;

namespace CodeAnimator
{
	public class WalkContext
	{
		public int X = 0;
		public int Y = 0;
		public Stack<Span> Spans = new Stack<Span>();
		private StringBuilder _builder = new StringBuilder();
		private List<AtomRenderer> _atoms = new List<AtomRenderer>();
		public Action<char, WalkContext> AddCharacterCallback { get; set; }
		public string Text => _builder.ToString();
		public int SpacesForTabs = 2;

		public void LineBreak()
		{
			this.X = 0;
			this.Y++;
		}

		public Span PushSpan()
		{
			Span span = new Span();
			Spans.Push(span);
			return span;
		}

		public Span PushSpan(Span span)
		{
			Spans.Push(span);
			return span;
		}

		public Span PopSpan()
		{
			return Spans.Pop();
		}
		public void AddCharacter(char character)
		{
			if (Utility.NoRender(character))
			{
				return;
			}
			
			//replace with spaces.
			if (character == '\t')
			{
				for (int i = 0; i < this.SpacesForTabs; i++)
				{
					AddCharacter(' ');
				}
			}
			
			if (character == '\n')
			{
				this.LineBreak();
				return;
			}
			
			this.AddCharacterCallback?.Invoke(character, this);
			_builder.Append(character);
			this.X++;
		}


		public void PopSpans(int scount)
		{
			for (int i = 0; i < scount; i++)
			{
				Spans.Pop();
			}
		}
	}
}