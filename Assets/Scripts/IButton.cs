using UnityEngine;

public abstract class IButton : MonoBehaviour
{
    public abstract void OnPointerClick();
    public abstract void OnPointerEnter();
    public abstract void OnPointerExit();

    public void ChangeUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){//UIの表示非表示
        canvasGroup.alpha = alfha;//透明度
        canvasGroup.interactable = interactable;//
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}

