using UnityEngine;
using UnityEngine.Playables;

namespace CodeAnimator
{
    // The runtime instance of the TextTrack. It is responsible for blending and setting the final data
    // on the Text binding
    public class TextTrackMixerBehaviour : PlayableBehaviour
    {
        [HideInInspector] public SpanSelector Selector;
        
        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //SetDefaults(playerData as TextRenderer);
            var trackBinding = (TextRenderer)playerData;
            if (trackBinding == null)
            {
                return;
            }

            int inputCount = playable.GetInputCount();
            
            Color blendedColor = Color.clear;
            float blendedAlpha = 0f;
            float blendedWidth = 0f;
            float blendedHeight = 0;
            
            float totalWeight = 0f;
            float totalColorWeight = 0f;
            bool setColor = false;
            bool shrink = false;
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<TextPlayableBehaviour> inputPlayable =
                    (ScriptPlayable<TextPlayableBehaviour>)playable.GetInput(i);
                TextPlayableBehaviour input = inputPlayable.GetBehaviour();

                if (input.Style.SetColor)
                {
                    blendedColor += input.Style.Color * inputWeight;
                    totalColorWeight += inputWeight;
                    if (inputWeight > 0)
                    {
                        setColor = true; //if any inputs set color, we set color.
                    }
                }
                blendedAlpha += input.Style.Alpha * inputWeight;
                blendedWidth += input.Style.RenderWidth * inputWeight;
                blendedHeight += input.Style.RenderHeight * inputWeight;
                totalWeight += inputWeight;
                if (input.Style.shrinkWithWidth && input.Style.RenderWidth != 1 && inputWeight > 0)
                {
                    shrink = true;
                }
            }

            float percentageDefault = 1 - totalWeight;
            float a = totalWeight > 0 ? blendedAlpha / totalWeight : 0f;
            float w = totalWeight > 0 ? blendedWidth / totalWeight : 1f;
            float h =  totalWeight > 0 ? blendedHeight / totalWeight : 1f;
            // blend to the default values
            var style = new TextStyle(blendedColor, a, w, h, setColor, shrink);
           var span = trackBinding.GetSpan(Selector);
           if (span != null)
           {
               TextRenderer.SetStyle(span, style, percentageDefault);
           }
        }

        // Invoked when the playable graph is destroyed, typically when PlayableDirector.Stop is called or the timeline
        // is complete.
        // public override void OnPlayableDestroy(Playable playable)
        // {
        //     RestoreDefaults();
        // }
        //
        // void SetDefaults(TextRenderer text)
        // {
        //     // if (text == )
        //         // return;
        //     
        //     RestoreDefaults();
        //     
        //     m_TrackBinding = text;
        //     if (m_TrackBinding != null)
        //     {
        //         m_DefaultColor = m_TrackBinding.color;
        //         m_DefaultFontSize = m_TrackBinding.fontSize;
        //         m_DefaultText = m_TrackBinding.text;
        //     }
        // }
        //
        // void RestoreDefaults()
        // {
        //     // if (m_TrackBinding == null)
        //     //     return;
        //     //
        //     // m_TrackBinding.color = m_DefaultColor;
        //     // m_TrackBinding.fontSize = m_DefaultFontSize;
        //     // m_TrackBinding.text = m_DefaultText;
        // }
    }
}
