using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartButton : IButton
{
    public override void OnPointerClick()
    {
        // シーンを切り替える前にBGMを停止
        if (StartSceneBGMManager.Instance != null)
        {
            StartSceneBGMManager.Instance.StopBGM();
            // タイトル画面のBGMを再生するために、StartSceneBGMManagerのPlayBGMメソッドを呼び出す
            StartSceneBGMManager.Instance.PlayBGM(StartSceneBGMManager.Instance.menuBGM);
        }
        else
        {
            Debug.LogWarning("StartSceneBGMManager instance is not available.");
        }
        // シーンを切り替える処理
        base.OnPointerClick();
        ChangeUI(beforeUI, 0, false, false);
        ChangeUI(afterUI, 1, true, true);

    }

    public override void OnPointerEnter()
    {

    }
    public override void OnPointerExit()
    {

    }
    public override void OnPointerDown()
    {

    }
    public override void OnPointerUp()
    {

    }
}