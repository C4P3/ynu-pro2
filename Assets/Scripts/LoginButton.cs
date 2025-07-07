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
