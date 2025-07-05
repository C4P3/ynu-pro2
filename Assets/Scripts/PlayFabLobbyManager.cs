using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.ClientModels; // このusingを追加
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class PlayFabLobbyManager : MonoBehaviour
{
    public static PlayFabLobbyManager Instance { get; private set; }

    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TextMeshProUGUI roomIdDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;

    private const string ROOM_ID_KEY = "CurrentRoomId";
    private string _myTicketId;
    private Coroutine _pollTicketCoroutine; // ポーリング処理を管理するためのコルーチン変数
    private bool _isHost = false; // 自分がホストかどうかを記録するフラグ

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); } else { Destroy(gameObject); }
    }

    // 「ルームを作成」ボタンから呼び出す
    public void CreateRoom()
    {
        _isHost = true;
        statusText.text = "ルームを作成中...";
        string shortId = UnityEngine.Random.Range(100000, 999999).ToString();
        roomIdDisplayText.text = $"ルームID: {shortId}";

        // 自分のプレイヤーデータにルームIDを保存し、成功したらチケット作成に進む
        UpdateUserDataAndCreateTicket(shortId);
    }
    
    // 「ルームに参加」ボタンから呼び出す
    public void JoinRoom()
    {
        _isHost = false;
        string shortIdToJoin = roomIdInput.text;
        if (string.IsNullOrEmpty(shortIdToJoin)) return;

        statusText.text = $"ルームID '{shortIdToJoin}' で参加します...";
        
        // 自分のプレイヤーデータに参加したいルームIDを保存し、成功したらチケット作成に進む
        UpdateUserDataAndCreateTicket(shortIdToJoin);
    }

    // UserDataを更新し、成功したらチケット作成に進む共通メソッド
    private void UpdateUserDataAndCreateTicket(string roomId)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { ROOM_ID_KEY, roomId } }
        };
        // ★★★ コールバックに roomId を渡すように変更 ★★★
        PlayFabClientAPI.UpdateUserData(request, 
            (result) => { OnUserDataUpdated_CreateTicket(roomId); }, 
            OnError);
    }

    // UserData保存成功後、マッチングチケットを作成する
    private void OnUserDataUpdated_CreateTicket(string roomId)
    {
        statusText.text = "対戦相手を探しています...";

        var matchmakingPlayer = new MatchmakingPlayer
        {
            Entity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = PlayFabAuthManager.MyEntity.Id,
                Type = PlayFabAuthManager.MyEntity.Type
            },
            // ★★★ ここが最重要：マッチングに使う属性を明示的に指定する ★★★
            Attributes = new MatchmakingPlayerAttributes
            {
                DataObject = new { CurrentRoomId = roomId }
            }
        };

        var ticketRequest = new CreateMatchmakingTicketRequest
        {
            Creator = matchmakingPlayer,
            GiveUpAfterSeconds = 120,
            QueueName = "PrivateRoomQueue"
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(ticketRequest, OnTicketCreated, OnError);
    }

    private void OnTicketCreated(CreateMatchmakingTicketResult result)
    {
        _myTicketId = result.TicketId;
        Debug.Log($"Matchmaking ticket created: {_myTicketId}");
        
        // ★★★ チケットのステータス監視コルーチンを開始 ★★★
        _pollTicketCoroutine = StartCoroutine(PollTicketStatusCoroutine());
    }

    // チケットの状態を定期的に確認するコルーチン
    private IEnumerator PollTicketStatusCoroutine()
    {
        while (true)
        {
            // ★★★ ログ追加1: ポーリングが実行されているか確認 ★★★
            Debug.Log($"[{(_isHost ? "HOST" : "CLIENT")}] Polling ticket... TicketId: {_myTicketId}");

            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest { TicketId = _myTicketId, QueueName = "PrivateRoomQueue" },
                OnGetTicketStatusSuccess,
                OnError // ★★★ エラーハンドラは設定済み
            );
            
            yield return new WaitForSeconds(3f);
        }
    }

    // チケット状態取得成功時のコールバック
    private void OnGetTicketStatusSuccess(GetMatchmakingTicketResult result)
    {
        switch (result.Status)
        {
            case "Matched":
                if (_pollTicketCoroutine != null)
                {
                    StopCoroutine(_pollTicketCoroutine);
                }
                
                statusText.text = "マッチ成立！ゲームを開始します...";
                Debug.Log($"Match found! MatchId: {result.MatchId}");

                if (_isHost)
                {
                    // ホストは即座にサーバーを開始
                    MyNetworkManager.singleton.StartHost();
                }
                else
                {
                    // ★★★ クライアントは直接接続せず、遅延コルーチンを開始 ★★★
                    StartCoroutine(ConnectAsClientWithDelay());
                }
                break;
            
            case "Canceled":
                // チケットがキャンセルされた（タイムアウトなど）
                StopCoroutine(_pollTicketCoroutine);
                statusText.text = "マッチングがキャンセルされました。";
                break;

            // "WaitingForPlayers", "WaitingForMatch" の場合は、何もしないで次のポーリングを待つ
        }
    }

    // ★★★ クライアントが遅れて接続を開始するためのコルーチンを新しく追加 ★★★
    private IEnumerator ConnectAsClientWithDelay()
    {
        // ホスト側の準備が整うのを2秒ほど待つ（この時間は適宜調整）
        statusText.text = "ホストに接続しています...";
        yield return new WaitForSeconds(2f);

        // ローカルテストなので、localhostに接続
        MyNetworkManager.singleton.networkAddress = "localhost";
        MyNetworkManager.singleton.StartClient();
    }

    // 共通のエラー処理
    private void OnError(PlayFabError error)
    {
        statusText.text = "エラーが発生しました";
        // ★★★ ログ追加3: エラーがどちらで発生したか明確にする ★★★
        Debug.LogError($"!!! PLAYFAB ERROR on {(_isHost ? "HOST" : "CLIENT")}!!!: " + error.GenerateErrorReport());
    }
}