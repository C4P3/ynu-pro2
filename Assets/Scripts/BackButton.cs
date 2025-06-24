using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour, IButton
{
    public GameObject HomeUI;
    public GameObject RecordUI;
    public GameObject ConfigUI;
    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("Back Button Clicked");
        // Here you can add functionality to start or stop recording
        // ConfigUI.GetComponent<UnityEngine.Transform>().enabled = false;
        // RecordUI.GetComponent<UnityEngine.Transform>().enabled = false;
        // HomeUI.GetComponent<UnityEngine.Transform>().enabled = true;

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
