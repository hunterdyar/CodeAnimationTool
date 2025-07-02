
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CodeAnimator
{
    // A track that allows the user to change Text parameters from a Timeline.
    // It demonstrates the following
    //  * How to support blending of timeline clips.
    //  * How to change data over time on Components that is not supported by Animation.
    //  * Putting properties into preview mode.
    //  * Reacting to changes on the clip from the Timeline Editor.
    [TrackColor(0.1394896f, 0.4411765f, 0.3413077f)]
    [TrackClipType(typeof(TextPlayableAsset))]
    [TrackBindingType(typeof(TextRenderer))]
    public class TextTrack : TrackAsset
    {
        public SpanSelector TrackSelector;
        // Creates a runtime instance of the track, represented by a PlayableBehaviour.
        // The runtime instance performs mixing on the timeline clips.
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<TextTrackMixerBehaviour>.Create(graph, inputCount);
            mixer.GetBehaviour().Selector = TrackSelector;
            return mixer;
        }

        // // Invoked by the timeline editor to put properties into preview mode. This permits the timeline
        // // to temporarily change fields for the purpose of previewing in EditMode.
        // public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        // {
        //     TMP_Text trackBinding = director.GetGenericBinding(this) as TMP_Text;
        //     if (trackBinding == null)
        //         return;
        //
        //     // The field names are the name of the backing serializable field. These can be found from the class source,
        //     // or from the unity scene file that contains an object of that type.
        //     // driver.AddFromName<TextRenderer>(trackBinding.gameObject, "m_text");
        //     // driver.AddFromName<TextRenderer>(trackBinding.gameObject, "m_fontSize");
        //     // driver.AddFromName<TextRenderer>(trackBinding.gameObject, "m_fontColor");
        //
        //     base.GatherProperties(director, driver);
        // }
    }
}
