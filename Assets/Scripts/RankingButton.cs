using UnityEngine;
using UnityEngine.EventSystems;

public class RankingButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        ChangeUI(beforeUI, 0, false, false);
        ChangeUI(afterUI, 1, true, true);
    }
}