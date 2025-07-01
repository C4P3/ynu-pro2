using UnityEngine;
using Mirror;

/// <summary>
/// マルチプレイ時にローカルプレイヤーの入力を検知し、サーバーにコマンドを送信するクラス
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(TypingManager))] 
public class NetworkPlayerInput : NetworkBehaviour
{
    [Header("Player Info")]
    [Tooltip("サーバーから割り当てられるプレイヤー番号")]
    [SyncVar] // この変数がサーバーから全クライアントに自動同期される
    public int playerIndex = 0;

    private PlayerController _playerController;
    private TypingManager _typingManager;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _typingManager = GetComponent<TypingManager>(); // 参照を取得

        // ★★★ まずはTypingManagerを無効にしておく ★★★
        _typingManager.enabled = false;
    }

    void Start()
    {
        Debug.Log($"--- Debug Start for {gameObject.name} ---");
        Debug.Log($"My playerIndex is [{playerIndex}]", gameObject);

        // LevelManager と TypingManager を探す処理を、プレイヤー番号に応じて変更
        LevelManager levelManager = null;
        GameObject gridObject = null;

        if (playerIndex == 1)
        {
            gridObject = GameObject.Find("Grid_P1");
            Debug.Log("playerIndex is 1, trying to find Grid_P1...", gameObject);
        }
        else if (playerIndex == 2)
        {
            gridObject = GameObject.Find("Grid_P2");
            Debug.Log("playerIndex is 2, trying to find Grid_P2...", gameObject);
        }

        if (gridObject != null)
        {
            levelManager = gridObject.GetComponent<LevelManager>();
            Debug.Log($"Found GameObject: {gridObject.name}", gameObject);
        }
        else
        {
            Debug.LogError("Could not find Grid GameObject!", gameObject);
        }

        Debug.Log($"Found LevelManager component on: {(levelManager != null ? levelManager.gameObject.name : "NULL")}", gameObject);


        if (levelManager != null)
        {
            // --- デバッグここから ---
            Debug.Log($"Assigning my transform ('{this.transform.name}') to {levelManager.gameObject.name}'s playerTransform.", gameObject);
            // --- デバッグここまで ---

            levelManager.playerTransform = this.transform;

            // ★★★ 最重要チェック ★★★
            Debug.Log($"IMMEDIATELY AFTER ASSIGNMENT, levelManager.playerTransform is: {(levelManager.playerTransform != null ? levelManager.playerTransform.name : "NULL")}", gameObject);

            // 他の参照設定
            _playerController.blockTilemap = levelManager.blockTilemap;
            _playerController.itemTilemap = levelManager.itemTilemap;
            _playerController.levelManager = levelManager;

            // 生成を呼び出し
            levelManager.InitialGenerate();
        }
        else
        {
            Debug.LogError($"Player {playerIndex} のLevelManagerが見つかりません！");
        }

        // ★★★ 対応するTypingPanelをTypingManagerに設定する ★★★
        if (playerIndex == 1)
        {
            _typingManager.typingPanel = GameObject.Find("TypingPanel_P1");
        }
        else if (playerIndex == 2)
        {
            _typingManager.typingPanel = GameObject.Find("TypingPanel_P2");
        }
        
        _playerController.Initialize();

        switch (playerIndex)
        {
            case 1: // 1人目のプレイヤー
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player1"));
                var vcam1 = GameObject.Find("VCam1")?.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam1 != null)
                {
                    vcam1.Follow = transform;
                    vcam1.gameObject.layer = LayerMask.NameToLayer("Player1"); // ★★★ VCam1のレイヤーを設定 ★★★
                }
                break;

            case 2: // 2人目のプレイヤー
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player2"));
                var vcam2 = GameObject.Find("VCam2")?.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam2 != null)
                {
                    vcam2.Follow = transform;
                    vcam2.gameObject.layer = LayerMask.NameToLayer("Player2"); // ★★★ VCam2のレイヤーを設定 ★★★
                }
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