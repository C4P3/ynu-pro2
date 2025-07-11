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