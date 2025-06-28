using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MatchButton : IButton
{
    public override void OnPointerClick()
    {
        base.OnPointerClick();   
        StartCoroutine(LoadSceneWithDelay( 0.3f));
    }

    private IEnumerator LoadSceneWithDelay(float delay)
    {
    yield return new WaitForSeconds(delay);
    SceneManager.LoadScene("GameScene");
    }

    public override void OnPointerEnter()
    {
        
    }

    public override void OnPointerExit()
    {

    }

    public override void OnPointerDown()
    {
        base.OnPointerDown();
    }

    public override void OnPointerUp()
    {
        base.OnPointerUp();
    }
}
