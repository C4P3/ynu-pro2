using UnityEngine;
using Mirror;
using TMPro;

/// <summary>
/// マルチプレイ時にローカルプレイヤーの入力を検知し、サーバーにコマンドを送信するクラス
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(TypingManager))]
public class NetworkPlayerInput : NetworkBehaviour
{
    [Header("Player Info")]
    [Tooltip("サーバーから割り当てられるプレイヤー番号")]
    [SyncVar]
    public int playerIndex = 0;

    // --- Private References ---
    private PlayerController _playerController;
    private TypingManager _typingManager;

    void Awake()
    {
        // --- 参照をまとめて取得 ---
        _playerController = GetComponent<PlayerController>();
        _typingManager = GetComponent<TypingManager>();

        _typingManager.enabled = false; // ローカルプレイヤー以外は無効化
    }

    void Start()
    {
        // --- LevelManagerの検索 ---
        LevelManager levelManager = null;
        GameObject gridObject = GameObject.Find((playerIndex == 1) ? "Grid_P1" : "Grid_P2");
        if (gridObject != null)
        {
            levelManager = gridObject.GetComponent<LevelManager>();
        }
        else
        {
            Debug.LogError($"Could not find Grid GameObject for Player {playerIndex}!", gameObject);
        }

        // --- 参照の受け渡し ---
        if (levelManager != null)
        {
            levelManager.playerTransform = this.transform;
            _playerController.blockTilemap = levelManager.blockTilemap;
            _playerController.itemTilemap = levelManager.itemTilemap;
            _playerController.levelManager = levelManager;
            
            // PlayerControllerとTypingManagerに自身のPlayerIndexを教える
            _playerController.playerIndex = playerIndex;
            _typingManager.playerIndex = playerIndex;
        }
        else
        {
            Debug.LogError($"Player {playerIndex} のLevelManagerが見つかりません！");
        }

        // --- TypingPanelの設定 ---
        GameObject typingPanelObject = GameObject.Find((playerIndex == 1) ? "TypingPanel_P1" : "TypingPanel_P2");
        if (typingPanelObject != null)
        {
            _typingManager.typingPanel = typingPanelObject;
        }
        else
        {
            Debug.LogError($"Player {playerIndex} のTypingPanelが見つかりません！");
        }

        _playerController.Initialize();

        // --- レイヤーとカメラの設定 ---
        switch (playerIndex)
        {
            case 1:
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player1"));
                var vcam1 = GameObject.Find("VCam1")?.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam1 != null) vcam1.Follow = transform;
                break;
            case 2:
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player2"));
                var vcam2 = GameObject.Find("VCam2")?.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam2 != null) vcam2.Follow = transform;
                break;
        }
    }

    // ★★★ 階層下のオブジェクトも含めてレイヤーを再帰的に設定するヘルパー関数 ★★★
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // ★★★ ここから追加 ★★★
    public override void OnStartServer()
    {
    }
    // ★★★ ここまで追加 ★★★


    public override void OnStartLocalPlayer()
    {
        // ★★★ ローカルプレイヤーのTypingManagerだけを有効にする ★★★
        _typingManager.enabled = true;
    }

    void Update()
    {
        // ★★★ ゲームがプレイ中でなければ入力を受け付けない ★★★
        if (GameDataSync.Instance.currentState != GameState.Playing) return;

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

    [Command]
    public void CmdDestroyBlock(Vector3Int gridPos)
    {
        // サーバーが受け取ったら、全クライアントに破壊を命令するRPCを呼び出す
        RpcDestroyBlock(gridPos);
    }

    [Command]
    public void CmdAddDestroyedBlock()
    {
        if (GameManagerMulti.Instance != null)
        {
            GameManagerMulti.Instance.ServerAddDestroyedBlock(playerIndex);
        }
    }

    [Command]
    public void CmdRecoverOxygen(float amount)
    {
        if (GameManagerMulti.Instance != null)
        {
            GameManagerMulti.Instance.ServerRecoverOxygen(playerIndex, amount);
        }
    }

    [Command]
    public void CmdTemporaryOxygenInvincibility(float duration)
    {
        if (GameManagerMulti.Instance != null)
        {
            GameManagerMulti.Instance.StartTemporaryInvincibility(playerIndex, duration);
        }
    }

    [Command]
    public void CmdNotifyMissType()
    {
        if (GameManagerMulti.Instance != null)
        {
            GameManagerMulti.Instance.ServerAddMissType(playerIndex);
        }
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
    // ★★★ ブロック破壊用のClientRpcを追加 ★★★
    [ClientRpc]
    private void RpcDestroyBlock(Vector3Int gridPos)
    {
        // このRPCは、このコンポーネントがアタッチされているプレイヤーオブジェクトに対して実行される
        // そのため、各クライアントで、対応するプレイヤーのLevelManagerが正しく呼ばれる
        if (_playerController != null && _playerController.levelManager != null)
        {
            _playerController.levelManager.DestroyConnectedBlocks(gridPos);
        }
    }
}