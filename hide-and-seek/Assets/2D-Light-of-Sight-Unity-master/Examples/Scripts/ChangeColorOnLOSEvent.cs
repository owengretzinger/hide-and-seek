using UnityEngine;
using System.Collections;
using LOS.Event;
using TMPro;

public class ChangeColorOnLOSEvent : MonoBehaviour
{
    public Color litColor;
    public Color notLitColor;

    GameObject self;

    private SpriteRenderer _renderer;
    private TMP_Text nameText;
    private LOSEventTrigger trigger;

    private void Start()
    {
        self = transform.parent.gameObject;

        _renderer = (SpriteRenderer)self.GetComponentInChildren<Renderer>();
        nameText = self.GetComponentInChildren<TMP_Text>();

        trigger = GetComponent<LOSEventTrigger>();
        trigger.OnNotTriggered += OnNotLit;
        trigger.OnTriggered += OnLit;
        
        OnNotLit();
    }

    private void OnNotLit()
    {
        _renderer.color = notLitColor;
        nameText.color = notLitColor;
    }

    public void OnLit()
    {
        _renderer.color = litColor;
        nameText.color = litColor;
    }

    private void OnDisable()
    {
        trigger.OnNotTriggered -= OnNotLit;
        trigger.OnTriggered -= OnLit;
    }
}
