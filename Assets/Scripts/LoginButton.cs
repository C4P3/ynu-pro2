using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginButton : IButton
{
    [SerializeField] TMP_InputField inputField;

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
