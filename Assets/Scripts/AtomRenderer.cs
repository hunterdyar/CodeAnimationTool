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

    public float renderWidthPercentage = 1;
    private TextStyle DefaultStyle;
    private float PercentDefault = 1;
    private int row;
    private int col;
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
        this.row = r;
        this.col = c;
        _spriteRenderer =  GetComponent<SpriteRenderer>();
        Render();
        transform.localPosition = textRenderer.GetLetterPosition(c, r);
    }

    public void UpdatePosition()
    {
        transform.localPosition = textRenderer.GetLetterPosition(this.col, this.row);
    }

    public void SetDefaultColorPercentage(float f)
    {
        PercentDefault = f;
    }
    public void SetStyle(TextStyle style, bool setDefault = false)
    {
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("No sprite renderer");
        }
        
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


            if (!Mathf.Approximately(s.RenderWidth, renderWidthPercentage))
            {
                textRenderer.SetLayoutDirty();
            }
            
            renderWidthPercentage = s.RenderWidth;
            if (style.shrinkWithWidth)
            {
                transform.localScale = new Vector3(s.RenderWidth, transform.localScale.y, transform.localScale.z);
            }

            return;
        }

        if (!Mathf.Approximately(style.RenderWidth, renderWidthPercentage))
        {
            textRenderer.SetLayoutDirty();
        }
        renderWidthPercentage = style.RenderWidth;
        if (style.shrinkWithWidth)
        {
            transform.localScale = new Vector3(style.RenderWidth, transform.localScale.y, transform.localScale.z);
        }
        _spriteRenderer.color = style.GetColorWithAlpha();
        
    }
}
