// PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの状態を定義する列挙型
/// </summary>
public enum PlayerState
{
    Roaming, // 自由に動ける状態
    Moving,  // グリッド間を移動中
    Typing   // タイピング中
}

/// <summary>
/// プレイヤーの移動、入力、状態遷移を管理するクラス
/// NetworkBehaviourを継承するように変更
/// </summary>
public class PlayerBrain : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    // --- ★変更点: ワールドプレハブへの参照を追加 ---
    [Header("Multiplayer Setup")]
    [Tooltip("プレイヤーごとに生成するワールド環境のプレハブ")]
    public GameObject worldPrefab;
    [Tooltip("P2（クライアント側）のワールドを生成する位置オフセット")]
    public Vector3 p2_worldOffset = new Vector3(1000, 0, 0);

    // --- ★変更点: public参照をprivateに変更し、動的に設定する ---
    private Tilemap blockTilemap;
    private Tilemap itemTilemap;
    private TypingManager typingManager;

    [Header("Audio")]
    [SerializeField] private AudioClip walkSound;
    private AudioSource audioSource;
    
    private PlayerState _currentState = PlayerState.Roaming;
    private Vector3Int _gridTargetPos;
    private Vector3Int _typingTargetPos;

    #region Unity Lifecycle Methods and Network Callbacks
    
    // OnEnable/OnDisable はイベント登録のためこのまま残す
    void OnEnable()
    {
        TypingManager.OnTypingEnded += HandleTypingEnded;
    }

    void OnDisable()
    {
        // --- ★変更点: typingManagerがnullの場合を考慮 ---
        // シーン終了時にオブジェクトが破棄される順序によって発生するエラーを防止
        if (typingManager != null)
        {
            TypingManager.OnTypingEnded -= HandleTypingEnded;
        }
    }

    /// <summary>
    /// シーン開始時に必要なコンポーネント参照などを設定する初期化メソッド
    /// </summary>
    /// <param name="worldInstance">プレイヤーが活動するワールドのルートオブジェクト</param>
    /// <param name="worldOffset">ワールドの座標オフセット</param>
    public void InitializeForScene(GameObject worldInstance, Vector3 worldOffset)
    {
        // ワールド内のコンポーネントへの参照を取得
        // (GetComponentInChildrenは、シングルトンよりも安全に自分専用のインスタンスを見つけられる)
        blockTilemap = worldInstance.GetComponentInChildren<Tilemap>(true); // BlockTilemapを取得
        foreach (var tm in worldInstance.GetComponentsInChildren<Tilemap>(true))
        {
            if (tm.gameObject.name.Contains("Item")) itemTilemap = tm;
            else if (tm.gameObject.name.Contains("Block")) blockTilemap = tm;
        }
        typingManager = worldInstance.GetComponentInChildren<TypingManager>(true);
        
        // プレイヤーの初期位置を設定
        transform.position += worldOffset;
        _gridTargetPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_gridTargetPos);
        
        // ... その他の初期化処理 ...
        CheckForItemAt(_gridTargetPos);
    }


    void Update()
    {
        // ★追加: 必要なコンポーネントが揃うまで待機
        if (blockTilemap == null || typingManager == null) return;
        
        switch (_currentState)
        {
            case PlayerState.Roaming:
                HandleRoamingState();
                break;
            case PlayerState.Moving:
                HandleMovingState();
                break;
            case PlayerState.Typing:
                break;
        }
    }

    /// <summary>
    /// GameObjectとそのすべての子オブジェクトのレイヤーを再帰的に設定するヘルパーメソッド
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    #endregion

    #region State Handling
    // ... (HandleRoamingState, HandleMovingState, HandleTypingEnded は変更なし)
    /// <summary>
    /// Roaming（待機・自由移動）状態の処理
    /// </summary>
    private void HandleRoamingState()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int moveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) moveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) moveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) moveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) moveVec = Vector3Int.right;

            if (moveVec != Vector3Int.zero)
            {
                CheckAndMove(moveVec);
            }
        }
    }

    /// <summary>
    /// Moving（移動中）状態の処理
    /// </summary>
    private void HandleMovingState()
    {
        transform.position = Vector3.MoveTowards(transform.position, blockTilemap.GetCellCenterWorld(_gridTargetPos), moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, blockTilemap.GetCellCenterWorld(_gridTargetPos)) < 0.01f)
        {
            transform.position = blockTilemap.GetCellCenterWorld(_gridTargetPos);
            CheckForItemAt(_gridTargetPos);
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CheckAndGenerateChunksAroundPlayer();
            }
            _currentState = PlayerState.Roaming;
        }
    }

    /// <summary>
    /// TypingManagerからのイベントを処理するメソッド
    /// </summary>
    private void HandleTypingEnded(bool wasSuccessful)
    {
        if (wasSuccessful)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.DestroyConnectedBlocks(_typingTargetPos);
            }
            MoveTo(_typingTargetPos);
        }
        else
        {
            _currentState = PlayerState.Roaming;
        }
    }
    #endregion

    #region Actions
    // ... (CheckAndMove, MoveTo, CheckForItemAt は変更なし)
    /// <summary>
    /// 指定された方向に移動できるかチェックし、行動を決定する
    /// </summary>
    void CheckAndMove(Vector3Int moveVec)
    {
        Vector3Int nextGridPos = _gridTargetPos + moveVec;
        
        if (blockTilemap.HasTile(nextGridPos))
        {
            _typingTargetPos = nextGridPos;
            _currentState = PlayerState.Typing;
            typingManager.StartTyping(moveVec);
        }
        else
        {
            MoveTo(nextGridPos);
        }
    }

    /// <summary>
    /// 指定された座標への移動を開始する
    /// </summary>
    public void MoveTo(Vector3Int targetPos)
    {
        _gridTargetPos = targetPos;
        _currentState = PlayerState.Moving;

         if (walkSound != null && audioSource != null)
         {
            audioSource.PlayOneShot(walkSound);
         }
    }

    /// <summary>
    /// 指定された座標にアイテムがあるかチェックし、あれば取得する
    /// </summary>
    private void CheckForItemAt(Vector3Int position)
    {
        if (itemTilemap == null) return;
        TileBase itemTile = itemTilemap.GetTile(position);
        if (itemTile != null && ItemManager.Instance != null)
        {
            ItemManager.Instance.AcquireItem(itemTile, position);
            itemTilemap.SetTile(position, null);
        }
    }
    #endregion
}