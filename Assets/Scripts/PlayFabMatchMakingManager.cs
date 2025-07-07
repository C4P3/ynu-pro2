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

    [SerializeField] private RelayNetworkManager relayManager;

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

    // 「ホストになる」ボタンから呼び出す
    public void CreateRoom()
    {
        statusText.text = "ホストを作成中...";
        // RelayNetworkManagerのホスト開始メソッドを呼び出す
        relayManager.StartRelayHost(1);
        // Join Codeが表示されるのを待つコルーチンを開始
        StartCoroutine(ShowJoinCodeCoroutine());
    }
    
    // Join Codeが生成されるのを待ってUIに表示する
    private IEnumerator ShowJoinCodeCoroutine()
    {
        // relayJoinCodeが空でなくなるまで毎フレーム待つ
        while (string.IsNullOrEmpty(relayManager.relayJoinCode))
        {
            yield return null;
        }
        roomIdDisplayText.text = relayManager.relayJoinCode;
        statusText.text = "コードを相手に伝えてください";
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
        // RelayNetworkManagerに参加用コードを設定し、参加メソッドを呼び出す
        relayManager.relayJoinCode = joinCode;
        relayManager.JoinRelayServer();
    }
    // public void CreateRoom()
    // {
    //     _isHost = true;
    //     statusText.text = "ルームを作成中...";
    //     string shortId = Random.Range(100000, 999999).ToString();
    //     roomIdDisplayText.text = $"ルームID: {shortId}";

    //     // ★★★ 新しいチケット作成処理を呼び出す ★★★
    //     CancelTicketIfExistsAndCreateNew(shortId);
    // }

    // public void JoinRoom()
    // {
    //     _isHost = false;
    //     string shortIdToJoin = roomIdInput.text;
    //     if (string.IsNullOrEmpty(shortIdToJoin)) return;
    //     statusText.text = $"ルームID '{shortIdToJoin}' で参加します...";

    //     // ★★★ 新しいチケット作成処理を呼び出す ★★★
    //     CancelTicketIfExistsAndCreateNew(shortIdToJoin);
    // }

    // ★★★ 古いチケットがあればキャンセルし、なければ新しいチケットを作成するメソッド ★★★
    // private void CancelTicketIfExistsAndCreateNew(string roomId)
    // {
    //     // もし古いチケットIDが残っていれば
    //     if (!string.IsNullOrEmpty(_myTicketId))
    //     {
    //         statusText.text = "古いチケットをキャンセル中...";
    //         var request = new CancelMatchmakingTicketRequest
    //         {
    //             QueueName = "PrivateRoomQueue",
    //             TicketId = _myTicketId
    //         };
    //         PlayFabMultiplayerAPI.CancelMatchmakingTicket(request,
    //             (result) =>
    //             {
    //                 // キャンセル成功後、新しいチケットを作成
    //                 Debug.Log("Old ticket cancelled successfully.");
    //                 UpdateUserDataAndCreateTicket(roomId);
    //             },
    //             (error) =>
    //             {
    //                 // キャンセルに失敗した場合でも、とりあえず新しいチケット作成を試みる
    //                 Debug.LogWarning("Old ticket cancellation failed. It might have already expired. Trying to create a new ticket anyway.");
    //                 UpdateUserDataAndCreateTicket(roomId);
    //             }
    //         );
    //     }
    //     else
    //     {
    //         // 古いチケットがない場合は、直接新しいチケットを作成
    //         UpdateUserDataAndCreateTicket(roomId);
    //     }
    // }

    // private void UpdateUserDataAndCreateTicket(string roomId)
    // {
    //     var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { ROOM_ID_KEY, roomId } } };
    //     PlayFabClientAPI.UpdateUserData(request, (result) => { OnUserDataUpdated_CreateTicket(roomId); }, OnError);
    // }

    // private void OnUserDataUpdated_CreateTicket(string roomId)
    // {
    //     statusText.text = "対戦相手を探しています...";
    //     var matchmakingPlayer = new MatchmakingPlayer
    //     {
    //         Entity = new PlayFab.MultiplayerModels.EntityKey { Id = PlayFabAuthManager.MyEntity.Id, Type = PlayFabAuthManager.MyEntity.Type },
    //         Attributes = new MatchmakingPlayerAttributes { DataObject = new { CurrentRoomId = roomId } }
    //     };
    //     var ticketRequest = new CreateMatchmakingTicketRequest
    //     {
    //         Creator = matchmakingPlayer,
    //         GiveUpAfterSeconds = 120,
    //         QueueName = "PrivateRoomQueue"
    //     };
    //     // ▼▼▼ デバッグログを追加 ▼▼▼
    //     Debug.Log($"--- チケット作成リクエスト ---");
    //     Debug.Log($"送信するキュー名: '{ticketRequest.QueueName}'");
    //     Debug.Log($"送信するルームID: '{roomId}'");
    //     Debug.Log($"送信者のEntity ID: '{matchmakingPlayer.Entity.Id}'");
    //     // ▲▲▲ ここまで ▲▲▲

    //     PlayFabMultiplayerAPI.CreateMatchmakingTicket(ticketRequest, OnTicketCreated, OnError);
    // }

    // private void OnTicketCreated(CreateMatchmakingTicketResult result)
    // {
    //     _myTicketId = result.TicketId;
    //     _pollTicketCoroutine = StartCoroutine(PollTicketStatusCoroutine());
    // }

    // private IEnumerator PollTicketStatusCoroutine()
    // {
    //     while (true)
    //     {
    //         PlayFabMultiplayerAPI.GetMatchmakingTicket(
    //             new GetMatchmakingTicketRequest { TicketId = _myTicketId, QueueName = "PrivateRoomQueue" },
    //             OnGetTicketStatusSuccess, OnError);
    //         yield return new WaitForSeconds(6f);
    //     }
    // }

    // // チケット状態取得成功時のコールバック
    // private void OnGetTicketStatusSuccess(GetMatchmakingTicketResult result)
    // {
    //     if (result.Status != "Matched") return;
    //     if (_pollTicketCoroutine != null) StopCoroutine(_pollTicketCoroutine);
        
    //     statusText.text = "マッチ成立！";
    //     result.Members.ForEach((member) =>
    //     {
    //         Debug.Log(member.Entity.Id);
    //     });
    //     _matchedTicketResult = result;

    //     if (_isHost) { StartHostWithUtpRelay(); }
    //     else { StartCoroutine(ConnectAsClientWithUtpRelay()); }
    // }


    // // ★★★ ホストのリレー確保処理を、UtpTransportの作法に合わせて書き換え ★★★
    // private void StartHostWithUtpRelay()
    // {
    //     statusText.text = "リレーサーバーを確保中...";
    //     _utpTransport.AllocateRelayServer(1, null, 
    //         (joinCode) => {
    //             //【ログポイント1】ホストがJoin Codeを取得
    //             Debug.Log($"[Host] 1. Relay Join Code Acquired: {joinCode}");
    //             statusText.text = "Join Codeを共有中...";
    //             NetworkManager.singleton.StartHost();
    //             UpdatePlayerDataWithJoinCode(joinCode);
    //         },
    //         () => { OnError(new PlayFabError { ErrorMessage = "Failed to allocate Relay server." }); }
    //     );
    // }

    // // ★★★ PlayerDataにJoinCodeを書き込む処理 ★★★
    // private void UpdatePlayerDataWithJoinCode(string joinCode)
    // {
    //     var request = new UpdateUserDataRequest
    //     {
    //         Data = new Dictionary<string, string> { { JOIN_CODE_KEY, joinCode } },
    //         Permission = UserDataPermission.Public
    //     };
    //     //【ログポイント2】ホストがJoin Codeを書き込み試行
    //     Debug.Log($"[Host] 2. Attempting to write Join Code to PlayerData...");
    //     PlayFabClientAPI.UpdateUserData(request, 
    //         (res) => { Debug.Log("[Host] 2a. PlayerData updated with Join Code SUCCESSFULLY."); }, 
    //         (err) => { Debug.LogError("[Host] 2b. FAILED to write Join Code to PlayerData."); OnError(err); });
    // }


    // private IEnumerator ConnectAsClientWithUtpRelay()
    // {
    //     statusText.text = "ホストの接続情報を待っています...";
    //     _matchedTicketResult.Members.ForEach((Member) =>
    //     {
    //         Debug.Log(Member.Entity.Id);
    //     });
    //     var hostEntity = _matchedTicketResult.Members.Find(m => m.Entity.Id != PlayFabAuthManager.MyEntity.Id);
    //     if (hostEntity == null) { OnError(new PlayFabError{ ErrorMessage = "Could not find Host in match." }); yield break; }

    //     string joinCode = null;
    //     int attempts = 0;
        
    //     while (string.IsNullOrEmpty(joinCode) && attempts < 10)
    //     {
    //         //【ログポイント3】クライアントがJoin Codeを読み取り試行
    //         Debug.Log($"[Client] 3. Polling for Join Code... (Attempt {attempts + 1})");
    //         var request = new GetUserDataRequest { PlayFabId = hostEntity.Entity.Id };
    //         PlayFabClientAPI.GetUserData(request, 
    //             (result) => {
    //                 if (result.Data != null && result.Data.TryGetValue(JOIN_CODE_KEY, out UserDataRecord dataRecord))
    //                 {
    //                     joinCode = dataRecord.Value;
    //                     Debug.Log($"[Client] 3a. Found Join Code: {joinCode}");
    //                 } else {
    //                     Debug.Log("[Client] 3b. GetUserData success, but Join Code not found yet.");
    //                 }
    //             }, 
    //             OnError);

    //         attempts++;
    //         yield return new WaitForSeconds(2f);
    //     }

    //     if (string.IsNullOrEmpty(joinCode)) { OnError(new PlayFabError { ErrorMessage = "Host did not provide a Join Code in time."}); yield break; }
        
    //     statusText.text = $"Join Code [{joinCode}] を使って接続準備中...";

    //     bool configurationFinished = false;
    //     bool configurationSucceeded = false;

    //     //【ログポイント4】クライアントがJoin Codeを使って接続設定
    //     Debug.Log($"[Client] 4. Attempting to configure Relay with Join Code...");
    //     _utpTransport.ConfigureClientWithJoinCode(joinCode,
    //         () => {
    //             Debug.Log("[Client] 4a. Relay configured SUCCESSFULLY.");
    //             configurationSucceeded = true;
    //             configurationFinished = true;
    //         },
    //         () => {
    //             Debug.LogError("[Client] 4b. FAILED to configure Relay with Join Code.");
    //             configurationFinished = true;
    //         }
    //     );

    //     while(!configurationFinished) { yield return null; }

    //     if (configurationSucceeded)
    //     {
    //         NetworkManager.singleton.StartClient();
    //     }
    // }

    // private void OnError(PlayFabError error)
    // {
    //     statusText.text = "エラーが発生しました";
    //     if (error != null) Debug.LogError($"!!! PLAYFAB ERROR on {(_isHost ? "HOST" : "CLIENT")}!!!: " + error.GenerateErrorReport());
    // }
}