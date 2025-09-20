using UnityEngine;
using TMPro;
using System.Collections;
using Utp;
using PlayFab.MultiplayerModels;
using PlayFab;

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
        PlayerHUDManager.Instance.UpdateWaitingPanel();
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

    public void JoinRandomMatch()
    {
        Matchmaking();
    }

    private void Matchmaking()
    {
        Debug.Log("マッチメイキングチケットをキューに積みます...\n");

        // プレイヤーの情報を作ります。
        var matchmakingPlayer = new MatchmakingPlayer
        {
            // Entityは下記のコードで決め打ちで大丈夫です。
            Entity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = PlayFabSettings.staticPlayer.EntityId,
                Type = PlayFabSettings.staticPlayer.EntityType
            }
        };

        var request = new CreateMatchmakingTicketRequest
        {
            // 先程作っておいたプレイヤー情報です。
            Creator = matchmakingPlayer,
            // マッチングできるまで待機する秒数を指定します。最大600秒です。
            GiveUpAfterSeconds = 30,
            // GameManagerで作ったキューの名前を指定します。
            QueueName = "1vs1RandomMatch"
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnCreateMatchmakingTicketSuccess, OnFailure);

        void OnCreateMatchmakingTicketSuccess(CreateMatchmakingTicketResult result)
        {
            Debug.Log("マッチメイキングチケットをキューに積みました！\n\n");

            // キューに積んだチケットの状態をマッチングするかタイムアウトするまでポーリングします。
            var getMatchmakingTicketRequest = new GetMatchmakingTicketRequest
            {
                TicketId = result.TicketId,
                QueueName = request.QueueName
            };

            StartCoroutine(Polling(getMatchmakingTicketRequest));
        }
    }

    IEnumerator Polling(GetMatchmakingTicketRequest request)
    {
        // ポーリングは1分間に10回まで許可されているので、6秒間隔で実行するのがおすすめです。
        var seconds = 6f;
        var MatchedOrCanceled = false;

        while (true)
        {
            if (MatchedOrCanceled)
            {
                yield break;
            }

            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGetMatchmakingTicketSuccess, OnFailure);
            yield return new WaitForSeconds(seconds);
        }

        void OnGetMatchmakingTicketSuccess(GetMatchmakingTicketResult result)
        {
            switch (result.Status)
            {
                case "Matched":
                    MatchedOrCanceled = true;
                    Debug.Log($"対戦相手が見つかりました！\n\nMatchIDは {result.MatchId} です！");
                    return;

                case "Canceled":
                    MatchedOrCanceled = true;
                    Debug.Log("対戦相手が見つからないのでキャンセルしました...");
                    return;

                default:
                    Debug.Log("対戦相手が見つかるまで待機します...\n");
                    return;
            }
        }
    }

    void OnFailure(PlayFabError error)
    {
        Debug.Log($"{error.ErrorMessage}");
    }
}