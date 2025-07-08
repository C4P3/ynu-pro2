using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ConfigButton : IButton
{
    public override void OnPointerClick()
    {
        base.OnPointerClick();
        ChangeUI(beforeUI, 0, false, false);
        ChangeUI(afterUI, 1, true, true);
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
