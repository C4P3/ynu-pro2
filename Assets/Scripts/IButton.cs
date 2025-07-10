using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
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
    protected Color normalColor = new Color(1, 1, 1, 1);
    protected Material matInstance;
    protected Vector3 originalScale;
    protected AudioClip mouseOverSound;


    protected void Start()
    {
        originalScale = transform.localScale;
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

        mouseOverSound = Resources.Load<AudioClip>("選択音");
    }

    // Submit（Enterキー）やクリックに対応
    public virtual void OnPointerClick(PointerEventData eventData = null)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData){
        matInstance.SetFloat("_IsHover", 1f);
        transform.localScale = originalScale * 1.05f;
         audioSource.PlayOneShot(mouseOverSound);       
    }

    public virtual void OnPointerExit(PointerEventData eventData ){
        matInstance.SetFloat("_IsHover", 0f);
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
        OnPointerEnter(null);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        OnPointerExit(null);
    }

    public void ChangeUI(CanvasGroup canvasGroup, int alpha, bool interactable, bool blocksRaycasts)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }

    public virtual void OnSubmit(BaseEventData eventData)
{
    Debug.Log("Submit (e.g. Enter key) pressed on button.");
    OnPointerClick(null); // クリック処理と共通にしたい場合
}
} 