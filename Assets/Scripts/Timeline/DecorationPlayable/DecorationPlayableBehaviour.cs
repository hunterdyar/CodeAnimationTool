
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
    public class DecorationPlayableBehaviour : PlayableBehaviour
    {
        public Decoration Decoration;
        
        // public string DisplayName()
        // {
        //     var namePieces = "";
        //    
        //     return namePieces;
        // }
    }
}
