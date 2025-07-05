using PlayFab;
using PlayFab.ClientModels;
using Unity.VisualScripting;
using UnityEngine;

public class PlayFabAuthManager : MonoBehaviour
{
    [SerializeField] CanvasGroup loginUI;
    [SerializeField] CanvasGroup homeUI;
    [SerializeField] CanvasGroup roomUI;
    [SerializeField] CanvasGroup recordUI;
    [SerializeField] CanvasGroup configUI;

    public static PlayFabAuthManager Instance { get; private set; }
    public static EntityKey MyEntity { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // このマネージャーもシーン間で永続化
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初回起動時にCustom ID（端末ID）で匿名ログインする
    void Start()
    {
        LoginWithCustomID();
    }

    // 匿名ログイン（Custom ID使用）
    void LoginWithCustomID()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier, // 端末固有ID
            CreateAccount = true // 初回は自動でアカウント作成
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
         // ★★★ ログイン成功時にEntityKeyを保存 ★★★
        MyEntity = result.EntityToken.Entity;

        // if (result.NewlyCreated)
        // {
        // Debug.Log("初回ログイン（新規アカウント作成）！");
        // 初回ログイン専用処理（例：ニックネーム設定画面へ遷移）
        SetUI(loginUI, 1, true, true);
        SetUI(homeUI, 0, false, false);
        // }
        // else
        // {
        // Debug.Log("既存アカウントでログイン成功！");
        // // 通常のホーム画面へ遷移など
        // SetUI(loginUI, 0, false, false);
        // SetUI(homeUI, 1, true, true);
        // }
        SetUI(roomUI, 0, false, false);
        SetUI(recordUI, 0, false, false);
        SetUI(configUI, 0, false, false);
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("ログイン失敗: " + error.GenerateErrorReport());
    }

    // ユーザーがニックネームを入力した時に呼び出す
    public void SetDisplayName(string nickname)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname // ニックネームを設定
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameSet, OnDisplayNameSetFailure);
    }

    void OnDisplayNameSet(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("ニックネーム設定成功: " + result.DisplayName);
        SetUI(loginUI, 0, false, false);
        SetUI(homeUI, 1, true, true);
    }

    void OnDisplayNameSetFailure(PlayFabError error)
    {
        Debug.LogError("ニックネーム設定失敗: " + error.GenerateErrorReport());
    }

    public void SetUI(CanvasGroup canvasGroup, int alfha, bool interactable, bool blocksRaycasts){
        canvasGroup.alpha = alfha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
