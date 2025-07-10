using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] CanvasGroup loadingUI;
    [SerializeField] CanvasGroup loginUI;
    [SerializeField] CanvasGroup tittleUI;
    [SerializeField] CanvasGroup homeUI;
    [SerializeField] CanvasGroup ruleSelectUI;
    [SerializeField] CanvasGroup recordUI;
    [SerializeField] CanvasGroup configUI;
    [SerializeField] CanvasGroup multiplayUI;
    [SerializeField] CanvasGroup roomRuleSelectUI;
    [SerializeField] CanvasGroup roomIDInputUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //初期のUIの表示非表示
        SetUI(loadingUI, 1, true, true);
        SetUI(loginUI, 0, false, false);
        SetUI(tittleUI, 0, false, false);
        SetUI(homeUI, 0, false, false);
        SetUI(ruleSelectUI, 0, false, false);
        SetUI(recordUI, 0, false, false);
        SetUI(configUI, 0, false, false);
        SetUI(multiplayUI, 0, false, false);
        SetUI(roomRuleSelectUI, 0, false, false);
        SetUI(roomIDInputUI, 0, false, false);



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
