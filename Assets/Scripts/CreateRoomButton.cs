using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreateRoomButton : IButton
{
    [SerializeField] TMP_InputField inputField;
    
    public override void OnPointerClick()
    {
        base.OnPointerClick();
        PlayFabMatchmakingManager.Instance.CreateRoom();
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