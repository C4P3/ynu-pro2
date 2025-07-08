using UnityEngine;
using UnityEngine.EventSystems;

public class LoginButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); // 音などの共通処理

        // 入力チェックやUI遷移などの処理をここに記述
        Debug.Log("ログインボタンが押されました");

        // 例: UI 切り替え（ログイン → マッチングへ）
        ChangeUI(beforeUI, 0, false, false);
        ChangeUI(afterUI, 1, true, true);
    }
}
