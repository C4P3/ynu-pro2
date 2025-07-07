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
        ChangeUI(roomUI, 1, true, true);
        ChangeUI(homeUI, 0, false, false);
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
