using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MatchButton : IButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick();   
        StartCoroutine(LoadSceneWithDelay( 0.3f));
        StartSceneBGMManager.Instance.StopBGM();
    }
    

    private IEnumerator LoadSceneWithDelay(float delay)
    {
    yield return new WaitForSeconds(delay);
    ChangeUI(beforeUI, 0, false, false);
    SceneManager.LoadScene("GameScene");
    }
}