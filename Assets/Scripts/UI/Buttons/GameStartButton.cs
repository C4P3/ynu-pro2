using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameStartButton : IButton
{
    public override void OnPointerClick()
    {
        base.OnPointerClick();   
        StartCoroutine(LoadSceneWithDelay( 0.3f));
        StartSceneBGMManager.Instance.StopBGM();
    }
    

    private IEnumerator LoadSceneWithDelay(float delay)
    {
    yield return new WaitForSeconds(delay);
    ChangeUI(beforeUI, 0, false, false);
    SceneManager.LoadScene("GameScene");
    }

    public override void OnPointerEnter()
    {
        base.OnPointerEnter();
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();
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