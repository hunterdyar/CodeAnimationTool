using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CodeAnimator
{
    // Represents the serialized data for a clip on the DecorationTrack
    [Serializable]
    public class DecorationPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
       // [NoFoldOut]
        [NotKeyable] // NotKeyable used to prevent Timeline from making fields available for animation.
        public DecorationPlayableBehaviour template = new DecorationPlayableBehaviour();
        
        // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        // Creates the playable that represents the instance of this clip.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // Using a template will clone the serialized values
            return ScriptPlayable<DecorationPlayableBehaviour>.Create(graph, template);
        }
    }
}
