using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.Json; // PlayFabSimpleJsonのために必要
using PlayFab.ClientModels; // ClientModels.EntityKeyのために必要
using PlayFab.MultiplayerModels;
using PlayFab.GroupsModels;
using PlayFab.DataModels;
using System.Linq;

public class PlayFabMatchmakingManager : MonoBehaviour
{
    public static PlayFabMatchmakingManager Instance { get; private set; }

    private string ticketId;
    private Coroutine _pollTicketCoroutine;

    // マッチングした相手の情報を保持
    private string hostEntityId;
    private PlayFab.MultiplayerModels.EntityKey hostMultiplayerEntityKey;
    private PlayFab.MultiplayerModels.EntityKey clientMultiplayerEntityKey;
    private string currentMatchId;

    // ### このスクリプト内で使用するヘルパークラス ###
    [System.Serializable]
    private class JoinCodeData
    {
        public string JoinCode;
    }


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
    // PlayFabのキューとグループを利用した自動マッチング

    /// <summary>
    /// ランダムマッチングを開始するエントリーポイント
    /// </summary>
    public void StartRandomMatchmaking()
    {
        Debug.Log("対戦相手を探しています...");
        var entityKey = new PlayFab.MultiplayerModels.EntityKey
        {
            Id = PlayFabSettings.staticPlayer.EntityId,
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
        _pollTicketCoroutine = StartCoroutine(PollTicketStatusCoroutine(result.TicketId, "1vs1RandomMatch"));
    }

    private IEnumerator PollTicketStatusCoroutine(string tId, string queueName)
    {
        while (true)
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest { TicketId = tId, QueueName = queueName },
                OnGetTicketStatusSuccess,
                OnPlayFabError
            );
            yield return new WaitForSeconds(6f);
        }
    }

    private void OnGetTicketStatusSuccess(GetMatchmakingTicketResult result)
    {
        switch (result.Status)
        {
            case "Matched":
                StopCoroutine(_pollTicketCoroutine);
                _pollTicketCoroutine = null;
                Debug.Log($"マッチング成功！ MatchId: {result.MatchId}");
                GetMatchDetails(result.MatchId, result.QueueName);
                break;
            case "Canceled":
                StopCoroutine(_pollTicketCoroutine);
                _pollTicketCoroutine = null;
                Debug.LogWarning("チケットはキャンセルされました。");
                break;
            default:
                Debug.Log("対戦相手を待っています...");
                break;
        }
    }

    private void GetMatchDetails(string matchId, string queueName)
    {
        PlayFabMultiplayerAPI.GetMatch(
            new GetMatchRequest { MatchId = matchId, QueueName = queueName },
            OnGetMatchSuccess,
            OnPlayFabError
        );
    }

    private void OnGetMatchSuccess(GetMatchResult result)
    {
        currentMatchId = result.MatchId;

        // メンバーリストの0番目をホストとする
        if (result.Members[0].Entity.Id == PlayFabSettings.staticPlayer.EntityId)
        {
            Debug.Log("自分がホストです。");
            StartHostFlow(result.MatchId, result.Members);
        }
        else
        {
            Debug.Log("自分はクライアントです。");
            StartClientFlow(result.Members);
        }
    }

    // ===============================================================
    // 前回までのマッチングキューなどのごみの処理
    // ===============================================================
    public void CancelActiveMatchmakingTickets()
    {
        var myEntityKey = new PlayFab.MultiplayerModels.EntityKey { Id = PlayFabSettings.staticPlayer.EntityId, Type = PlayFabSettings.staticPlayer.EntityType };
        
        // "1vs1RandomMatch"キューに参加しているチケットを検索
        var request = new ListMatchmakingTicketsForPlayerRequest
        {
            Entity = myEntityKey,
            QueueName = "1vs1RandomMatch"
        };

        PlayFabMultiplayerAPI.ListMatchmakingTicketsForPlayer(request,
            (result) =>
            {
                // 見つかったチケットを全てキャンセル
                foreach (var ticketId in result.TicketIds)
                {
                    Debug.Log($"古いマッチングチケットを発見: {ticketId}。キャンセルします。");
                    PlayFabMultiplayerAPI.CancelMatchmakingTicket(
                        new CancelMatchmakingTicketRequest { TicketId = ticketId, QueueName = "1vs1RandomMatch" },
                        (cancelResult) => {},
                        (error) => {} // エラーでも次の処理に進む
                    );
                }
                
                // チケットの処理が終わったら、グループのクリーンアップに進む
                LeaveOrDeleteOldGroups();
            },
            (error) =>
            {
                Debug.LogWarning("チケットの検索中にエラーが発生しましたが、処理を続行します。");
                // エラーが発生しても、グループのチェックは試みる
                LeaveOrDeleteOldGroups();
            }
        );
    }
    /// <summary>
    /// プレイヤーが所属している古いマッチング用グループから脱退、またはグループを削除する
    /// </summary>
    private void LeaveOrDeleteOldGroups()
    {
        // 自分が所属しているグループの一覧を取得
        PlayFabGroupsAPI.ListMembership(new ListMembershipRequest(), 
            (result) =>
            {
                foreach (var groupInfo in result.Groups)
                {
                    // "Match-"で始まる名前のグループのみを対象とする
                    if (groupInfo.GroupName.StartsWith("Match-"))
                    {
                        Debug.Log($"古いグループを発見: {groupInfo.GroupName}。処理します。");

                        // さらに、そのグループのメンバーリストを取得
                        PlayFabGroupsAPI.ListGroupMembers(new ListGroupMembersRequest { Group = groupInfo.Group }, 
                            (membersResult) => 
                            {
                                // もしグループのメンバーが自分一人だけなら、グループごと削除する
                                if (membersResult.Members.Count == 1 && membersResult.Members[0].Key.Id == PlayFabSettings.staticPlayer.EntityId)
                                {
                                    Debug.Log("自分が最後のメンバーなので、グループを削除します。");
                                    PlayFabGroupsAPI.DeleteGroup(new DeleteGroupRequest { Group = groupInfo.Group }, null, null);
                                }
                                // 他にメンバーがいる場合は、自分だけがグループから脱退する
                                else
                                {
                                    Debug.Log("グループから脱退します。");
                                    var myEntityKey = new PlayFab.GroupsModels.EntityKey { Id = PlayFabSettings.staticPlayer.EntityId, Type = PlayFabSettings.staticPlayer.EntityType };
                                    PlayFabGroupsAPI.RemoveMembers(
                                        new RemoveMembersRequest 
                                        { 
                                            Group = groupInfo.Group, 
                                            Members = new List<PlayFab.GroupsModels.EntityKey> { myEntityKey } 
                                        }, 
                                        null, null
                                    );
                                }
                            }, 
                            null
                        );
                    }
                }
                Debug.Log("クリーンアップ処理が完了しました。");
            },
            OnPlayFabError
        );
    }

    // ===============================================================
    // ホスト側の処理
    // ===============================================================
    private void StartHostFlow(string matchId, List<MatchmakingPlayerWithTeamAssignment> members)
    {
        hostMultiplayerEntityKey = members[0].Entity;
        clientMultiplayerEntityKey = members[1].Entity;

        var createGroupRequest = new CreateGroupRequest { GroupName = $"Match-{matchId}" };
        PlayFabGroupsAPI.CreateGroup(createGroupRequest, OnCreateGroupSuccess, OnPlayFabError);
    }

    private void OnCreateGroupSuccess(CreateGroupResponse response)
    {
        var groupEntityKey = response.Group;
        var inviteRequest = new InviteToGroupRequest
        {
            Group = groupEntityKey,
            Entity = new PlayFab.GroupsModels.EntityKey { Id = clientMultiplayerEntityKey.Id, Type = clientMultiplayerEntityKey.Type }
        };
        PlayFabGroupsAPI.InviteToGroup(inviteRequest, (inviteResponse) =>
        {
            Debug.Log("クライアントをグループに招待しました。");
            StartCoroutine(HostRelayAndShareJoinCode(groupEntityKey));
        }, OnPlayFabError);
    }

    private IEnumerator HostRelayAndShareJoinCode(PlayFab.GroupsModels.EntityKey groupEntityKey)
    {
        MyRelayNetworkManager.Instance.StartRelayHost(1);

        while (string.IsNullOrEmpty(MyRelayNetworkManager.Instance.relayJoinCode))
        {
            yield return null; // 毎フレーム待機
        }

        string joinCode = MyRelayNetworkManager.Instance.relayJoinCode;
        Debug.Log($"Join Code生成完了: {joinCode}");

        var setObjectsRequest = new SetObjectsRequest
        {
            Entity = new PlayFab.DataModels.EntityKey { Id = groupEntityKey.Id, Type = groupEntityKey.Type },
            Objects = new List<SetObject> {
                new SetObject {
                    ObjectName = "ConnectionInfo",
                    DataObject = new { JoinCode = joinCode }
                }
            }
        };
        PlayFabDataAPI.SetObjects(setObjectsRequest, (res) => Debug.Log("グループにJoinCodeを書き込みました。"), OnPlayFabError);
    }

    // ===============================================================
    // クライアント側の処理
    // ===============================================================
    private void StartClientFlow(List<MatchmakingPlayerWithTeamAssignment> members)
    {
        hostEntityId = members[0].Entity.Id;
        StartCoroutine(PollForGroupInvitation());
    }

    private IEnumerator PollForGroupInvitation()
    {
        float timeout = 20f;
        float elapsedTime = 0f;
        AcceptGroupInvitationRequest acceptedInvite = null;

        while (elapsedTime < timeout && acceptedInvite == null)
        {
            bool apiCallDone = false;
            PlayFabGroupsAPI.ListMembershipOpportunities(new ListMembershipOpportunitiesRequest(), (response) =>
            {
                var invitation = response.Invitations.FirstOrDefault(inv => inv.InvitedByEntity.Key.Id == hostEntityId);
                if (invitation != null)
                {
                    Debug.Log("ホストからグループへの招待を発見！");
                    var myEntityKey = new PlayFab.GroupsModels.EntityKey { Id = PlayFabSettings.staticPlayer.EntityId, Type = PlayFabSettings.staticPlayer.EntityType };
                    acceptedInvite = new AcceptGroupInvitationRequest { Group = invitation.Group, Entity = myEntityKey };
                }
                apiCallDone = true;
            }, (error) => { apiCallDone = true; });

            yield return new WaitUntil(() => apiCallDone);
            if (acceptedInvite == null) { yield return new WaitForSeconds(2f); elapsedTime += 2f; }
        }

        if (acceptedInvite != null)
        {
            PlayFabGroupsAPI.AcceptGroupInvitation(acceptedInvite, OnAcceptInvitationSuccess, OnPlayFabError);
        }
        else { Debug.LogError("招待がタイムアウトしました。"); }
    }

    private void OnAcceptInvitationSuccess(PlayFab.GroupsModels.EmptyResponse response)
    {
        Debug.Log("グループへの参加に成功！");
        PlayFabGroupsAPI.ListMembership(new ListMembershipRequest(), (listResponse) =>
        {

            // ★★★ 手順2: 正しいグループをmatchIdから検索 ★★★
            // "Match-{matchId}" という命名規則を利用します
            var targetGroupName = $"Match-{currentMatchId}";
            var myGroup = listResponse.Groups.FirstOrDefault(g => g.GroupName == targetGroupName);

            if (myGroup != null)
            {
                StartCoroutine(PollForJoinCodeInGroup(myGroup.Group));
            }
            else
            {
                Debug.LogError($"現在のmatchIdに一致するグループ({targetGroupName})が見つかりませんでした。");
            }
        }, OnPlayFabError);
    }

    private IEnumerator PollForJoinCodeInGroup(PlayFab.GroupsModels.EntityKey groupEntityKey)
    {
        float timeout = 20f;
        float elapsedTime = 0f;
        string joinCode = null;

        while (elapsedTime < timeout && string.IsNullOrEmpty(joinCode))
        {
            bool apiCallDone = false;
            var getObjectsRequest = new GetObjectsRequest { Entity = new PlayFab.DataModels.EntityKey { Id = groupEntityKey.Id, Type = groupEntityKey.Type } };
            PlayFabDataAPI.GetObjects(getObjectsRequest, (response) =>
            {
                if (response.Objects.TryGetValue("ConnectionInfo", out var obj))
                {
                    var json = PlayFabSimpleJson.SerializeObject(obj.DataObject);
                    var data = JsonUtility.FromJson<JoinCodeData>(json);
                    joinCode = data.JoinCode;
                }
                apiCallDone = true;
            }, (error) => { apiCallDone = true; });

            yield return new WaitUntil(() => apiCallDone);
            if (string.IsNullOrEmpty(joinCode)) { yield return new WaitForSeconds(1f); elapsedTime += 1f; }
        }

        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log($"JoinCode [{joinCode}] を取得！接続します。");
            MyRelayNetworkManager.Instance.relayJoinCode = joinCode;
            MyRelayNetworkManager.Instance.JoinRelayServer();
        }
        else { Debug.LogError("JoinCodeの取得がタイムアウトしました。"); }
    }
    #endregion

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // ===============================================================
    // 共通エラーハンドラ
    // ===============================================================
    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError("PlayFab API Error: " + error.GenerateErrorReport());
        if (_pollTicketCoroutine != null)
        {
            StopCoroutine(_pollTicketCoroutine);
            _pollTicketCoroutine = null;
        }
        CancelActiveMatchmakingTickets();
    }

    private PlayFab.GroupsModels.EntityKey currentGroupKey;

    // 試合終了時にホストが呼び出す
    public void CleanUpGroupAsHost()
    {
        if (currentGroupKey == null) return;

        var request = new DeleteGroupRequest { Group = currentGroupKey };
        PlayFabGroupsAPI.DeleteGroup(request,
            (res) =>
            {
                Debug.Log("グループを正常に削除しました。");
                currentGroupKey = null; // リセット
            },
            OnPlayFabError
        );
    }
    
    // 試合終了時にクライアントが呼び出す
    public void CleanUpGroupAsClient()
    {
        if (currentGroupKey == null) return;

        // 自分自身のEntityKeyを取得
        var myEntityKey = new PlayFab.GroupsModels.EntityKey { Id = PlayFabSettings.staticPlayer.EntityId, Type = PlayFabSettings.staticPlayer.EntityType };

        var request = new RemoveMembersRequest
        {
            Group = currentGroupKey,
            Members = new List<PlayFab.GroupsModels.EntityKey> { myEntityKey }
        };

        PlayFabGroupsAPI.RemoveMembers(request, 
            (res) => {
                Debug.Log("グループから正常に脱退しました。");
                currentGroupKey = null; // リセット
            },
            OnPlayFabError
        );
    }
}