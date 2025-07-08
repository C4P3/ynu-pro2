using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MatchButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); // 音などの共通処理

        // BGMを止めてからシーンを遷移
        StartSceneBGMManager.Instance.StopBGM();

        // 遷移前にUI非表示、遅延付きでシーンロード
        ChangeUI(beforeUI, 0, false, false);
        StartCoroutine(LoadSceneWithDelay(0.3f));
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameScene");
    }
}