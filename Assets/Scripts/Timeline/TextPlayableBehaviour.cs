
using System;
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

        public string DisplayName()
        {
            return "Set Color";
        }
    }
}
