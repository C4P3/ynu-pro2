using UnityEngine;
using TMPro;
using System.Collections;

public class PlayFabMatchmakingManager : MonoBehaviour
{
    public static PlayFabMatchmakingManager Instance { get; private set; }

    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TextMeshProUGUI roomIdDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private MyRelayNetworkManager relayManager;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // 「ホストになる」ボタンから呼び出す
    public void CreateRoom()
    {
        statusText.text = "ホストを作成中...";

        relayManager.StartRelayHost(1); // relayManagerの最大プレイヤー数はホスト以外の人数
        StartCoroutine(ShowJoinCodeCoroutine());
    }

    // Join Codeが生成されるのを待ってUIに表示する
    private IEnumerator ShowJoinCodeCoroutine()
    {
        statusText.text = "Join Codeを生成中...";
        // relayJoinCodeが空でなくなるまで毎フレーム待つ
        while (string.IsNullOrEmpty(relayManager.relayJoinCode))
        {
            yield return null;
        }

        // Join Codeの表示
        roomIdDisplayText.text = relayManager.relayJoinCode;
        statusText.text = "コードを相手に伝えてください";
        Debug.Log("Join Code is: " + relayManager.relayJoinCode);
    }

    // 「参加する」ボタンから呼び出す
    public void JoinRoom()
    {
        string joinCode = roomIdInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            statusText.text = "Join Codeを入力してください";
            return;
        }

        statusText.text = $"コード '{joinCode}' で参加中...";
        relayManager.relayJoinCode = joinCode;
        relayManager.JoinRelayServer();
    }
}