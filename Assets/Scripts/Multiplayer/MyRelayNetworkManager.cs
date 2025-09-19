// Multiplayer/MyNetworkManager.cs
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utp;

public class MyRelayNetworkManager : RelayNetworkManager
{
    [Header("Singleton Prefabs")]
    [Tooltip("サーバー起動時に自動で生成されるシングルトンオブジェクト")]
    [SerializeField] private GameObject gameDataSyncPrefab;
    public static MyRelayNetworkManager Instance { get; private set; }

    public override void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        base.Awake();
    }

    /// <summary>
    /// サーバーが正常に起動したときに呼び出される
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // GameDataSyncシングルトンを生成する
        if (gameDataSyncPrefab != null)
        {
            // 既にインスタンスが存在しないか念のため確認
            if (GameDataSync.Instance == null)
            {
                Debug.Log("Spawning GameDataSync singleton on server start.");
                GameObject singletonInstance = Instantiate(gameDataSyncPrefab);
                NetworkServer.Spawn(singletonInstance);
            }
        }
    }


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
        // プレイヤーが2人になったらゲーム開始のRPCを呼び出す
        if (numPlayers == 2)
        {
            // 0.5秒待ってからゲーム開始シーケンスを呼び出す
            // クライアント側でプレイヤーの準備が整うのを待つため
            Invoke(nameof(StartGame), 0.5f);
        }
    }

    private void StartGame()
    {
        // GameDataSyncの開始シーケンスを呼び出す
        if (GameDataSync.Instance != null)
        {
            GameDataSync.Instance.StartGameSequence();
        }
        else
        {
            Debug.LogError("GameDataSync.Instance is null. Cannot start game sequence.");
        }
    }

    /// <summary>
    /// サーバーからクライアントが切断されたときにサーバー側で呼び出される
    /// </summary>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("A client disconnected from the server.");
        // 他のプレイヤーに通知するなどの処理をここに追加できる
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// サーバーへの接続が切断されたときにクライアント側で呼び出される
    /// </summary>
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Disconnected from server. Returning to StartScene.");
        
        // Time.timeScaleを元に戻す
        Time.timeScale = 1f;
        
        // シンプルにStartSceneをロードするだけ
        SceneManager.LoadScene("StartScene");
    }
}