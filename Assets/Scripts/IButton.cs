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


    protected virtual void Start()
    {
        originalScale = transform.localScale;
        if (buttonImage != null)
        {
            // マテリアルをインスタンス化して独立制御
            matInstance = Instantiate(buttonImage.material);
            buttonImage.material = matInstance;

            matInstance.SetColor("_HoverColor", hoverColor);
            matInstance.SetColor("_NormalColor", normalColor);
            matInstance.SetFloat("_IsHover", 0f);
        }
        
        // オーディオソースがなければ取得または追加
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        mouseOverSound = Resources.Load<AudioClip>("選択音");
    }

    // --- Public methods for UnityEvents (e.g. Button.onClick) ---

    public virtual void OnPointerClick()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public virtual void OnPointerEnter()
    {
        if (matInstance != null)
        {
            matInstance.SetFloat("_IsHover", 1f);
        }
        transform.localScale = originalScale * 1.05f;
        if (mouseOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(mouseOverSound);
        }
    }

    public virtual void OnPointerExit()
    {
        if (matInstance != null)
        {
            matInstance.SetFloat("_IsHover", 0f);
        }
        transform.localScale = originalScale;
    }

    public virtual void OnPointerDown()
    {
        if(buttonImage != null)
        {
            buttonImage.color *= 0.7f;
        }
        matInstance?.SetFloat("_ClickDarkness", 0.7f);
        matInstance?.SetFloat("_ClickSaturation", 1.3f);
    }

    public virtual void OnPointerUp()
    {
        if(buttonImage != null)
        {
            buttonImage.color /= 0.7f;
        }
        matInstance?.SetFloat("_ClickDarkness", 1.0f);
        matInstance?.SetFloat("_ClickSaturation", 1.0f);
    }

    // --- Interface Implementations ---

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUp();
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnPointerEnter();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnPointerExit();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnPointerClick();
    }

    public void ChangeUI(CanvasGroup canvasGroup, float alpha, bool interactable, bool blocksRaycasts)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = blocksRaycasts;
        }
    }
}