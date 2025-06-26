using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabAuthManager : MonoBehaviour
{
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
        Debug.Log("匿名ログイン成功！ PlayFabId: " + result.PlayFabId);
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
    }

    void OnDisplayNameSetFailure(PlayFabError error)
    {
        Debug.LogError("ニックネーム設定失敗: " + error.GenerateErrorReport());
    }
}
