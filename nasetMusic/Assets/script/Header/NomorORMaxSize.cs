using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class NomorORMaxSize : MonoBehaviour
{
    public Sprite min;
    public Sprite max;
    public Image image;
    void Start()
    {
        image = GetComponent<Image>();   
    }
    private void Update()
    {
        if (WindowSetting.GetPlacement())
        {
            image.sprite = max;
        }
        else
        {
            image.sprite = min;
        }
    }

}
