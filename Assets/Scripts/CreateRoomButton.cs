using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CreateRoomButton : IButton
{
    [SerializeField] TMP_InputField inputField;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // 入力値がある場合のみ部屋を作成する処理（任意）
        if (!string.IsNullOrEmpty(inputField.text))
        {
            PlayFabMatchmakingManager.Instance.CreateRoom();
        }
        else
        {
            Debug.LogWarning("ルーム名が入力されていません。");
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
