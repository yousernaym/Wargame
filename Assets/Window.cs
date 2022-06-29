using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public Action OnHide { get; set; }
    RectTransform rectTransform;
    Vector2 pos;
    static Vector2 staticPos;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        OnHide?.Invoke();
        gameObject.SetActive(false);
    }

    public void SetPos(Vector2 pos)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
    }
}
