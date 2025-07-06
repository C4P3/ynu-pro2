using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MultiPlayButton : IButton
{
    [SerializeField] CanvasGroup homeUI;
    [SerializeField] CanvasGroup roomUI;

    public override void OnPointerClick()
    {
        base.OnPointerClick();
        SetUI(roomUI, 1, true, true);
        SetUI(homeUI, 0, false, false);
        StartSceneBGMManager.Instance.PlayBGM(StartSceneBGMManager.Instance.taisenBGM);
    }

    public void SetUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){
        canvasGroup.alpha = alfha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
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
