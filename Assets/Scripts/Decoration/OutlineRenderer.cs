using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace CodeAnimator
{
	public class OutlineRenderer : MonoBehaviour
	{
		private LineRenderer _lineRenderer;
		private Span currentSpan;
		private readonly HashSet<Vector3> _points  = new HashSet<Vector3>();

		public void SetActive(bool active)
		{
			if (_lineRenderer != null)
			{
				_lineRenderer.enabled = active;
			}
		}
		public void SetLineStyle(Color color, float thickness)
		{
			if (_lineRenderer != null)
			{
				_lineRenderer.startColor = color;
				_lineRenderer.endColor = color;
				_lineRenderer.loop = true;
				_lineRenderer.numCornerVertices = 5;
				_lineRenderer.widthCurve = new AnimationCurve(new Keyframe(0, thickness), new Keyframe(1, thickness));
			}
		}
		public void SetFromSpan(Span span)
		{
			if (_lineRenderer == null)
			{
				_lineRenderer = gameObject.AddComponent<LineRenderer>();
				_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
				_lineRenderer.useWorldSpace = false;
			}
			if (span == currentSpan)
			{
				return;
			}
			_points.Clear();
			foreach (AtomRenderer atom in span.Atoms)
			{
				foreach (var p in atom.GetCorners())
				{
					_points.Add(p);
				}
			}
			
			//todo: simplify line. use multiple lines for islands (calculate islands?)

			UpdateLineRendererFromPoints();
			currentSpan = span;
		}

		void UpdateLineRendererFromPoints()
		{
			//todo: fix later.
			if (_points.Count > 0)
			{
				var hull = ConvexHull.ComputeConvexHull(_points.ToList(), false);
				_lineRenderer.positionCount = hull.Count;
				_lineRenderer.SetPositions(hull.ToArray());
			}
		}
	}

	public struct LineSegment
	{
		public float x1;
		public float y1;
		public float x2;
		public float y2;
		public float Slope => (y2 - y1) / (x2 - x1);

		public LineSegment(float x1, float y1, float x2, float y2)
		{
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;
		}
	}
}