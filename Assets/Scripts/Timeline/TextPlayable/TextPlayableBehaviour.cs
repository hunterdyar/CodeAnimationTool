
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace CodeAnimator
{
    // Runtime representation of a TextClip.
    // The Serializable attribute is required to be animated by timeline, and used as a template.
    [Serializable]
    public class TextPlayableBehaviour : PlayableBehaviour
    {
        [Tooltip("The color of the text")]
        public TextStyle Style;

        public TextPlayableBehaviour()
        {
            Style = TextStyle.GetDefaultStyle();
        }
        public string DisplayName()
        {
            var namePieces = "";
            if (Style.SetColor)
            {
                namePieces += ("Set Color");
            }

            if (Style.Alpha < 1)
            {
                namePieces += $" Alpha({Style.Alpha:N2})";
            }

            if (Style.RenderWidth != 1)
            {
                namePieces += ($" Width({Style.RenderWidth:N2})");
            }

            if (namePieces.Length == 0)
            {
                return "Normal";
            }

            return namePieces;
        }
    }
}
