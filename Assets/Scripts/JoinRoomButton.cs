using UnityEngine;
using UnityEngine.EventSystems;

public class JoinRoomButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        PlayFabMatchmakingManager.Instance.JoinRoom();

    }
}