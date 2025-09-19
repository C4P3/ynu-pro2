using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] public  CanvasGroup loadingUI;
    [SerializeField] public  CanvasGroup loginUI;
    [SerializeField] public  CanvasGroup tittleUI;
    [SerializeField] public  CanvasGroup homeUI;
    [SerializeField] public  CanvasGroup ruleSelectUI;
    [SerializeField] public  CanvasGroup recordUI;
    [SerializeField] public  CanvasGroup rankingUI;
    [SerializeField] public  CanvasGroup configUI;
    [SerializeField] public  CanvasGroup multiplayUI;
    [SerializeField] public  CanvasGroup roomRuleSelectUI;
    [SerializeField] public  CanvasGroup roomIDInputUI;
    [SerializeField] public TMP_InputField roomIdInput;
    [SerializeField] public TextMeshProUGUI statusText;
    
    public void ShowInitialUI()
    {
        //初期のUIの表示非表示
        SetUI(loadingUI, 0, false, false); // Loadingは不要
        SetUI(loginUI, 0, false, false);
        SetUI(tittleUI, 1, true, true); // タイトルを表示
        SetUI(homeUI, 0, false, false);
        SetUI(ruleSelectUI, 0, false, false);
        SetUI(recordUI, 0, false, false);
        SetUI(rankingUI, 0, false, false);
        SetUI(configUI, 0, false, false);
        SetUI(multiplayUI, 0, false, false);
        SetUI(roomRuleSelectUI, 0, false, false);
        SetUI(roomIDInputUI, 0, false, false);
    }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    void Start()
    {
        // PlayFabAuthManagerが既にログイン済みかチェック
        if (PlayFabAuthManager.Instance != null && PlayFabAuthManager.Instance.IsLoggedIn)
        {
            // ログイン済みの場合（＝GameSceneから戻ってきた場合）、直接タイトルを表示
            ShowInitialUI();
        }
        else
        {
            // 未ログインの場合（＝初回起動）、ローディング画面から開始
            SetUI(loadingUI, 1, true, true);
            SetUI(loginUI, 0, false, false);
            SetUI(tittleUI, 0, false, false);
            SetUI(homeUI, 0, false, false);
            SetUI(ruleSelectUI, 0, false, false);
            SetUI(recordUI, 0, false, false);
            SetUI(rankingUI, 0, false, false);
            SetUI(configUI, 0, false, false);
            SetUI(multiplayUI, 0, false, false);
            SetUI(roomRuleSelectUI, 0, false, false);
            SetUI(roomIDInputUI, 0, false, false);
        }

        PlayFabAuthManager.Instance.InitializeAndLogin();
    }

    void Update()
    {
        
    }

    public void SetUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){
        canvasGroup.alpha = alfha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
