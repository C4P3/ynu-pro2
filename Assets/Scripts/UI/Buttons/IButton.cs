using UnityEngine;
using UnityEngine.UI;

public class IButton : MonoBehaviour
{
    [SerializeField] protected Image buttonImage;
    [SerializeField] protected AudioClip clickSound;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected CanvasGroup beforeUI;
    [SerializeField] protected CanvasGroup afterUI;
    protected AudioClip mouseOverSound;
    protected Color hoverColor = new Color(1, 1, 0, 1);
    protected Color normalColor = new Color(1, 1, 1, 1);
    protected Material matInstance;
    protected Vector3 originalScale;
    private bool isSelected = false; // 選択状態を保持するフラグ

    protected void Awake()
    {
        originalScale = transform.localScale;
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

        mouseOverSound = Resources.Load<AudioClip>("選択音");
    }

    public void Select()
    {
        isSelected = true;
        matInstance.SetFloat("_IsHover", 1f);
        transform.localScale = originalScale * 1.05f;
    }

    public void Deselect()
    {
        isSelected = false;
        matInstance.SetFloat("_IsHover", 0f);
        transform.localScale = originalScale;
    }

    public virtual void OnPointerClick()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public virtual void OnPointerEnter()
    {
        matInstance.SetFloat("_IsHover", 1f);
        transform.localScale = originalScale * 1.05f;
        audioSource.PlayOneShot(mouseOverSound);
    }

    public virtual void OnPointerExit()
    {
        // 既に選択されている場合は何もしない
        if (isSelected) return;

        matInstance.SetFloat("_IsHover", 0f);
        transform.localScale = originalScale;
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
        canvasGroup.alpha = alfha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
