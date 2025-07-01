using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class BackButton : MonoBehaviour, IButton
{
    public GameObject HomeCanvas;
    public GameObject RecordCanvas;
    public GameObject ConfigCanvas;
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OnPointerClick()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
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
