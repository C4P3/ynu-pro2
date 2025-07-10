using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class JoinRoomButton : IButton
{
    [SerializeField] TMP_InputField inputField;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // ルームIDが空でなければマッチング処理を実行
        if (!string.IsNullOrEmpty(inputField.text))
        {
            PlayFabMatchmakingManager.Instance.JoinRoom();
        }
        else
        {
            Debug.LogWarning("ルームIDが入力されていません。");
        }
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
