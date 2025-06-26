using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordButton : MonoBehaviour, IButton
{
    public GameObject HomeCanvas;
    public GameObject RecordCanvas;

    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("Record Button Clicked");
        // Here you can add functionality to start or stop recording
        HomeCanvas.GetComponent<UnityEngine.Canvas>().enabled = false;
        RecordCanvas.GetComponent<UnityEngine.Canvas>().enabled = true;

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
