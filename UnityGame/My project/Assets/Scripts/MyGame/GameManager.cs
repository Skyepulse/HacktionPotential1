using UnityEngine;
using System.Collections;
using System.Diagnostics;

//================================//
class GameManager: MonoBehaviour
{
    static GameManager instance;

    //================================//
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.Debug.Log("GameManager initialized");
    }

    //================================//
    public static void OnLeft()
    { 
        UnityEngine.Debug.Log("Left action triggered");
    }

    //================================//
    public static void OnRight()
    {
        UnityEngine.Debug.Log("Right action triggered");
    }

}