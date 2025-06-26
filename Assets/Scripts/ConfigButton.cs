using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigButton : MonoBehaviour, IButton
{
    public GameObject HomeCanvas;
    public GameObject ConfigCanvas;
    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("Config Button Clicked");
        // Here you can add functionality to open the configuration menu
        HomeCanvas.GetComponent<UnityEngine.Canvas>().enabled = false;
        ConfigCanvas.GetComponent<UnityEngine.Canvas>().enabled = true;

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
