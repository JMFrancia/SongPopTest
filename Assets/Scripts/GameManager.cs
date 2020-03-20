using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public DataManager Data { get; private set; }

    private void Start()
    {
        Data = GetComponent<DataManager>();
        Data.Initialize();
    }
}
