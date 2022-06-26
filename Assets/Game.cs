using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.Map.GenerateMap();

    }

    // Update is called once per frame
    void Update()
    {
        GameManager.Instance.Map.GenerateMap();

    }
}
