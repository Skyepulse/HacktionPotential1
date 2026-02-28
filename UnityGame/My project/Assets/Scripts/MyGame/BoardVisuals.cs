using UnityEngine;
using System;
using System.Diagnostics;

//================================//
class BoardVisuals: MonoBehaviour
{
    [SerializeField]
    private GameObject ROOT;

    [SerializeField]
    private GameObject OnLever;

    [SerializeField]
    private GameObject OffLever;

    [SerializeField]
    private GameObject OnButton;

    [SerializeField]
    private GameObject OffButton;

    [SerializeField]
    public Transform offset;

    [SerializeField]
    public float baseScale = 0.15f;

    //================================//
    void Awake()
    {
        SetLever(false);
        SetButton(false);
    }

    //================================//
    public void SetLever(bool isOn)
    {
        OnLever.SetActive(isOn);
        OffLever.SetActive(!isOn);
    }

    //================================//
    public void SetButton(bool isOn)
    {
        OnButton.SetActive(isOn);
        OffButton.SetActive(!isOn);
    }
}