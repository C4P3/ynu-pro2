using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LoginButton : IButton
{
    [SerializeField] TMP_InputField inputField;
    public GameObject joinButton;

    public override void OnPointerClick(PointerEventData eventData)
    {
        var authManager = FindFirstObjectByType<PlayFabAuthManager>();

        if (authManager == null)
        {
            Debug.LogError("PlayFabAuthManagerが見つかりません。");
            return;
        }

        if (inputField == null)
        {
            Debug.LogError("InputFieldが設定されていません。");
            return;
        }

        base.OnPointerClick(eventData);
        Debug.Log("ログインボタンが押されました");

        ChangeUI(beforeUI, 0, false, false); // 基底クラスの beforeUI をそのまま使用
        ChangeUI(afterUI, 1, true, true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(joinButton);
        
        authManager.SetDisplayName(inputField.text);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }
}
