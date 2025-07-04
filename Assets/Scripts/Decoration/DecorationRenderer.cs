﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeAnimator
{
	public class DecorationRenderer : MonoBehaviour
	{
		//A decoration Renderer Renders a single Decoration. 
		public Decoration? currentDecoration;
		public Span decorationSpan;
		public TextRenderer textRenderer;

		private readonly List<SpriteRenderer> _underlineRenderers =  new List<SpriteRenderer>();
		private readonly List<SpriteRenderer> _highlightRenderers = new List<SpriteRenderer>();

		private OutlineRenderer _outlineRenderer;

		void LazyInitOutline()
		{
			if (_outlineRenderer == null)
			{
				var child = new GameObject("outline");
				child.transform.SetParent(transform, false);
				_outlineRenderer = child.AddComponent<OutlineRenderer>();
			}
		}

		private void ClearUnderline()
		{
			for (int i = _underlineRenderers.Count - 1; i >= 0; i--)
			{
				Destroy(_underlineRenderers[i].gameObject);
			}
			_underlineRenderers.Clear();
		}

		private void ClearHighlight()
		{
			for (int i = _highlightRenderers.Count - 1; i >= 0; i--)
			{
				Destroy(_highlightRenderers[i].gameObject);
			}
			_highlightRenderers.Clear();
		}
		
		/// <summary>
		/// Renders a decoration while comparing to current decoration to do as little work as possible.
		/// </summary>
		public void RenderDecoration(Decoration decoration)
		{
			if (decoration == currentDecoration)
			{
				return;
			}
			
			if (decoration.Underline)
			{
				if (currentDecoration == null || !currentDecoration.Value.Underline)
				{
					//we now have an underline.
					RenderUnderline(decoration.UnderlineColor);
				}
				else
				{
					if (currentDecoration.Value.UnderlineColor != decoration.UnderlineColor)
					{
						SetUnderlineColor(currentDecoration.Value.UnderlineColor);
					}
				}
			}
			else
			{
				ClearUnderline();
			}

			if (decoration.Highlight)
			{
				if (currentDecoration == null || !currentDecoration.Value.Highlight)
				{
					//we now have an underline.
					RenderHighlight(decoration.HighlightColor);
				}
				else
				{
					if (currentDecoration.Value.HighlightColor != decoration.HighlightColor)
					{
						SetHighlightColor(currentDecoration.Value.HighlightColor);
					}
				}
			}
			else
			{
				ClearHighlight();
			}

			if (decoration.Outline)
			{
				LazyInitOutline();
				//this is cheap we can just set some variables.

				if (currentDecoration == null || !currentDecoration.Value.Outline)
				{
					//we now have an underline.
					_outlineRenderer.SetFromSpan(decorationSpan);
					_outlineRenderer.SetActive(true);
					_outlineRenderer.SetLineStyle(decoration.OutlineColor, decoration.OutlineThickness);
				}
				else
				{
					//the box already exists for this span.
					_outlineRenderer.SetActive(true);
					if (currentDecoration.Value.OutlineColor != decoration.OutlineColor || !Mathf.Approximately(currentDecoration.Value.OutlineThickness, decoration.OutlineThickness))
					{
						_outlineRenderer.SetLineStyle(decoration.OutlineColor, decoration.OutlineThickness);
					}
				}
			}
			else
			{
				if (_outlineRenderer != null)
				{
					_outlineRenderer.SetActive(false);
				}
			}
			
			currentDecoration = decoration;
		}

		private void SetUnderlineColor(Color color)
		{
			foreach (var renderer in _underlineRenderers)
			{
				renderer.color = color;
			}
		}
		private void SetHighlightColor(Color color)
		{
			foreach (var renderer in _highlightRenderers)
			{
				renderer.color = color;
			}
		}

		//set decoration properties, pass along to atomRenderer setProperties.

		private void RenderUnderline(Color color)
		{
			var underlineSprite = textRenderer.Font.GetUnderlineSprite();
			foreach (AtomRenderer atomRenderer in decorationSpan.Atoms)
			{
				var sr = CreateChildSpriteRenderer(atomRenderer);
				_underlineRenderers.Add(sr);
				sr.sortingOrder = 10;
				sr.sprite = underlineSprite;
				sr.color = color;
			}
			
		}

		private void RenderHighlight(Color color)
		{
			var underlineSprite = textRenderer.Font.GetHighlightSprite();
			foreach (AtomRenderer atomRenderer in decorationSpan.Atoms)
			{
				var sr = CreateChildSpriteRenderer(atomRenderer);
				_highlightRenderers.Add(sr);
				sr.sortingOrder = -10;
				sr.sprite = underlineSprite;
				sr.color = color;
			}
		}

		private SpriteRenderer CreateChildSpriteRenderer(AtomRenderer atom)
		{
			var srgo = new GameObject();
			var sr = srgo.AddComponent<SpriteRenderer>();
			srgo.name = "Decoration";
			srgo.transform.SetParent(atom.transform);
			srgo.transform.localPosition = Vector3.zero;
			srgo.transform.localScale = Vector3.one;
			return sr;
		}

		private void OnDestroy()
		{
			ClearUnderline();
			ClearHighlight();
		}
	}
}