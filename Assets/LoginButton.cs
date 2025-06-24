using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginButton : MonoBehaviour, IButton
{
    [SerializeField] TMP_InputField inputField;

    public void OnPointerClick(){
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

        authManager.SetDisplayName(inputField.text);
    }
    public void OnPointerEnter(){

    }
    public void OnPointerExit(){

    }
}
