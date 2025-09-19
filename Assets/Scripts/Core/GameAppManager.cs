using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class GameAppManager : MonoBehaviour
{
    public static GameAppManager Instance { get; private set; }

    void Awake()
    {
        // シングルトンパターンの設定のみ行う
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void LoadScene(string sceneName)
    {
        // ネットワークがアクティブでなければ何もしない
        if (MyRelayNetworkManager.singleton.isNetworkActive)
        {
            // 自分がホスト（サーバー兼クライアント）の場合
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                Debug.Log("ホストを停止します。");
                MyRelayNetworkManager.singleton.StopHost();
            }
            // 自分がクライアントの場合
            else if (NetworkClient.isConnected)
            {
                Debug.Log("クライアントを停止します。");
                MyRelayNetworkManager.singleton.StopClient();
            }
            // 自分がサーバーのみの場合（ヘッドレスサーバーなど）
            else if (NetworkServer.active)
            {
                Debug.Log("サーバーを停止します。");
                MyRelayNetworkManager.singleton.StopServer();
            }
        }
        SceneManager.LoadScene(sceneName);
    }
}
