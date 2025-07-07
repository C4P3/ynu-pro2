using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Mirror;
using Unity.Networking.Transport.Relay;
using Utp;

public class PlayFabMatchmakingManager : MonoBehaviour
{
    public static PlayFabMatchmakingManager Instance { get; private set; }

    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TextMeshProUGUI roomIdDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;

    // 定数
    private const string ROOM_ID_KEY = "CurrentRoomId";
    private const string JOIN_CODE_KEY = "RelayJoinCode";

    // 状態変数
    private UtpTransport _utpTransport;
    private string _myTicketId;
    private GetMatchmakingTicketResult _matchedTicketResult;
    private Coroutine _pollTicketCoroutine;
    private bool _isHost = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 最初にTransportコンポーネントを取得しておく
        _utpTransport = NetworkManager.singleton.GetComponent<UtpTransport>();
    }

    public void CreateRoom()
    {
        _isHost = true;
        statusText.text = "ルームを作成中...";
        string shortId = Random.Range(100000, 999999).ToString();
        roomIdDisplayText.text = $"ルームID: {shortId}";
        
        // ★★★ 新しいチケット作成処理を呼び出す ★★★
        CancelTicketIfExistsAndCreateNew(shortId);
    }

    public void JoinRoom()
    {
        _isHost = false;
        string shortIdToJoin = roomIdInput.text;
        if (string.IsNullOrEmpty(shortIdToJoin)) return;
        statusText.text = $"ルームID '{shortIdToJoin}' で参加します...";
        
        // ★★★ 新しいチケット作成処理を呼び出す ★★★
        CancelTicketIfExistsAndCreateNew(shortIdToJoin);
    }

    // ★★★ 古いチケットがあればキャンセルし、なければ新しいチケットを作成するメソッド ★★★
    private void CancelTicketIfExistsAndCreateNew(string roomId)
    {
        // もし古いチケットIDが残っていれば
        if (!string.IsNullOrEmpty(_myTicketId))
        {
            statusText.text = "古いチケットをキャンセル中...";
            var request = new CancelMatchmakingTicketRequest
            {
                QueueName = "PrivateRoomQueue",
                TicketId = _myTicketId
            };
            PlayFabMultiplayerAPI.CancelMatchmakingTicket(request, 
                (result) => {
                    // キャンセル成功後、新しいチケットを作成
                    Debug.Log("Old ticket cancelled successfully.");
                    UpdateUserDataAndCreateTicket(roomId);
                },
                (error) => {
                    // キャンセルに失敗した場合でも、とりあえず新しいチケット作成を試みる
                    Debug.LogWarning("Old ticket cancellation failed. It might have already expired. Trying to create a new ticket anyway.");
                    UpdateUserDataAndCreateTicket(roomId);
                }
            );
        }
        else
        {
            // 古いチケットがない場合は、直接新しいチケットを作成
            UpdateUserDataAndCreateTicket(roomId);
        }
    }

    private void UpdateUserDataAndCreateTicket(string roomId)
    {
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { ROOM_ID_KEY, roomId } } };
        PlayFabClientAPI.UpdateUserData(request, (result) => { OnUserDataUpdated_CreateTicket(roomId); }, OnError);
    }

    private void OnUserDataUpdated_CreateTicket(string roomId)
    {
        statusText.text = "対戦相手を探しています...";
        var matchmakingPlayer = new MatchmakingPlayer
        {
            Entity = new PlayFab.MultiplayerModels.EntityKey { Id = PlayFabAuthManager.MyEntity.Id, Type = PlayFabAuthManager.MyEntity.Type },
            Attributes = new MatchmakingPlayerAttributes { DataObject = new { CurrentRoomId = roomId } }
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
        _pollTicketCoroutine = StartCoroutine(PollTicketStatusCoroutine());
    }

    private IEnumerator PollTicketStatusCoroutine()
    {
        while (true)
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest { TicketId = _myTicketId, QueueName = "PrivateRoomQueue" },
                OnGetTicketStatusSuccess, OnError);
            yield return new WaitForSeconds(6f);
        }
    }

    // チケット状態取得成功時のコールバック
    private void OnGetTicketStatusSuccess(GetMatchmakingTicketResult result)
    {
        if (result.Status != "Matched") return;

        if (_pollTicketCoroutine != null) StopCoroutine(_pollTicketCoroutine);
        
        statusText.text = "マッチ成立！";
        _matchedTicketResult = result;

        if (_isHost)
        {
            // ★★★ UtpTransportのメソッドを使ってホストを開始 ★★★
            StartHostWithUtpRelay();
        }
        else
        {
            // ★★★ UtpTransportのメソッドを使ってクライアントを開始 ★★★
            StartCoroutine(ConnectAsClientWithUtpRelay());
        }
    }


    // ★★★ ホストのリレー確保処理を、UtpTransportの作法に合わせて書き換え ★★★
    private void StartHostWithUtpRelay()
    {
        statusText.text = "リレーサーバーを確保中...";
        // 1はホスト以外のプレイヤー数。リージョンはnullで自動選択
        _utpTransport.AllocateRelayServer(1, null, 
            (joinCode) => {
                // 成功コールバック
                Debug.Log($"Host: Relay Join Code is {joinCode}");
                statusText.text = "Join Codeを共有中...";
                NetworkManager.singleton.StartHost();
                UpdatePlayerDataWithJoinCode(joinCode);
            },
            () => {
                // 失敗コールバック
                OnError(new PlayFabError { ErrorMessage = "Failed to allocate Relay server." });
            }
        );
    }

    // ★★★ PlayerDataにJoinCodeを書き込む処理 ★★★
    private void UpdatePlayerDataWithJoinCode(string joinCode)
    {
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { JOIN_CODE_KEY, joinCode } } };
        PlayFabClientAPI.UpdateUserData(request, (res) => Debug.Log("Host: PlayerData updated with Join Code."), OnError);
    }


    // ★★★ クライアントの接続処理を、UtpTransportの作法に合わせて書き換え ★★★
    private IEnumerator ConnectAsClientWithUtpRelay()
    {
        statusText.text = "ホストのJoin Codeを待っています...";
        var hostEntity = _matchedTicketResult.Members.Find(m => m.Entity.Id != PlayFabAuthManager.MyEntity.Id);
        if (hostEntity == null) { /* ... エラー処理 ... */ yield break; }

        string joinCode = null;
        int attempts = 0;
        
        // ホストがJoinCodeを書き込むまでポーリングして待つ
        while (string.IsNullOrEmpty(joinCode) && attempts < 10)
        {
            var request = new GetUserDataRequest { PlayFabId = hostEntity.Entity.Id };
            // ★★★ ここを修正 ★★★
            PlayFabClientAPI.GetUserData(request,
                (result) =>
                {
                    // まず UserDataRecord 型でデータを受け取る
                    if (result.Data != null && result.Data.TryGetValue(JOIN_CODE_KEY, out UserDataRecord dataRecord))
                    {
                        // その中の .Value プロパティが、目的の文字列
                        joinCode = dataRecord.Value;
                    }
                },
                OnError);

            attempts++;
            yield return new WaitForSeconds(2f); // 2秒待機
        }

        if (string.IsNullOrEmpty(joinCode)) { /* ... エラー処理 ... */ yield break; }
        
        statusText.text = $"Join Code [{joinCode}] を使って接続準備中...";

        bool configurationFinished = false;
        bool configurationSucceeded = false;

        _utpTransport.ConfigureClientWithJoinCode(joinCode,
            () => {
                // 成功コールバック
                Debug.Log("Client: Relay configured successfully.");
                configurationSucceeded = true;
                configurationFinished = true;
            },
            () => {
                // 失敗コールバック
                OnError(new PlayFabError { ErrorMessage = "Failed to configure Relay with Join Code." });
                configurationFinished = true;
            }
        );

        // ConfigureClientWithJoinCodeの完了を待つ
        while (!configurationFinished) { yield return null; }

        if (configurationSucceeded)
        {
            // 接続設定が成功した場合のみ、クライアントを開始
            NetworkManager.singleton.StartClient();
        }
    }
    // ★★★ クライアントの接続処理を、UtpTransportのコールバック形式に修正 ★★★
    private IEnumerator ConnectAsClientWithRelay()
    {
        statusText.text = "ホストの接続情報を待っています...";

        var hostEntity = _matchedTicketResult.Members.Find(m => m.Entity.Id != PlayFabAuthManager.MyEntity.Id);
        if (hostEntity == null) { /* ... エラー処理 ... */ yield break; }

        string joinCode = null;
        int attempts = 0;

        // ホストがJoinCodeを書き込むまでポーリングして待つ
        while (string.IsNullOrEmpty(joinCode) && attempts < 10)
        {
            var request = new GetUserDataRequest { PlayFabId = hostEntity.Entity.Id };

            // ★★★ ここを修正 ★★★
            PlayFabClientAPI.GetUserData(request,
                (result) =>
                {
                    // まず UserDataRecord 型でデータを受け取る
                    if (result.Data != null && result.Data.TryGetValue(JOIN_CODE_KEY, out UserDataRecord dataRecord))
                    {
                        // その中の .Value プロパティが、目的の文字列
                        joinCode = dataRecord.Value;
                    }
                },
                OnError);

            attempts++;
            yield return new WaitForSeconds(2f); // 2秒待機
        }

        if (string.IsNullOrEmpty(joinCode)) { /* ... エラー処理 ... */ yield break; }

        statusText.text = $"Join Code [{joinCode}] を使って接続準備中...";

        bool isConfigured = false;
        _utpTransport.ConfigureClientWithJoinCode(joinCode,
            () =>
            {
                // 成功コールバック：設定が完了したらMirrorクライアントを開始
                Debug.Log("Client: Relay configured successfully. Starting client...");
                NetworkManager.singleton.StartClient();
                isConfigured = true;
            },
            () =>
            {
                // 失敗コールバック
                OnError(new PlayFabError { ErrorMessage = "Failed to configure Relay with Join Code." });
                isConfigured = true; // ループを抜けるためにtrueにする
            }
        );

        // isConfiguredフラグがtrueになるのを待つ（ただし、成功・失敗どちらでもtrueになる）
        while (!isConfigured) { yield return null; }
    }

    private void OnError(PlayFabError error)
    {
        statusText.text = "エラーが発生しました";
        if (error != null) Debug.LogError(error.GenerateErrorReport());
    }
}