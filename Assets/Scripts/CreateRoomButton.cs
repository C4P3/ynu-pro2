using UnityEngine;
using UnityEngine.EventSystems;

public class CreateRoomButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick();
        PlayFabMatchmakingManager.Instance.CreateRoom();
    }
}