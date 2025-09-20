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
        Debug.Log("ホストを作成中...");
        MyRelayNetworkManager.Instance.StartRelayHost(1); // relayManagerの最大プレイヤー数はホスト以外の人数
        StartCoroutine(ShowJoinCodeCoroutine());
    }

    // Join Codeが生成されるのを待ってUIに表示する
    private IEnumerator ShowJoinCodeCoroutine()
    {
        if (PlayerHUDManager.Instance?.statusText != null)
        {
            PlayerHUDManager.Instance.statusText.text = "Join Codeを生成中...";
        }
        // relayJoinCodeが空でなくなるまで毎フレーム待つ
        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null;
        }

        // Join Codeの表示
        roomId = MyRelayNetworkManager.Instance.relayJoinCode;
        if (PlayerHUDManager.Instance?.statusText != null)
        {
            PlayerHUDManager.Instance.statusText.text = "コードを相手に伝えてください";
            PlayerHUDManager.Instance.UpdateWaitingPanel();
        }
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

        Debug.Log($"コード '{joinCode}' で参加中...");
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
        Debug.Log("対戦相手を探しています...");
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
                Debug.Log("対戦相手が見つかりました！");
                GetMatchDetails(result.MatchId, result.QueueName);
                break;
            case "Canceled":
                // チケットがキャンセルされた（タイムアウトなど）
                StopCoroutine(_pollTicketCoroutine);
                _pollTicketCoroutine = null;
                Debug.LogWarning("チケットはキャンセルされました。");
                Debug.Log("対戦相手が見つかりませんでした。");
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
        Debug.Log( "ホストを作成中...");
        MyRelayNetworkManager.Instance.StartRelayHost(1);

        // 2. Join Codeが生成されるのを待つ
        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null;
        }
        string joinCode = MyRelayNetworkManager.Instance.relayJoinCode;
        Debug.Log($"Join Code生成完了: {joinCode}");

        // 3. Join CodeをPlayFabのEntity Objectに書き込む
        if (PlayerHUDManager.Instance?.statusText != null)
        {
            PlayerHUDManager.Instance.statusText.text = "マッチング完了！相手を待っています...";
            PlayerHUDManager.Instance.roomIdInputField.text = "接続情報を共有中...";
        }
        ShareJoinCodeOnPlayFab(matchId, joinCode);
    }

    private void ShareJoinCodeOnPlayFab(string matchId, string joinCode)
    {
        var request = new SetObjectsRequest
        {
            // Title Entity を使用する (ゲーム全体で共有されるデータ領域)
            Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabSettings.TitleId, Type = "title" },
            Objects = new List<SetObject>
            {
                new SetObject
                {
                    // ObjectName に MatchId を使用して、このマッチ専用のデータとする
                    ObjectName = "matchId",
                    DataObject = new { JoinCode = joinCode } // 匿名型でJSONオブジェクトを作成
                }
            }
        };
        PlayFabDataAPI.SetObjects(request, OnSetJoinCodeSuccess, OnPlayFabError);
    }

    private void OnSetJoinCodeSuccess(SetObjectsResponse response)
    {
        Debug.Log("Join Codeの共有に成功しました。");
        if (PlayerHUDManager.Instance?.statusText != null)
        {
            PlayerHUDManager.Instance.statusText.text = "相手の参加を待っています...";
        }
        // ここで待機画面などを表示
    }

    private IEnumerator ClientFlow(string matchId)
    {
        Debug.Log("ホストの情報を待っています...");
        string joinCode = null;
        float timeout = 30f; // 30秒でタイムアウト
        
        while (timeout > 0 && string.IsNullOrEmpty(joinCode))
        {
            // Title Entity 上のオブジェクトを取得するリクエスト
            var request = new GetObjectsRequest { Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabSettings.TitleId, Type = "title" } };
            
            PlayFabDataAPI.GetObjects(request,
                (result) =>
                {
                    // ObjectName が matchId と一致するものを探す
                    if (result.Objects.TryGetValue(matchId, out var obj))
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
                (error) => 
                { 
                    // OnPlayFabErrorで処理されるが、ポーリング中の軽微なエラーは無視してもよい
                    Debug.LogWarning("Polling for Join Code failed, will retry...");
                }
            );
            
            // 2秒待ってから再試行する。この間に上記の非同期コールバックがjoinCodeをセットする可能性がある
            yield return new WaitForSeconds(2f);
            timeout -= 2f;
        }

        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log($"Join Code '{joinCode}' を取得しました。Relayに参加します。");
            Debug.Log( $"コード '{joinCode}' で参加中...");
            MyRelayNetworkManager.Instance.relayJoinCode = joinCode;
            MyRelayNetworkManager.Instance.JoinRelayServer();
        }
        else
        {
            Debug.LogError("Join Codeの取得に失敗しました（タイムアウト）。");
            Debug.Log("エラー: 参加に失敗しました。");
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
        Debug.Log("エラーが発生しました");
        // 実行中のコルーチンがあれば停止する
        if (_pollTicketCoroutine != null)
        {
            StopCoroutine(_pollTicketCoroutine);
            _pollTicketCoroutine = null;
        }
    }
}