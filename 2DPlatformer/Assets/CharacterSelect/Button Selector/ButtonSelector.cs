using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ButtonSelector : Selector
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshPro text;
    public void SetButton(bool ready = false) {
        if (ready)
        {
            spriteRenderer.color = Color.green;
            text.text = "Ready";
        }
        else {
            spriteRenderer.color = Color.red;
            text.text = "Not Ready";
        }
    }
    private void Start()
    {
        SetButton(false);
    }
}
