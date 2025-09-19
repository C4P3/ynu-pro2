using UnityEngine;
using TMPro;
using System.Collections;
using Utp;

public class PlayFabMatchmakingManager : MonoBehaviour
{
    public static PlayFabMatchmakingManager Instance { get; private set; }

    public string roomId;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // 「ホストになる」ボタンから呼び出す
    public void CreateRoom()
    {
        UIController.Instance.statusText.text = "ホストを作成中...";
        MyRelayNetworkManager.Instance.StartRelayHost(1); // relayManagerの最大プレイヤー数はホスト以外の人数
        StartCoroutine(ShowJoinCodeCoroutine());
    }

    // Join Codeが生成されるのを待ってUIに表示する
    private IEnumerator ShowJoinCodeCoroutine()
    {
        UIController.Instance.statusText.text = "Join Codeを生成中...";
        // relayJoinCodeが空でなくなるまで毎フレーム待つ
        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null;
        }

        // Join Codeの表示
        roomId = MyRelayNetworkManager.Instance.relayJoinCode;
        UIController.Instance.statusText.text = "コードを相手に伝えてください";
        Debug.Log("Join Code is: " + MyRelayNetworkManager.Instance.relayJoinCode);
    }

    // 「参加する」ボタンから呼び出す
    public void JoinRoom()
    {
        string joinCode = UIController.Instance.roomIdInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            UIController.Instance.statusText.text = "Join Codeを入力してください";
            return;
        }

        UIController.Instance.statusText.text = $"コード '{joinCode}' で参加中...";
        MyRelayNetworkManager.Instance.relayJoinCode = joinCode;
        MyRelayNetworkManager.Instance.JoinRelayServer();
    }
}