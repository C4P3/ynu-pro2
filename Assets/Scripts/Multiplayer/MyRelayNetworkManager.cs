// Multiplayer/MyNetworkManager.cs
using Mirror;
using UnityEngine;
using Utp;

public class MyRelayNetworkManager : RelayNetworkManager
{
    /// <summary>
    /// サーバーに新しいクライアントが接続し、プレイヤーが生成された時に呼び出される
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // 接続したプレイヤーのGameObjectからNetworkPlayerInputコンポーネントを取得
        NetworkPlayerInput player = conn.identity.GetComponent<NetworkPlayerInput>();
        if (player != null)
        {
            // numPlayersは現在接続しているプレイヤーの数（1から始まる）
            // この番号をプレイヤーのインデックスとして設定する
            player.playerIndex = numPlayers;
        }
        Debug.Log(numPlayers);
        // ★★★ プレイヤーが2人になったらゲーム開始のRPCを呼び出す ★★★
        if (numPlayers == 2)
        {
            // 0.5秒待ってからゲーム開始シーケンスを呼び出す
            // クライアント側でプレイヤーの準備が整うのを待つため
            Invoke(nameof(StartGame), 0.5f);
        }
    }

    private void StartGame()
    {
        // ★★★ GameDataSyncの開始シーケンスを呼び出す ★★★
        if (GameDataSync.Instance != null)
        {
            GameDataSync.Instance.StartGameSequence();
        }
        else
        {
            Debug.LogError("GameDataSync.Instance is null. Cannot start game sequence.");
        }
    }

}