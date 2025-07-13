using UnityEngine;
using UnityEngine.Tilemaps;
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
        // GameManagerMultiから参照を取���
        GameManagerMulti gm = GameManagerMulti.Instance;
        if (gm == null)
        {
            Debug.LogError("GameManagerMulti instance not found!");
            return;
        }

        // --- LevelManagerの取得 ---
        LevelManager levelManager = (playerIndex == 1) ? gm.levelManagerP1 : gm.levelManagerP2;
        
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
            Debug.LogError($"Player {playerIndex} のLevelManagerが見つかりません！ GameManagerMultiのインスペクターで設定されているか確認してください。");
        }

        // --- TypingPanelの設定 ---
        GameObject typingPanelObject = (playerIndex == 1) ? gm.typingPanelP1 : gm.typingPanelP2;
        if (typingPanelObject != null)
        {
            _typingManager.typingPanel = typingPanelObject;
        }
        else
        {
            Debug.LogError($"Player {playerIndex} のTypingPanelが見つかりません！ GameManagerMultiのインスペクターで設定されているか確認してください。");
        }
        _typingManager.Initialize();

        _playerController.Initialize();

        // --- レイヤーとカメラの設定 ---
        switch (playerIndex)
        {
            case 1:
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player1"));
                var vcam1 = gm.vcamP1;
                if (vcam1 != null) vcam1.Follow = transform;
                else Debug.LogError("VCam1がGameManagerMultiに設定されていません。");
                break;
            case 2:
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player2"));
                var vcam2 = gm.vcamP2;
                if (vcam2 != null) vcam2.Follow = transform;
                else Debug.LogError("VCam2がGameManagerMultiに設定されていません。");
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

        // サーバーに準備完了を通知
        CmdPlayerReady();
    }

    [Command]
    void CmdPlayerReady()
    {
        Debug.Log($"Player {playerIndex} is ready.");
        GameDataSync.Instance.PlayerReady();
    }

    void Update()
    {
        // ★★★ GameDataSyncが利用可能かチェック ★★★
        if (GameDataSync.Instance == null) return;

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
        // サーバー側でLevelManagerの破壊処理を呼び出す
        if (_playerController != null && _playerController.levelManager != null)
        {
            _playerController.levelManager.DestroyConnectedBlocks(gridPos, this);
        }
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
    // ★★★ ブロック破壊用のClientRpcは不要になったので削除 ★★★

    [Command]
    public void CmdAcquireItem(Vector3Int itemPosition)
    {
        TileBase itemTile = _playerController.itemTilemap.GetTile(itemPosition);
        if (itemTile != null && ItemManager.Instance != null)
        {
            // サーバー側でアイテム処理を実行
            ItemManager.Instance.ServerHandleItemAcquisition(itemTile, itemPosition, _playerController);
        }
    }

    [ClientRpc]
    public void RpcPlayItemAcquisitionEffects(int itemID, Vector3Int itemPosition)
    {
        ItemData itemData = ItemManager.Instance.GetItemDataByID(itemID);
        if (itemData != null)
        {
            ItemManager.Instance.PlayItemAcquisitionEffectsOnClient(itemData, itemPosition, _playerController);
        }
    }

    [ClientRpc]
    public void RpcStunPlayer(float duration)
    {
        // このRPCを受け取ったクライアントのプレイヤーをスタンさせる
        _playerController.Stun(duration);
    }

    [ClientRpc]
    public void RpcRemoveTile(Vector3Int position, bool isBlock)
    {
        if (_playerController == null || _playerController.levelManager == null) return;

        if (isBlock)
        {
            _playerController.levelManager.blockTilemap.SetTile(position, null);
        }
        else
        {
            _playerController.levelManager.itemTilemap.SetTile(position, null);
        }
    }

    [ClientRpc]
    public void RpcPlayDebuffEffect(ItemEffectType effectType)
    {
        ItemData itemData = ItemManager.Instance.GetItemDataByType(effectType);
        if (itemData != null && EffectManager.Instance != null)
        {
            // 追従エフェクトを再生
            if (itemData.followEffectPrefab != null)
            {
                EffectManager.Instance.PlayFollowEffect(itemData.followEffectPrefab, itemData.followEffectDuration, _playerController.transform, _playerController.gameObject);
            }
            // 通常のエフェクトを再生
            else if (itemData.acquisitionEffectPrefab != null)
            {
                EffectManager.Instance.PlayItemAcquisitionEffect(itemData, _playerController.levelManager.itemTilemap.WorldToCell(_playerController.transform.position), _playerController.levelManager.itemTilemap, _playerController.gameObject);
            }
        }
    }

    [ClientRpc]
    public void RpcPlaceUnchiTile()
    {
        var unchiData = ItemManager.Instance.GetItemDataByType(ItemEffectType.Unchi) as UnchiItemData;
        if (unchiData != null && _playerController != null && _playerController.levelManager != null)
        {
            Vector3Int playerGridCenter = _playerController.levelManager.itemTilemap.WorldToCell(_playerController.transform.position);
            unchiData.Activate(playerGridCenter, _playerController.levelManager.blockTilemap, _playerController.levelManager.itemTilemap);
        }
    }

    [ClientRpc]
    public void RpcPlayRocketEffect(Vector3 startPosition, Vector3Int direction, GameObject targetPlayer)
    {
        // ItemManagerからロケットのデータを取得してエフェクトを再生
        var rocketData = ItemManager.Instance.GetItemDataByType(ItemEffectType.Rocket) as RocketItemData;
        if (rocketData != null && EffectManager.Instance != null && rocketData.beamEffectPrefab != null)
        {
            EffectManager.Instance.PlayDirectionalEffect(rocketData.beamEffectPrefab, startPosition, direction, targetPlayer);
        }
    }
}