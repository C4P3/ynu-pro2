using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class PlayFabAuthManager : MonoBehaviour
{
    [SerializeField] CanvasGroup loadingUI;
    [SerializeField] CanvasGroup loginUI;
    [SerializeField] CanvasGroup tittleUI;
    [SerializeField] string customIdPepper = "";

    public static PlayFabAuthManager Instance { get; private set; }
    public static EntityKey MyEntity { get; private set; }
    public static string MyDisplayName { get; private set; } // 表示名を保持する

    // ログイン状態を外部から確認できるようにするプロパティ
    public bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // 初回起動時にCustom ID（端末ID）で匿名ログインする
    async void Start()
    {
        await InitializeUnityServices();
        LoginWithCustomID();
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Unity Services Signed In: PlayerID={AuthenticationService.Instance.PlayerId}");
            }
        }
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
        MyEntity = result.EntityToken.Entity;
        
        // プロフィール（表示名）を取得
        GetPlayerProfile();

        SetUI(loadingUI, 0, false, false);

        if (result.NewlyCreated)
        {
            Debug.Log("初回ログイン（新規アカウント作成）成功");
            SetUI(loginUI, 1, true, true);
        }
        else
        {
            Debug.Log("既存アカウントでログイン成功");
            SetUI(loginUI, 0, false, false);
            SetUI(tittleUI, 1, true, true);
        }
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("ログイン失敗: " + error.GenerateErrorReport());
    }

    // プレイヤーのプロフィール情報を取得する
    private void GetPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };
        PlayFabClientAPI.GetPlayerProfile(request, OnGetProfileSuccess, OnGetProfileFailure);
    }

    private void OnGetProfileSuccess(GetPlayerProfileResult result)
    {
        MyDisplayName = result.PlayerProfile.DisplayName;
        Debug.Log($"表示名を取得しました: {MyDisplayName}");
    }

    private void OnGetProfileFailure(PlayFabError error)
    {
        Debug.LogError("プロフィール情報の取得に失敗: " + error.GenerateErrorReport());
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
        MyDisplayName = result.DisplayName; // 設定成功時にローカルの表示名も更新
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