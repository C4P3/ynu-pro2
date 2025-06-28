using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginButton : IButton
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] CanvasGroup homeUI;
    [SerializeField] CanvasGroup loginUI;

    public override void OnPointerClick(){

        var authManager = FindFirstObjectByType<PlayFabAuthManager>();

        if(authManager == null)
        {
            Debug.LogError("PlayFabAuthManagerが見つかりません。");
            return;
        }

        if(inputField == null)
        {
            Debug.LogError("InputFieldが設定されていません。");
            return;
        }

        base.OnPointerClick();
        authManager.SetDisplayName(inputField.text);

        ChangeUI(homeUI, 1, true, true); //homeUIを表示
        ChangeUI(loginUI, 0, false, false); //homeUIを非表示

    }
    public override void OnPointerEnter(){

    }
    public override void OnPointerExit(){

    }
    // public override void OnPointerDown()
    // {
    //     buttonImage.color = buttonImage.color * 0.7f;
    // }

    // public override void OnPointerUp()
    // {
    //     buttonImage.color = buttonImage.color / 0.7f;
    // }
}
