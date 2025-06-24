using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour, IButton
{
    public GameObject HomeCanvas;
    public GameObject RecordCanvas;
    public GameObject ConfigCanvas;
    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("Back Button Clicked");
        // Here you can add functionality to start or stop recording
        ConfigCanvas.GetComponent<UnityEngine.Canvas>().enabled = false;
        RecordCanvas.GetComponent<UnityEngine.Canvas>().enabled = false;
        HomeCanvas.GetComponent<UnityEngine.Canvas>().enabled = true;

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
