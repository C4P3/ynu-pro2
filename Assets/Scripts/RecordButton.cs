using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        title.gameObject.SetActive(false);
        matchButton.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
        configButton.gameObject.SetActive(false);

        RecordCanvas.SetActive(true);

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
