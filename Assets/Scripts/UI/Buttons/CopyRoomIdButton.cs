using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CopyRoomIdButton : IButton
{
    [SerializeField] private GameObject popup;
    [SerializeField] private float popupDuration = 1.5f;

    void Start()
    {
        popup.SetActive(false);
    }

    public override void OnPointerClick()
    {
        base.OnPointerClick();
        // クリップボードにコピー
        if (PlayFabMatchmakingManager.Instance != null)
        {
            GUIUtility.systemCopyBuffer = PlayFabMatchmakingManager.Instance.roomId;
        }

        // ポップアップを表示
        StartCoroutine(ShowPopup());
    }

    IEnumerator ShowPopup()
    {
        popup.SetActive(true);
        yield return new WaitForSeconds(popupDuration);
        popup.SetActive(false);
    }
}