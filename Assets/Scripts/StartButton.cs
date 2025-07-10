using UnityEngine;
using UnityEngine.EventSystems;

public class StartButton : IButton
{
    public GameObject joinButton; // HomeUIで最初に選択させたいボタン（例: Join）

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); // 共通処理（例：効果音）

        Debug.Log("スタートボタンが押されました");

        // UI遷移（TitleUI → HomeUI）
        ChangeUI(beforeUI, 0, false, false); // TitleUI を非表示
        ChangeUI(afterUI, 1, true, true);    // HomeUI を表示

        // 選択対象を明示的に設定（Enterキー対応）
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(joinButton);
    }
}