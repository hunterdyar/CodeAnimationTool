using UnityEngine;
using UnityEngine.Playables;

namespace CodeAnimator
{
    // The runtime instance of the TextTrack. It is responsible for blending and setting the final data
    // on the Text binding
    public class DecorationTrackMixerBehaviour : PlayableBehaviour
    {
        [HideInInspector] public SpanSelector Selector;
        
        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            //SetDefaults(playerData as TextRenderer);
            var trackBinding = (TextRenderer)playerData;
            if (trackBinding == null)
            {
                return;
            }

            int inputCount = playable.GetInputCount();
            
            Color blendedHighlight = Color.clear;
            float highlightAlpha = 0;
            Color blendedUnderline = Color.clear;
            float underlineAlpha = 0;
            Color blendedOutline = Color.clear;
            float outlineAlpha = 0;
            float outlineThickness = 0;
            bool highlight = false;
            bool underline = false;
            bool outline = false;
            
            float totalWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<DecorationPlayableBehaviour> inputPlayable =
                    (ScriptPlayable<DecorationPlayableBehaviour>)playable.GetInput(i);
                DecorationPlayableBehaviour input = inputPlayable.GetBehaviour();

                if (inputWeight > 0 && input.Decoration.Underline)
                {
                    underline = true;
                    underlineAlpha += input.Decoration.UnderlineColor.a*inputWeight;
                    blendedUnderline += input.Decoration.UnderlineColor * inputWeight;
                }

                if (inputWeight > 0 && input.Decoration.Highlight)
                {
                    highlight = true;
                    highlightAlpha += input.Decoration.HighlightColor.a * inputWeight;
                    blendedHighlight += input.Decoration.HighlightColor * inputWeight;
                }

                if (inputWeight > 0 && input.Decoration.Outline)
                {
                    outline = true;
                    outlineAlpha += input.Decoration.OutlineColor.a * inputWeight;
                    blendedOutline += input.Decoration.OutlineColor * inputWeight;
                    outlineThickness += input.Decoration.OutlineThickness*inputWeight;
                }
                
                totalWeight += inputWeight;
            }

            if (totalWeight > 0)
            {
                //when mixing half of one color, we want it's full color (/= totalweight to normalize), 
                //and the "remaining" percentage is transparency.
                
                blendedHighlight /= totalWeight;
                blendedHighlight = blendedHighlight.WithAlpha(highlightAlpha);
                
                blendedUnderline /= totalWeight;
                blendedUnderline = blendedUnderline.WithAlpha(underlineAlpha);

                blendedOutline /= totalWeight;
                blendedOutline = blendedOutline.WithAlpha(outlineAlpha);
            }
            // blend to the default values
            var decoration = new Decoration()
            {
                Highlight = highlight,
                HighlightColor = blendedHighlight,
                Underline = underline,
                UnderlineColor = blendedUnderline,
                Outline = outline,
                OutlineColor = blendedOutline,
                OutlineThickness = outlineThickness,
            };
           var span = trackBinding.GetSpan(Selector);
           if (span != null)
           {
               trackBinding.SetDecoration(span, decoration);
           }
        }
    }
}
