using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // ← これが必要！

public class MatchButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        StartCoroutine(DoMatching());
    }

    private IEnumerator DoMatching()
    {
        // 処理の例
        yield return new WaitForSeconds(1f);
        Debug.Log("マッチング開始");
    }
}