using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        ChangeUI(beforeUI, 1, true, true);  // 元のUIを表示
        ChangeUI(afterUI, 0, false, false); // 現在のUIを非表示
    }
}