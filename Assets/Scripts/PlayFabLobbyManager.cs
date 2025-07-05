using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.ClientModels; // このusingを追加
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class PlayFabLobbyManager : MonoBehaviour
{
    public static PlayFabLobbyManager Instance { get; private set; }

    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TextMeshProUGUI roomIdDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;

    private string _currentLobbyConnectionString; // LobbyIdからConnectionStringに変更

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); } else { Destroy(gameObject); }
    }

     // 「ルームを作成」ボタンから呼び出す
    public void CreateLobby()
    {
        statusText.text = "ロビーを作成中...";
        
        var ownerEntity = new PlayFab.MultiplayerModels.EntityKey { Id = PlayFabAuthManager.MyEntity.Id, Type = PlayFabAuthManager.MyEntity.Type };

        var request = new CreateLobbyRequest
        {
            MaxPlayers = 2,
            AccessPolicy = AccessPolicy.Public,
            Owner = ownerEntity,
            // ★★★ 修正点1: 最初のメンバーとして自分自身を追加 ★★★
            Members = new List<Member>
            {
                new Member { MemberEntity = ownerEntity }
            }
        };
        
        PlayFabMultiplayerAPI.CreateLobby(request, OnCreateLobbySuccess, OnError);
    }

    // ロビー作成成功時のコールバック
    private void OnCreateLobbySuccess(CreateLobbyResult result)
    {
        _currentLobbyConnectionString = result.ConnectionString;
        statusText.text = "対戦相手を待っています...";
        // ★★★ 修正点2: 表示するテキストを分かりやすくする ★★★
        roomIdDisplayText.text = "ルームID (コピーして相手に送ってください):\n" + _currentLobbyConnectionString; 
        
        MyNetworkManager.singleton.StartHost();
    }

    // 「ルームに参加」ボタンから呼び出す
    public void JoinLobby()
    {
        // ★★★ 修正点3: 入力された文字列をそのまま使う ★★★
        string connectionStringToJoin = roomIdInput.text;
        if (string.IsNullOrEmpty(connectionStringToJoin)) 
        {
            statusText.text = "ルームIDを入力してください";
            return;
        }
        
        statusText.text = "ロビーに参加中...";

        var memberEntity = new PlayFab.MultiplayerModels.EntityKey { Id = PlayFabAuthManager.MyEntity.Id, Type = PlayFabAuthManager.MyEntity.Type };

        var request = new JoinLobbyRequest
        {
            ConnectionString = connectionStringToJoin,
            MemberEntity = memberEntity
        };
        
        PlayFabMultiplayerAPI.JoinLobby(request, OnJoinLobbySuccess, OnError);
    }


    // ロビー参加成功時のコールバック
    private void OnJoinLobbySuccess(JoinLobbyResult result)
    {
        statusText.text = "接続中...";
        
        // 開発中のローカルテストでは、アドレスはlocalhostのまま
        MyNetworkManager.singleton.networkAddress = "localhost";
        MyNetworkManager.singleton.StartClient();
    }

    private void OnError(PlayFabError error)
    {
        statusText.text = "エラーが発生しました";
        Debug.LogError(error.GenerateErrorReport());
    }
}