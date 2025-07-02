using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColorCode;
using UnityEngine;
using HtmlAgilityPack;
using UnityEngine.Rendering;

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
		private List<AtomRenderer> renderers = new List<AtomRenderer>();
		private int[] LineLengths;
		private List<Span> LineSpans;
		private List<Span> ColumnSpans;
		private Dictionary<string, Span> classSpans = new Dictionary<string, Span>();
		private Dictionary<string, Dictionary<string, Span>> styles = new Dictionary<string, Dictionary<string, Span>>();
		private Dictionary<string, Span> idSpans = new Dictionary<string, Span>();
		public bool highlightAsLanguage;
		public string HighlightLangugage;

		private Dictionary<SpanSelector, Span> GetSpanCache = new Dictionary<SpanSelector, Span>();
		//oublic bool html
		//spans are separate, and are just lists of various atoms.
		//We can also search processed text 
		public void Start()
		{
			RenderText();
			GetSpanCache.Clear();
		}

		[ContextMenu("Render Text")]
		private void RenderText()
		{
			//preprocess and reset
			_processedText = PreProcessText(inputText);
			Clear();

			//Walk! this popualtes contexts and spans, and callback creates children.
			var walk = RenderAsHTML(_processedText);


			if (styles.TryGetValue("color", out var colors))
			{
				foreach (var kvp in colors)
				{
					var colorName = kvp.Key.Trim().ToLower();
					if (ColorUtility.TryParseHtmlString(colorName, out var color))
					{
						SetDefaultColor(kvp.Value, color);
					}else if (colorName.StartsWith("rgb"))
					{
						//rgb(r,g,b)
						var clean = colorName.Replace("rgb", string.Empty).Replace("(", string.Empty)
							.Replace(")", string.Empty);
						var vals =  clean.Split(',');
						if (vals.Length == 3)
						{
							var r = int.Parse(vals[0]) / 255f;
							var g = int.Parse(vals[1]) / 255f;
							var b = int.Parse(vals[2]) / 255f;
							Color c = new Color(r, g, b);
							SetDefaultColor(kvp.Value, c);
						}
						else
						{
							Debug.Log($"Huh? rgb {vals.Length} from {colorName}");
						}
					}
				}
			} 
		}

		private string PreProcessText(string input)
		{
			string spaces = "";
			for (int i = 0; i < font.gapsPerTab; i++)
			{
				spaces += " ";
			}

			var text = input.Replace("/t", spaces);

			if (highlightAsLanguage)
			{
				//var language = ColorCode.Languages.FindById("java");
				//todo: language selection by name or enum
				var formatter = new HtmlFormatter();
				var highlightedCode = formatter.GetHtmlString(text, Languages.CSharp);
				return highlightedCode;
			}

			return text;
		}

		private WalkContext RenderAsHTML(string source)
		{
			var html = new HtmlDocument();
			html.LoadHtml(source);

			var context = new WalkContext();
			context.SpacesForTabs = Font.gapsPerTab;
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
					if (classSpans.TryGetValue(cl, out var span))
					{
						context.PushSpan(span);
						scount++;
					}
					else
					{
						var s = context.PushSpan();
						classSpans.Add(cl, s);
						scount++;
					}
				}

				var style = node.GetAttributeValue("Style", null);
				if (!string.IsNullOrEmpty(style))
				{
					var styleElements = style.Split(';').Select(x=>x.Trim());
					
					foreach (var styleElement in styleElements)
					{
						if (string.IsNullOrEmpty(styleElement))
						{
							continue;
						}
						var kvp = styleElement.Split(':');
						if (kvp.Length != 2){
							Debug.LogWarning($"{node.Name}: style invalid: {style}");
							continue;
						}
						var key = kvp[0].Trim();
						var styleValue = kvp[1].Trim();
						if (!styles.ContainsKey(key))
						{
							styles.Add(key, new Dictionary<string, Span>());
						}
						if (styles.TryGetValue(key, out var styleKeyDict))
						{
							if (styleKeyDict.TryGetValue(styleValue, out Span styleValueSpan))
							{
								//uhg trying not to do this sort of "reaching in", but testing bugs.
								if (!context.Spans.Contains(styleValueSpan))
								{
									context.PushSpan(styleValueSpan);
									scount++;
								}
							}
							else
							{
								styleKeyDict.Add(styleValue, context.PushSpan());
								scount++;
							}
						}
						else
						{
							Debug.LogError("uh oh");
						}
						
					}
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
			
			//ordered list
			renderers.Add(ar);
			
			//xy indexed list
			if (!startingPosRenderers.ContainsKey(new Vector2Int(context.X, context.Y)))
			{
				startingPosRenderers.Add(new Vector2Int(context.X, context.Y), ar);
			}
			else
			{
				Debug.LogWarning($"Key {context.X}:{context.Y} is already in the list.");
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
		

		private void Clear()
		{
			foreach (var atom in startingPosRenderers.Values)
			{
				Destroy(atom.gameObject);
			}
			
			LineSpans  = new List<Span>();
			ColumnSpans = new List<Span>();
			styles.Clear();
			idSpans.Clear();
			classSpans.Clear();
			renderers.Clear();
			startingPosRenderers.Clear();
		}


		public void SetDefaultColor(Span span, Color color)
		{
			foreach (var atom in span.Atoms)
			{
				atom.SetDefaultColorPercentage(1);
				atom.SetColor(color, true);
			}
		}
		public void SetColor(Span span, Color color, float defaultPercentage)
		{
			foreach (var atom in span.Atoms)
			{
				atom.SetDefaultColorPercentage(defaultPercentage);
				atom.SetColor(color, false);
			}
		}

		public Span GetSpan(SpanSelector selector)
		{
			if (GetSpanCache.TryGetValue(selector, out var cachedSpan))
			{
				return cachedSpan;
			}
			string search = selector.Sensitive == CaseSensitive.CaseSensitive ? selector.searchText : selector.searchText.ToLower();
			Span result = null;
			switch (selector.searchType)
			{
				case TextSearchType.HTMLId:
				{
					if (!idSpans.TryGetValue(search, out result))
					{
						Debug.LogWarning($"ID {search} not found.");
						return null;
					}
					break;
				}	
				case TextSearchType.HTMLClass:
				{
					if (!classSpans.TryGetValue(search, out result))
					{
						Debug.LogWarning($"Class {selector.searchText} not found.");
						return null;
					}
					break;
				}
				case TextSearchType.FirstTextMatch:
					result = TextUtility.GetFirstSubSequence(renderers, search.ToCharArray());
					break;
				case TextSearchType.LastTextMatch:
					result = TextUtility.GetLastSubSequence(renderers, search.ToCharArray());
					break;
				case TextSearchType.AllTextMatches:
					var allSpans = TextUtility.GetAllSubSequence(renderers, search.ToCharArray());
					Span resultSpan = new Span();
					foreach (var span in allSpans)
					{
						resultSpan.AddSpan(span);
					}

					result = resultSpan;
					break;
				default:
					Debug.LogError("Bad selector!");
					return null;
			}
			
			GetSpanCache.Add(selector,result);
			return result;
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
			if (TextUtility.NoRender(character))
			{
				return;
			}
			
			if (character == '\t')
			{
				this.X += this.SpacesForTabs;
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