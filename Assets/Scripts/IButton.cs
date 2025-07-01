using UnityEngine;
using UnityEngine.UI;

public abstract class IButton : MonoBehaviour
{
    [SerializeField] protected Image buttonImage;
    [SerializeField] protected AudioClip clickSound;
    [SerializeField] protected AudioSource audioSource;

    public virtual void OnPointerClick()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public abstract void OnPointerEnter();

    public abstract void OnPointerExit();

    public virtual void OnPointerDown()
    {
        buttonImage.color = buttonImage.color * 0.7f;
    }

    public virtual void OnPointerUp()
    {
        buttonImage.color = buttonImage.color / 0.7f;
    }

    public void ChangeUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){//UIの表示非表示
        canvasGroup.alpha = alfha;//透明度
        canvasGroup.interactable = interactable;//
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}

