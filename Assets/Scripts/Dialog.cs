using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : Window
{
    bool resultOk;
    public bool ResultOk
    {
        get => resultOk;
        private set
        {
            resultOk = value;
            Hide();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ResultOk = false;
        else if (Input.GetKeyDown(KeyCode.Return))
            ResultOk = true;
    }
}
