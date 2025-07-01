using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RecordButton : MonoBehaviour, IButton
{
    public GameObject HomeCanvas;
    public GameObject RecordCanvas;
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
