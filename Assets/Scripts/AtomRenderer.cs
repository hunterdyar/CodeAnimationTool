using System;
using CodeAnimator;
using UnityEngine;
using Font = CodeAnimator.Font;

[RequireComponent(typeof(SpriteRenderer))]
public class AtomRenderer : MonoBehaviour
{
    private Atom atom;
    private TextRenderer textRenderer;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Render()
    {
        if (textRenderer.Font.TryGetSprite(atom,  out Sprite sprite))
        {
            _spriteRenderer.sprite = sprite;
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

    public void SetColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning("No sprite renderer");
        }
    }
}
