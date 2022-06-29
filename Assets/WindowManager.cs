using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance {  get; private set; }
    void Awake()
    {
        Instance = this;
    }

   public void Show(Window window, Vector2 pos, Action onClose)
    {
        window.OnHide = onClose;
        window.SetPos(pos);
        window.Show();
    }
}
