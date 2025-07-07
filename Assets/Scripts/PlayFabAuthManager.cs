using PlayFab;
using PlayFab.ClientModels;
using Unity.VisualScripting;
using UnityEngine;

public class PlayFabAuthManager : MonoBehaviour
{
    [SerializeField] CanvasGroup loadingUI;
    [SerializeField] CanvasGroup loginUI;
    [SerializeField] CanvasGroup tittleUI;
    [SerializeField] string customIdPepper = "";

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
        string customId = SystemInfo.deviceUniqueIdentifier;
        // エディタ実行時や開発ビルドの場合、IDをユニークにするためのサフィックスを追加
        if (Application.isEditor || Debug.isDebugBuild)
        {
            customId += customIdPepper;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId, // 端末固有ID
            CreateAccount = true // 初回は自動でアカウント作成
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        // ★★★ ログイン成功時にEntityKeyを保存 ★★★
        MyEntity = result.EntityToken.Entity;

        SetUI(loadingUI, 0, false, false);

        // if (result.NewlyCreated)
        // {
        //     Debug.Log("初回ログイン（新規アカウント作成）！");
        //     // 初回ログイン専用処理（例：ニックネーム設定画面へ遷移）
        //     SetUI(loginUI, 1, true, true);
        // }
        // else
        // {
        //     Debug.Log("既存アカウントでログイン成功！");
        //     // 通常のホーム画面へ遷移など
        //     SetUI(tittleUI, 1, true, true);
        // }
        
        //デバッグ用：通常の処理がしたいときは上の処理を使う
        SetUI(loginUI, 1, true, true);
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
        SetUI(tittleUI, 1, true, true);
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
