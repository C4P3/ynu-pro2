using UnityEngine;
using Mirror;

/// <summary>
/// マルチプレイ時にローカルプレイヤーの入力を検知し、サーバーにコマンドを送信するクラス
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class NetworkPlayerInput : NetworkBehaviour
{
    private PlayerController _playerController;

    void Awake()
    {
        // 自身がアタッチされているGameObjectのPlayerControllerを取得
        _playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        // この処理は全プレイヤー（ホスト、クライアント）で実行される
        if (LevelManager.Instance != null)
        {
            _playerController.blockTilemap = LevelManager.Instance.blockTilemap;
            _playerController.itemTilemap = LevelManager.Instance.itemTilemap;
        }
        else
        {
            Debug.LogError("LevelManagerが見つかりません！");
        }
        
        var typingManager = Object.FindFirstObjectByType<TypingManager>();
        if (typingManager != null)
        {
            _playerController.typingManager = typingManager;
        }
        else
        {
             Debug.LogError("TypingManagerが見つかりません！");
        }

        // ★★★ 参照設定が終わった直後に、プレイヤーの初期化処理を呼び出す ★★★
        _playerController.Initialize();
    }

    // ★★★ ここから追加 ★★★
    public override void OnStartServer()
    {
        // このメソッドはサーバー上でプレイヤーが生成された時に一度だけ呼ばれる
        // PlayerControllerが必要とする参照を、実行時に探して設定する

        // LevelManagerはシングルトンなので、Instanceから簡単にアクセスできる
        if (LevelManager.Instance != null)
        {
            // LevelManagerが持っているTilemapへの参照を、PlayerControllerに渡す
            _playerController.blockTilemap = LevelManager.Instance.blockTilemap;
            _playerController.itemTilemap = LevelManager.Instance.itemTilemap;
        }
        else
        {
            Debug.LogError("LevelManagerが見つかりません！ MultiPlaySceneに配置されていますか？");
        }

        // PlayerControllerはTypingManagerも必要とするので、同様に設定する
        var typingManager = Object.FindFirstObjectByType<TypingManager>();
        if (typingManager != null)
        {
            _playerController.typingManager = typingManager;
        }
        else
        {
            Debug.LogError("TypingManagerが見つかりません！ MultiPlaySceneに配置されていますか？");
        }
    }
    // ★★★ ここまで追加 ★★★


    public override void OnStartLocalPlayer()
    {
    }

    void Update()
    {
        // ローカルプレイヤーでなければ、入力処理は行わない
        if (!isLocalPlayer) return;
        
        // Roaming状態のときだけ入力を受け付ける
        // （PlayerControllerの内部状態を直接参照するのは設計上あまり良くないが、
        //  不要なコマンド送信を防ぐための最適化として許容範囲）
        // if (_playerController.CurrentState != PlayerState.Roaming) return;


        // Shiftキーが押されているとき、WASD入力を検知する
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int moveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) moveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) moveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) moveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) moveVec = Vector3Int.right;

            if (moveVec != Vector3Int.zero)
            {
                // ★★★ ここが重要 ★★★
                // 入力を検知したら、サーバーにコマンドを送信する
                CmdSendMoveInput(moveVec);
            }
        }
    }

    // --- サーバーへのコマンド送信 ---
    
    /// <summary>
    /// [Command]属性: ローカルプレイヤーからサーバーへメッセージを送信する
    /// 移動入力をサーバーに伝える
    /// </summary>
    [Command]
    private void CmdSendMoveInput(Vector3Int moveVec)
    {
        // サーバー側で受け取ったコマンドを、全てのクライアントに伝える
        RpcReceiveMoveInput(moveVec);
    }
    
    // --- 全クライアントへのRPC ---

    /// <summary>
    /// [ClientRpc]属性: サーバーから全てのクライアントへメッセージを送信する
    /// 全てのクライアントでPlayerControllerの移動処理を呼び出す
    /// </summary>
    [ClientRpc]
    private void RpcReceiveMoveInput(Vector3Int moveVec)
    {
        // 全てのクライアント（自分自身も含む）で、PlayerControllerのメソッドを呼び出す
        _playerController.OnMoveInput(moveVec);
    }
}