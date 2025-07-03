using System;
using CodeAnimator;
using UnityEngine;
using Font = CodeAnimator.Font;

[RequireComponent(typeof(SpriteRenderer))]
public class AtomRenderer : MonoBehaviour
{
    public Atom Atom => atom;
    private Atom atom;
    private TextRenderer textRenderer;
    private SpriteRenderer _spriteRenderer;

    private TextStyle DefaultStyle;
    private float PercentDefault = 1;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Render()
    {
        if (textRenderer.Font.TryGetSprite(atom,  out Sprite sprite))
        {
            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
            else
            {
                //spaces, non-visible chars. null is fine for now.  w
            }
        }
    }
    
    public void Init(Atom atom, int c, int r, TextRenderer textRenderer)
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        this.textRenderer = textRenderer;
        this.atom = atom;
        _spriteRenderer =  GetComponent<SpriteRenderer>();
        Render();
        transform.localPosition = new Vector3(c*textRenderer.Font.aspect, -r, 0);
    }

    public void SetDefaultColorPercentage(float f)
    {
        PercentDefault = f;
    }
    public void SetStyle(TextStyle style, bool setDefault = false)
    {
        if (_spriteRenderer != null)
        {
            if (setDefault)
            {
                DefaultStyle = style;
            }
            else
            {
                //apply as slight tint if that's what we are doing, away from default.
                //var c = Color.Lerp(DefaultStyle.Color, style.Color, style.Color.a);
                var s = TextStyle.Lerp(style, DefaultStyle, PercentDefault);
                if (style.SetColor)
                {
                    _spriteRenderer.color = s.GetColorWithAlpha();
                }
                else
                {
                    _spriteRenderer.color = DefaultStyle.Color.WithAlpha(s.Alpha);
                }

                return;
            }
            
            _spriteRenderer.color = style.GetColorWithAlpha();
        }
        else
        {
            Debug.LogWarning("No sprite renderer");
        }
    }
}
