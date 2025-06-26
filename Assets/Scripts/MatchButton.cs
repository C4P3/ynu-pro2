using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MatchButton : IButton
{
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }
    public override void OnPointerClick()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        StartCoroutine(LoadSceneWithDelay( 0.3f));
    }

    private IEnumerator LoadSceneWithDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    SceneManager.LoadScene("GameScene");
}

    public override void OnPointerEnter(){

    }
    public override void OnPointerExit(){

    }
}
