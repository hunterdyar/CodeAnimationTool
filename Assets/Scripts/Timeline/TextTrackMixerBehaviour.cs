using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace CodeAnimator
{
    // The runtime instance of a the TextTrack. It is responsible for blending and setting the final data
    // on the Text binding
    public class TextTrackMixerBehaviour : PlayableBehaviour
    {
        [HideInInspector] public SpanSelector Selector;
        
        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            SetDefaults(playerData as TextRenderer);
            var trackBinding = (TextRenderer)playerData;
            if (trackBinding == null)
            {
                return;
            }

            int inputCount = playable.GetInputCount();
            
            Color blendedColor = Color.clear;
            float totalWeight = 0f;
            float greatestWeight = 0f;
            
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<TextPlayableBehaviour> inputPlayable = (ScriptPlayable<TextPlayableBehaviour>)playable.GetInput(i);
                TextPlayableBehaviour input = inputPlayable.GetBehaviour();
                
                blendedColor += input.color * inputWeight;
                totalWeight += inputWeight;

                // use the text with the highest weight
                if (inputWeight > greatestWeight)
                {
                    // text = input.text;
                    greatestWeight = inputWeight;
                }
                
            }

            // blend to the default values
            var color = Color.Lerp(Color.clear, blendedColor, totalWeight);
           
           var span = trackBinding.GetSpan(Selector);
           if (span != null)
           {
               trackBinding.SetColor(span, color);
           }
        }

        // Invoked when the playable graph is destroyed, typically when PlayableDirector.Stop is called or the timeline
        // is complete.
        public override void OnPlayableDestroy(Playable playable)
        {
            RestoreDefaults();
        }

        void SetDefaults(TextRenderer text)
        {
            // if (text == m_TrackBinding)
            //     return;
            //
            // RestoreDefaults();
            //
            // m_TrackBinding = text;
            // if (m_TrackBinding != null)
            // {
            //     m_DefaultColor = m_TrackBinding.color;
            //     m_DefaultFontSize = m_TrackBinding.fontSize;
            //     m_DefaultText = m_TrackBinding.text;
            // }
        }

        void RestoreDefaults()
        {
            // if (m_TrackBinding == null)
            //     return;
            //
            // m_TrackBinding.color = m_DefaultColor;
            // m_TrackBinding.fontSize = m_DefaultFontSize;
            // m_TrackBinding.text = m_DefaultText;
        }
    }
}
