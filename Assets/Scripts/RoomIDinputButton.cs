using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RoomIDInputButton : IButton
{
    public override void OnPointerClick()
    {
        base.OnPointerClick();
        PlayFabMatchmakingManager.Instance.JoinRoom();
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