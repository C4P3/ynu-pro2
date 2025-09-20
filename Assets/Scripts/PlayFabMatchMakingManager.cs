using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.DataModels;
using PlayFab.Json;

public class PlayFabMatchmakingManager : MonoBehaviour
{
    public static PlayFabMatchmakingManager Instance { get; private set; }

    private string ticketId;
    private string myEntityId;
    private Coroutine _pollTicketCoroutine;

    #region  手動接続（ルームマッチ）
    // 自動マッチングでは使用しない

    public string roomId;

    // 「ホストになる」ボタンから呼び出す
    public void CreateRoom()
    {
        PlayerHUDManager.Instance.statusText.text = "ホストを作成中...";
        MyRelayNetworkManager.Instance.StartRelayHost(1); // relayManagerの最大プレイヤー数はホスト以外の人数
        StartCoroutine(ShowJoinCodeCoroutine());
    }

    // Join Codeが生成されるのを待ってUIに表示する
    private IEnumerator ShowJoinCodeCoroutine()
    {
        PlayerHUDManager.Instance.statusText.text = "Join Codeを生成中...";
        // relayJoinCodeが空でなくなるまで毎フレーム待つ
        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null;
        }

        // Join Codeの表示
        roomId = MyRelayNetworkManager.Instance.relayJoinCode;
        PlayerHUDManager.Instance.statusText.text = "コードを相手に伝えてください";
        Debug.Log("Join Code is: " + MyRelayNetworkManager.Instance.relayJoinCode);
        PlayerHUDManager.Instance.UpdateWaitingPanel();
    }

    // 「参加する」ボタンから呼び出す
    public void JoinRoom()
    {
        string joinCode = PlayerHUDManager.Instance.roomIdInputField.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            PlayerHUDManager.Instance.statusText.text = "Join Codeを入力してください";
            return;
        }

        PlayerHUDManager.Instance.statusText.text = $"コード '{joinCode}' で参加中...";
        MyRelayNetworkManager.Instance.relayJoinCode = joinCode;
        MyRelayNetworkManager.Instance.JoinRelayServer();
    }
    #endregion

    #region 自動マッチングフロー
    // PlayFabのキューを利用した自動マッチング

    /// <summary>
    /// ランダムマッチングを開始するエントリーポイント
    /// </summary>
    public void StartRandomMatchmaking()
    {
        PlayerHUDManager.Instance.statusText.text = "対戦相手を探しています...";
        Debug.Log("マッチメイキングチケットを作成します...");

        // 自身のEntity情報を取得（PlayFabにログイン済みであること）
        myEntityId = PlayFabSettings.staticPlayer.EntityId;
        var entityKey = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = myEntityId,
            Type = PlayFabSettings.staticPlayer.EntityType
        };

        var request = new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer { Entity = entityKey },
            GiveUpAfterSeconds = 30,
            QueueName = "1vs1RandomMatch"
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnCreateTicketSuccess, OnPlayFabError);
    }

    private void OnCreateTicketSuccess(CreateMatchmakingTicketResult result)
    {
        ticketId = result.TicketId;
        Debug.Log($"チケット作成成功: {ticketId}");

        // チケットの状態をポーリングするコルーチンを開始
        _pollTicketCoroutine = StartCoroutine(PollTicketStatusCoroutine(result.TicketId, "1vs1RandomMatch"));
    }

    private IEnumerator PollTicketStatusCoroutine(string tId, string queueName)
    {
        while (true)
        {
            Debug.Log("チケットの状態を確認中...");
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest { TicketId = tId, QueueName = queueName },
                OnGetTicketStatusSuccess,
                OnPlayFabError
            );
            // 推奨される6秒間隔（1分に10回まで。）でポーリング
            yield return new WaitForSeconds(6f);
        }
    }

    private void OnGetTicketStatusSuccess(GetMatchmakingTicketResult result)
    {
        switch (result.Status)
        {
            case "Matched":
                // マッチング成功！ポーリングを停止し、マッチ情報を取得する
                StopCoroutine(_pollTicketCoroutine);
                _pollTicketCoroutine = null;
                Debug.Log($"マッチング成功！ MatchId: {result.MatchId}");
                PlayerHUDManager.Instance.statusText.text = "対戦相手が見つかりました！";
                GetMatchDetails(result.MatchId, result.QueueName);
                break;
            case "Canceled":
                // チケットがキャンセルされた（タイムアウトなど）
                StopCoroutine(_pollTicketCoroutine);
                _pollTicketCoroutine = null;
                Debug.LogWarning("チケットはキャンセルされました。");
                PlayerHUDManager.Instance.statusText.text = "対戦相手が見つかりませんでした。";
                break;
            case "WaitingForPlayers":
            case "WaitingForMatch":
            default:
                Debug.Log("対戦相手を待っています...");
                break;
        }
    }

    private void GetMatchDetails(string matchId, string queueName)
    {
        Debug.Log("マッチの詳細情報を取得します...");
        PlayFabMultiplayerAPI.GetMatch(
            new GetMatchRequest { MatchId = matchId, QueueName = queueName },
            OnGetMatchSuccess,
            OnPlayFabError
        );
    }

    private void OnGetMatchSuccess(GetMatchResult result)
    {
        Debug.Log("マッチ情報の取得に成功。メンバー:");
        foreach (var member in result.Members)
        {
            Debug.Log($"- Entity ID: {member.Entity.Id}");
        }

        // メンバーリストの最初のプレイヤーをホストとする
        string hostEntityId = result.Members[0].Entity.Id;

        if (hostEntityId == myEntityId)
        {
            Debug.Log("自分がホストです。Relayサーバーを開始します。");
            StartCoroutine(HostFlow(result.MatchId));
        }
        else
        {
            Debug.Log("自分はクライアントです。ホストのJoin Codeを待ちます。");
            StartCoroutine(ClientFlow(result.MatchId));
        }
    }

    private IEnumerator HostFlow(string matchId)
    {
        // 1. Relayホストを開始
        PlayerHUDManager.Instance.statusText.text = "ホストを作成中...";
        MyRelayNetworkManager.Instance.StartRelayHost(1);

        // 2. Join Codeが生成されるのを待つ
        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null;
        }
        string joinCode = MyRelayNetworkManager.Instance.relayJoinCode;
        Debug.Log($"Join Code生成完了: {joinCode}");

        // 3. Join CodeをPlayFabのEntity Objectに書き込む
        PlayerHUDManager.Instance.statusText.text = "接続情報を共有中...";
        ShareJoinCodeOnPlayFab(matchId, joinCode);
    }

    private void ShareJoinCodeOnPlayFab(string matchId, string joinCode)
    {
        var request = new SetObjectsRequest
        {
            // マッチIDをEntityとして扱う
            Entity = new PlayFab.DataModels.EntityKey { Id = matchId, Type = "match" },
            Objects = new List<SetObject>
            {
                new SetObject
                {
                    ObjectName = "RelayInfo",
                    DataObject = new { JoinCode = joinCode } // 匿名型でJSONオブジェクトを作成
                }
            }
        };
        PlayFabDataAPI.SetObjects(request, OnSetJoinCodeSuccess, OnPlayFabError);
    }

    private void OnSetJoinCodeSuccess(SetObjectsResponse response)
    {
        Debug.Log("Join Codeの共有に成功しました。");
        PlayerHUDManager.Instance.statusText.text = "相手の参加を待っています...";
        // ここで待機画面などを表示
    }

    private IEnumerator ClientFlow(string matchId)
    {
        PlayerHUDManager.Instance.statusText.text = "ホストの情報を待っています...";
        string joinCode = null;
        float timeout = 30f; // 30秒でタイムアウト

        while (timeout > 0 && string.IsNullOrEmpty(joinCode))
        {
            // 2秒ごとにJoin Codeをポーリング
            yield return new WaitForSeconds(2f);
            timeout -= 2f;

            var request = new GetObjectsRequest { Entity = new PlayFab.DataModels.EntityKey { Id = matchId, Type = "match" } };
            PlayFabDataAPI.GetObjects(request,
                (result) =>
                {
                    if (result.Objects.TryGetValue("RelayInfo", out var obj))
                    {
                        // PlayFabのSimpleJsonを使ってパース
                        var json = PlayFabSimpleJson.SerializeObject(obj.DataObject);
                        var data = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(json);
                        if (data.TryGetValue("JoinCode", out var code))
                        {
                            joinCode = code.ToString();
                        }
                    }
                },
                (error) => { /* エラーはOnPlayFabErrorで処理 */ }
            );

            // joinCodeが取得できるまで待機
            yield return new WaitUntil(() => joinCode != null);
        }

        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log($"Join Code '{joinCode}' を取得しました。Relayに参加します。");
            PlayerHUDManager.Instance.statusText.text = $"コード '{joinCode}' で参加中...";
            MyRelayNetworkManager.Instance.relayJoinCode = joinCode;
            MyRelayNetworkManager.Instance.JoinRelayServer();
        }
        else
        {
            Debug.LogError("Join Codeの取得に失敗しました（タイムアウト）。");
            PlayerHUDManager.Instance.statusText.text = "エラー: 参加に失敗しました。";
        }
    }

    #endregion

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError("PlayFab API Error: " + error.GenerateErrorReport());
        PlayerHUDManager.Instance.statusText.text = "エラーが発生しました";
        // 実行中のコルーチンがあれば停止する
        if (_pollTicketCoroutine != null)
        {
            StopCoroutine(_pollTicketCoroutine);
            _pollTicketCoroutine = null;
        }
    }
}