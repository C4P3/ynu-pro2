using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IButton : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler
{
    [SerializeField] protected Image buttonImage;
    [SerializeField] protected AudioClip clickSound;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected CanvasGroup beforeUI;
    [SerializeField] protected CanvasGroup afterUI;

    protected Color hoverColor = new Color(1, 1, 0, 1);
    protected Color normalColor = new Color(1, 1, 1, 1); // 通常色
    protected Material matInstance;

    protected void Start()
    {
        if (buttonImage == null)
        {
            Debug.LogError("targetImageをInspectorで設定してください");
            return;
        }

        matInstance = Instantiate(buttonImage.material);
        buttonImage.material = matInstance;

        matInstance.SetColor("_HoverColor", hoverColor);
        matInstance.SetColor("_NormalColor", normalColor);
        matInstance.SetFloat("_IsHover", 0f);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverEffect(true);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        ApplyHoverEffect(false);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.color *= 0.7f;
        matInstance?.SetFloat("_ClickDarkness", 0.7f);
        matInstance?.SetFloat("_ClickSaturation", 1.3f);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.color /= 0.7f;
        matInstance?.SetFloat("_ClickDarkness", 1.0f);
        matInstance?.SetFloat("_ClickSaturation", 1.0f);
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        ApplyHoverEffect(true);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        ApplyHoverEffect(false);
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        OnPointerClick(null); // Enterキーでクリック処理実行
    }

    protected void ApplyHoverEffect(bool isHovering)
    {
        if (matInstance != null)
            matInstance.SetFloat("_IsHover", isHovering ? 1f : 0f);
    }

    public void ChangeUI(CanvasGroup canvasGroup, int alpha, bool interactable, bool blocksRaycasts)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}