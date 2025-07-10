using UnityEngine;
using UnityEngine.EventSystems;

public class LoginButton : IButton
{
    public GameObject joinButton; // ← これだけ定義すればOK

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        Debug.Log("ログインボタンが押されました");

        ChangeUI(beforeUI, 0, false, false); // 基底クラスの beforeUI をそのまま使用
        ChangeUI(afterUI, 1, true, true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(joinButton);
    }
}