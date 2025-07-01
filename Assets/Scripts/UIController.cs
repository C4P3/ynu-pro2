using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] CanvasGroup loginUI;
    [SerializeField] CanvasGroup homeUI;
    [SerializeField] CanvasGroup recordUI;
    [SerializeField] CanvasGroup configUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //初期のUIの表示非表示
        SetUI(loginUI, 1, true, true);
        SetUI(homeUI, 0, false, false);
        SetUI(recordUI, 0, false, false);
        SetUI(configUI, 0, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){
        canvasGroup.alpha = alfha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
