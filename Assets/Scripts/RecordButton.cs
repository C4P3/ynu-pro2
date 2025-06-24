using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecordButton : MonoBehaviour, IButton
{
    public GameObject title;
    public MatchButton matchButton;
    public ConfigButton configButton;
    public GameObject RecordCanvas;

    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("Record Button Clicked");
        // Here you can add functionality to start or stop recording


        //RecordCanvas.GetComponent<UnityEngine.Canvas>().enabled = true;

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
