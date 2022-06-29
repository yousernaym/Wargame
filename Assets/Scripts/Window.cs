using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public Action OnHide { get; set; }
    RectTransform rectTransform;
    RectTransform RectTransform => rectTransform ?? (rectTransform = GetComponent<RectTransform>());
    public Vector2 Pos
    {
        get => RectTransform.anchoredPosition;
        set => RectTransform.anchoredPosition = value;
    }

    public Vector2 Size
    {
        get => RectTransform.sizeDelta;
        set => RectTransform.sizeDelta = value;
    }

    public Vector2 ParentSize
    {
        get
        {
            var parentRt = RectTransform.parent.GetComponent<RectTransform>();
            return parentRt.rect.size;
        }
    }
 

    public void Show(Vector2 pos)
    {
        Pos = pos;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


}
