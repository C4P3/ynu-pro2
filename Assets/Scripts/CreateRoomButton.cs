using UnityEngine;
using UnityEngine.EventSystems;

public class CreateRoomButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        PlayFabLobbyManager.Instance.CreateRoom();
    }
}