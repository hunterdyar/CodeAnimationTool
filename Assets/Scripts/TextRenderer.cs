using System;
using System.Collections.Generic;
using System.Linq;
using ColorCode;
using UnityEngine;
using HtmlAgilityPack;
using UnityEditor.Searcher;

namespace CodeAnimator
{
	//Superclass that we actually interface with.
	public class TextRenderer : MonoBehaviour
	{
		public Font Font => font;
		[SerializeField] Font font;
		[TextArea,SerializeField] private string inputText;
		private string _processedText;
		private readonly Dictionary<Vector2Int, AtomRenderer> _startingPosRenderers = new Dictionary<Vector2Int, AtomRenderer>();
		private readonly List<AtomRenderer> _renderers = new List<AtomRenderer>();
		private int[] _lineLengths;
		private List<Span> _lineSpans;
		private List<Span> _columnSpans;
		private readonly Dictionary<string, Span> _htmlClassSpans = new Dictionary<string, Span>();
		private readonly Dictionary<string, Dictionary<string, Span>> _styles = new Dictionary<string, Dictionary<string, Span>>();
		private readonly Dictionary<string, Span> _htmlIdSpans = new Dictionary<string, Span>();
		public bool highlightAsLanguage;
		public string HighlightLanguage;
		private float _maxRowWidth;
		private int _maxCol;
		private int _maxRow;
		private bool _isLayoutDirty;
		private readonly Dictionary<SpanSelector, Span> _spanCache = new Dictionary<SpanSelector, Span>();
		private readonly Dictionary<Span, DecorationRenderer> _decorationRenderers = new Dictionary<Span, DecorationRenderer>();
		//spans are separate, and are just lists of various atoms.
		//We can also search processed text 

		[Header("Positioning")] public bool CenterOnStart = true;
		[Range(0,1)]
		public float ScaleToScreen = 0.8f;
		public void Start()
		{
			RenderText();
			_spanCache.Clear();

			if (CenterOnStart)
			{
				//center
				var r = Utility.GetScreenRect(Camera.main, 10);
				SetAndScaleSelfIntoWorldRect(r, ScaleToScreen);
			}
		}

		private void Update()
		{
			if (_isLayoutDirty)
			{
				foreach (var renderer in _renderers)
				{
					renderer.UpdatePosition();
				}
				_isLayoutDirty = false;
			}
		}

		[ContextMenu("Render Text")]
		private void RenderText()
		{
			//preprocess and reset
			_processedText = PreProcessText(inputText);
			Clear();

			//Walk! this populates contexts and spans, and callback creates children.
			RenderAsHTML(_processedText);

			if (!_styles.TryGetValue("color", out var colors))
			{
				return;
			}
			
			foreach (var kvp in colors)
			{
				string colorName = kvp.Key.Trim().ToLower();
				if (ColorUtility.TryParseHtmlString(colorName, out var color))
				{
					SetDefaultStyle(kvp.Value, new TextStyle(color));
				}else if (Utility.TryParseRGBColor(colorName, out var color2))
				{
					SetDefaultStyle(kvp.Value, new TextStyle(color2));
				}
				else
				{
					Debug.LogError($"Parse error. Bad color: {colorName}");
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

			var context = new WalkContext
			{
				SpacesForTabs = Font.gapsPerTab,
				AddCharacterCallback = AddCharacter
			};
			
			WalkHTMLNode(html.DocumentNode, ref context);
			return context;
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
				int sCount = 0;
				foreach (var cl in node.GetClasses())
				{
					if (_htmlClassSpans.TryGetValue(cl, out var span))
					{
						context.PushSpan(span);
						sCount++;
					}
					else
					{
						var s = context.PushSpan();
						_htmlClassSpans.Add(cl, s);
						sCount++;
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
						if (!_styles.ContainsKey(key))
						{
							_styles.Add(key, new Dictionary<string, Span>());
						}
						if (_styles.TryGetValue(key, out var styleKeyDict))
						{
							if (styleKeyDict.TryGetValue(styleValue, out Span styleValueSpan))
							{
								//uhg trying not to do this sort of "reaching in", but testing bugs.
								if (!context.Spans.Contains(styleValueSpan))
								{
									context.PushSpan(styleValueSpan);
									sCount++;
								}
							}
							else
							{
								styleKeyDict.Add(styleValue, context.PushSpan());
								sCount++;
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
					_htmlIdSpans.Add(node.Id, context.PushSpan());
					sCount++;
				}
				
				var children = node.ChildNodes;
				WalkHTMLNode(children, ref context);
				context.PopSpans(sCount);
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
					var text = Utility.HtmlDecode(textNode.InnerText);
					
					foreach (var c in text)
					{
						context.AddCharacter(c);
					}
				}
				else
				{
					Debug.LogWarning("HTML text node is not a text node?");	
				}
			}
		}

		private void WalkHTMLNode(HtmlNodeCollection nodes, ref WalkContext context)
		{
			foreach (var node in nodes)
			{
				WalkHTMLNode(node, ref context);
			}
		}

		private void AddCharacter(char c, WalkContext context)
		{
			var atom = new Atom(c, FontStyle.Normal);
			var argo = new GameObject();
			argo.transform.SetParent(transform);
			argo.name = c.ToString();
			var ar = argo.AddComponent<AtomRenderer>();
			ar.Init(atom, context.X, context.Y, this);

			while (_lineSpans.Count <= context.Y)
			{
				_lineSpans.Add(new Span());
			}

			while (_columnSpans.Count <= context.X)
			{
				_columnSpans.Add(new Span());
			}

			_lineSpans[context.Y].AddAtom(ar);
			_columnSpans[context.X].AddAtom(ar);

			foreach (var span in context.Spans)
			{
				span.AddAtom(ar);
			}
			
			//ordered list
			_renderers.Add(ar);
			
			//xy indexed list
			if (!_startingPosRenderers.ContainsKey(new Vector2Int(context.X, context.Y)))
			{
				_startingPosRenderers.Add(new Vector2Int(context.X, context.Y), ar);
			}
			else
			{
				Debug.LogWarning($"Key {context.X}:{context.Y} is already in the list.");
			}

			_maxRow = Mathf.Max(_maxRow, context.Y);
			_maxCol = Mathf.Max(_maxCol, context.X);
		}
		
		public Span GetSpanForRange(int startRow, int startColumn, int endRow, int endColumn)
		{
			var s = new Span();
			var r = startRow;
			while (r <= endRow)
			{
				var c = r == startRow ? startColumn : 0;
				var end = r == endRow ? endColumn : _lineLengths[r];
				while (c <= end)
				{
					if (_startingPosRenderers.TryGetValue(new Vector2Int(r, c), out AtomRenderer value))
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
			foreach (var atom in _startingPosRenderers.Values)
			{
				Destroy(atom.gameObject);
			}
			
			_lineSpans  = new List<Span>();
			_columnSpans = new List<Span>();
			_styles.Clear();
			_htmlIdSpans.Clear();
			_htmlClassSpans.Clear();
			_renderers.Clear();
			_spanCache.Clear();
			_startingPosRenderers.Clear();
		}

		private static void SetDefaultStyle(Span span, TextStyle style)
		{
			foreach (var atom in span.Atoms)
			{
				atom.SetDefaultColorPercentage(1);
				atom.SetStyle(style, true);
			}
		}

		public static void SetStyle(Span span, TextStyle style, float percentDefault)
		{
			foreach (AtomRenderer atom in span.Atoms)
			{
				atom.SetDefaultColorPercentage(percentDefault);
				atom.SetStyle(style);
			}
		}

		public void SetDecoration(Span span, Decoration decoration)
		{
			if (_decorationRenderers.TryGetValue(span, out var renderer))
			{
				renderer.RenderDecoration(decoration);
			}
			else
			{
				var drgo = new GameObject();
				drgo.transform.SetParent(transform);
				
				
				drgo.gameObject.name = "Decoration Renderer";
				renderer = drgo.AddComponent<DecorationRenderer>();
				renderer.transform.localPosition = Vector3.zero;
				renderer.transform.localScale = Vector3.one;
				renderer.transform.localRotation = Quaternion.identity;
				renderer.textRenderer = this;
				renderer.decorationSpan = span;
				_decorationRenderers.Add(span, renderer);
				renderer.RenderDecoration(decoration);
			}
		}

		public Span GetSpan(SpanSelector selector)
		{
			if (_spanCache.TryGetValue(selector, out var cachedSpan))
			{
				return cachedSpan;
			}
			string search = selector.Sensitive == CaseSensitive.CaseSensitive ? selector.searchText : selector.searchText.ToLower();
			Span result;
			switch (selector.searchType)
			{
				case TextSearchType.HTMLId:
				{
					if (!_htmlIdSpans.TryGetValue(search, out result))
					{
						Debug.LogWarning($"ID {search} not found.");
						return null;
					}
					break;
				}	
				case TextSearchType.HTMLClass:
				{
					if (!_htmlClassSpans.TryGetValue(search, out result))
					{
						Debug.LogWarning($"Class {selector.searchText} not found.");
						return null;
					}
					break;
				}
				case TextSearchType.FirstTextMatch:
					result = Utility.GetFirstSubSequence(_renderers, search.ToCharArray());
					break;
				case TextSearchType.LastTextMatch:
					result = Utility.GetLastSubSequence(_renderers, search.ToCharArray());
					break;
				case TextSearchType.AllTextMatches:
					var allSpans = Utility.GetAllSubSequence(_renderers, search.ToCharArray());
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
			
			_spanCache.Add(selector,result);
			return result;
		}

		public Vector3 GetLetterPosition(int c, int r)
		{
			//get the width by walking along and adding up all the display percentages.
			
			float wPosition = 0;
			for (int i = 0; i < c; i++)
			{
				var pos = new Vector2Int(i, r);
				if (_startingPosRenderers.TryGetValue(pos, out var ren))
				{
					wPosition += Font.aspect * ren.renderWidthPercentage;
				}
				else
				{
					wPosition += font.aspect;
				}
			}
			
			_maxRowWidth = Mathf.Max(wPosition, _maxRowWidth);
			return new Vector3(wPosition, -r, 0);
		}

		//todo: align left, right, etc.
		public void SetAndScaleSelfIntoWorldRect(Rect rect, float percent)
		{
			float totalWidth = _maxRowWidth;
			float totalHeight = _maxRow;//1 in localSpace for each row/col.
			float sizeModifier = 1;
			float shiftX = 0;
			float shiftY = 0;
			if (totalHeight > totalWidth)
			{
				sizeModifier = rect.height*percent / totalHeight;
				shiftY = rect.height * (1 - percent) / 2;
			}
			else
			{
				sizeModifier = rect.width*percent / totalWidth;
				shiftX = rect.width * (1 - percent) / 2f;
			}

			transform.localScale = Vector3.one * sizeModifier;
			
			//center it. Our zero is top left, so we need to move from the center we want up and to the left.
			var centerOffset = new Vector2(-totalWidth / 2f, totalHeight / 2f)+new Vector2(shiftX,shiftY);
			transform.position = rect.center + centerOffset;
		}
		
		public void SetLayoutDirty()
		{
			_isLayoutDirty = true;
		}
	}
}