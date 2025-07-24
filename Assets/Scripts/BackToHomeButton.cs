using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToHomeButton : IButton
{
    public override void OnPointerClick()
    {
        base.OnPointerClick();

        // Time.timeScaleを元に戻す
        Time.timeScale = 1f;
        
        // StartSceneをロード
        SceneManager.LoadScene("StartScene");
    }

    public override void OnPointerEnter()
    {
        base.OnPointerEnter();
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();
    }

    public override void OnPointerDown()
    {
        base.OnPointerDown();
    }

    public override void OnPointerUp()
    {
        base.OnPointerUp();
    }
}
