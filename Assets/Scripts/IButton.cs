using UnityEngine;
using UnityEngine.UI;

public class IButton : MonoBehaviour
{
    [SerializeField] protected Image buttonImage;
    [SerializeField] protected AudioClip clickSound;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected Color hoverColor = Color.red;
    [SerializeField] protected Color normalColor = new Color(0, 0, 0, 0); // 透明
    protected Material matInstance;

    protected void Start()
    {
        if (buttonImage == null)
        {
            Debug.LogError("targetImageをInspectorで設定してください");
            return;
        }

        // マテリアルをインスタンス化して独立制御
        matInstance = Instantiate(buttonImage.material);
        buttonImage.material = matInstance;

        matInstance.SetColor("_HoverColor", hoverColor);
        matInstance.SetColor("_NormalColor", normalColor);
        matInstance.SetFloat("_IsHover", 0f);
    }

    public virtual void OnPointerClick()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public virtual void OnPointerEnter(){
        matInstance.SetFloat("_IsHover", 1f);
        Debug.Log("排他");
    }

    public virtual void OnPointerExit(){
        matInstance.SetFloat("_IsHover", 0f);
    }

    public virtual void OnPointerDown()
    {
        buttonImage.color = buttonImage.color * 0.7f;
        if (matInstance != null)
        {
            matInstance.SetFloat("_ClickDarkness", 0.7f);
            matInstance.SetFloat("_ClickSaturation", 1.3f);
        }
    }

    public virtual void OnPointerUp()
    {
        buttonImage.color = buttonImage.color / 0.7f;
        if (matInstance != null)
        {
            matInstance.SetFloat("_ClickDarkness", 1.0f);
            matInstance.SetFloat("_ClickSaturation", 1.0f);
        }
    }

    public void ChangeUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){//UIの表示非表示
        canvasGroup.alpha = alfha;//透明度
        canvasGroup.interactable = interactable;//
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}

