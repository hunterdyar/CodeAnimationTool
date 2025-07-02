using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HtmlAgilityPack;
using Highlight;
using Highlight.Engines;

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
		private List<Span> LineSpans;
		private List<Span> ColumnSpans;
		private Dictionary<string, Span> classSpans = new Dictionary<string, Span>();
		private Dictionary<string, Span> idSpans = new Dictionary<string, Span>();
		
		public string HighlightLangugage;
		//spans are separate, and are just lists of various atoms.
		//We can also search processed text 
		public void Start()
		{
			RenderText();
		}

		private string PreProcessText(string input)
		{
			string spaces = "";
			for (int i = 0; i < font.gapsPerTab; i++)
			{
				spaces += " ";
			}

			var text = input.Replace("/t", spaces);

			if (!String.IsNullOrEmpty(HighlightLangugage))
			{
				var highlighter = new Highlighter(new HtmlEngine(){UseCss = true});//useCss enables inline styles, which we will parse into spans.
				var highlightedCode = highlighter.Highlight(HighlightLangugage, inputText);
				return highlightedCode;
			}

			return text;
		}

		private WalkContext RenderAsHTML(string source)
		{
			var html = new HtmlDocument();
			html.LoadHtml(source);

			var context = new WalkContext();
			context.AddCharacterCallback = AddCharacter;
			WalkHTMLNode(html.DocumentNode, ref context);
			return context;
		}

		private void WalkHTMLNode(HtmlNodeCollection nodes, ref WalkContext context)
		{
			foreach (var node in nodes)
			{
				WalkHTMLNode(node, ref context);
			}
		}

		private void WalkHTMLNode(HtmlNode node, ref WalkContext context)
		{
			if (node == null)
			{
				return;
			}
			
			if (node.Name == "Br" || node.Name == "Break")
			{
				context.LineBreak();
				return;
			}

			if (node.NodeType == HtmlNodeType.Element)
			{
				int scount = 0;
				foreach (var cl in node.GetClasses())
				{
					var s = context.PushSpan();
					classSpans.Add(cl, s);
					scount++;
				}

				if (!string.IsNullOrEmpty(node.Id))
				{
					idSpans.Add(node.Id, context.PushSpan());
					scount++;
				}
				
				var children = node.ChildNodes;
				WalkHTMLNode(children, ref context);
				context.PopSpans(scount);
			}
			else if (node.NodeType == HtmlNodeType.Document)
			{
				var children = node.ChildNodes;
				WalkHTMLNode(children, ref context);
			}
			else if (node.NodeType == HtmlNodeType.Comment)
			{
				
			}
			else if (node.NodeType == HtmlNodeType.Text)
			{
				if (node is HtmlTextNode textNode)
				{
					var text = TextUtility.HtmlDecode(textNode.InnerText);
					
					foreach (var c in text)
					{
						context.AddCharacter(c);
					}

					return;
				}
				else
				{
					Debug.LogWarning("HTML text node is not a text node?");	
				}
			}
		}

		private void RenderText()
		{
			//preprocess and reset
			_processedText = PreProcessText(inputText);
			Clear();

			//Walk! this popualtes contexts and spans, and callback creates children.
			var walk = RenderAsHTML(_processedText);

			//colorize
			var classStyles = new[] { 
				("CsharpStatement", Color.green),
				("CsharpOperator",Color.cyan),
				("CsharpValueType", Color.yellow),
			};
			Debug.Log("Classes Found: "+classSpans.Count);
			foreach (var style in classStyles)
			{
				if (classSpans.TryGetValue(style.Item1, out var span))
				{
					SetColor(span, style.Item2);
				}
			}
			
			//complete.
			Debug.Log(walk.Text);
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
		

		private void Clear()
		{
			foreach (var atom in startingPosRenderers.Values)
			{
				Destroy(atom.gameObject);
			}
			
			LineSpans  = new List<Span>();
			ColumnSpans = new List<Span>();
			startingPosRenderers.Clear();
		}

		public void AddCharacter(char c, WalkContext context)
		{
			var atom = new Atom(c, TextStyle.Normal);
			var argo = new GameObject();
			argo.transform.SetParent(transform);
			argo.name = c.ToString();
			var ar = argo.AddComponent<AtomRenderer>();
			ar.Init(atom, context.X, context.Y, this);

			while (LineSpans.Count <= context.Y)
			{
				LineSpans.Add(new Span());
			}

			while (ColumnSpans.Count <= context.X)
			{
				ColumnSpans.Add(new Span());
			}
			
			LineSpans[context.Y].AddAtom(ar);
			ColumnSpans[context.X].AddAtom(ar);

			foreach (var span in context.Spans)
			{
				span.AddAtom(ar);
			}
			
			if (!startingPosRenderers.ContainsKey(new Vector2Int(context.X, context.Y)))
			{
				startingPosRenderers.Add(new Vector2Int(context.X, context.Y), ar);
			}
			else
			{
				Debug.LogWarning($"Key {context.X}:{context.Y} is already in the list.");
			}
		}

		public void SetColor(Span span, Color color)
		{
			foreach (var atom in span.Atoms)
			{
				atom.SetColor(color);
			}
		}
	}

	public class WalkContext
	{
		public int X = 0;
		public int Y = 0;
		public Stack<Span> Spans = new Stack<Span>();
		private StringBuilder _builder = new StringBuilder();
		private List<AtomRenderer> _atoms = new List<AtomRenderer>();
		public Action<char, WalkContext> AddCharacterCallback { get; set; }
		public string Text => _builder.ToString();
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

		public Span PopSpan()
		{
			return Spans.Pop();
		}
		public void AddCharacter(char character)
		{
			if (TextUtility.NoRender(character))
			{
				return;
			}
			
			if (character == '\t')
			{
				Debug.LogWarning("Tab found in walkContext, tabs should be replaced with spaces before walking html.");
				this.X += 2;
				return;
			}

			if (character == ' ')
			{
				this.X += 1;
				return;
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