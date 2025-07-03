using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeAnimator
{
	public class DecorationRenderer : MonoBehaviour
	{
		//A decoration Renderer Renders a single Decoration. 
		public DecorationType decorationType;
		public Span decorationSpan;
		public Color decorationColor;
		public TextRenderer textRenderer;

		private List<AtomRenderer> _decAtomRenderers =  new List<AtomRenderer>();

		public void Clear()
		{
			for (var i = _decAtomRenderers.Count - 1; i >= 0; i--)
			{
				AtomRenderer atomRenderer = _decAtomRenderers[i];
				Destroy(atomRenderer.gameObject);
			}
		}
		
		public void RenderDecoration()
		{
			switch (decorationType)
			{
				case DecorationType.Underline:
					RenderUnderline();
					break;
				case DecorationType.SurroundBox:
					throw new NotImplementedException();
					//break;
			}
		}
		
		//set decoration properties, pass along to atomRenderer setProperties.

		public void RenderUnderline()
		{
			Clear();
			foreach (AtomRenderer atomRenderer in decorationSpan.Atoms)
			{
				//call the factory method to create an atomRenderer.
				//set it's z draw to one above the atomRenderer we are attached to.
				//set its sprite to underline sprite.
				//enable it.
			}
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}