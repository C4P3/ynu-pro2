// Multiplayer/MyNetworkManager.cs
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
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

        // ★★★ プレイヤーが2人になったらゲーム開始のRPCを呼び出す ★★★
        if (numPlayers == 2)
        {
            // ★★★ GameDataSyncの開始シーケンスを呼び出す ★★★
            GameDataSync.Instance.StartGameSequence();
        }
    }

}