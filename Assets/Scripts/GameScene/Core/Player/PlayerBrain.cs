// 【新規】
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerState
{
    Roaming,
    Moving,
    Typing
}

/// <summary>
/// プレイヤーの動作ロジック（頭脳）を担当する。ネットワークに依存しない。
/// </summary>
public class PlayerBrain : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip walkSound;
    private AudioSource audioSource;
    
    // 参照はprivateにし、InitializeForSceneで設定する
    private Tilemap blockTilemap;
    private Tilemap itemTilemap;
    private TypingManager typingManager;

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
        if (typingManager != null) // 安全のためnullチェック
        {
            TypingManager.OnTypingEnded -= HandleTypingEnded;
        }
    }

    void Update()
    {
        // 初期化が済んでいない場合は何もしない
        if (blockTilemap == null) return;

        switch (_currentState)
        {
            case PlayerState.Roaming: HandleRoamingState(); break;
            case PlayerState.Moving: HandleMovingState(); break;
            case PlayerState.Typing: break;
        }
    }

    /// <summary>
    /// NetworkPlayerやSingleplayerInitializerから呼び出される初期化メソッド
    /// </summary>
    public void InitializeForScene(GameObject worldInstance, Vector3 worldOffset)
    {
        // 1. ワールド内のコンポーネント参照を取得
        foreach (var tm in worldInstance.GetComponentsInChildren<Tilemap>(true))
        {
            if (tm.gameObject.name.Contains("Item")) itemTilemap = tm;
            else if (tm.gameObject.name.Contains("Block")) blockTilemap = tm;
        }
        typingManager = worldInstance.GetComponentInChildren<TypingManager>(true);

        // 2. プレイヤー自身のコンポーネントを初期化
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 3. プレイヤーの位置を初期化
        transform.position += worldOffset;
        _gridTargetPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_gridTargetPos);
        
        // 4. LevelManagerを初期化
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.Initialize(this.transform);
        }

        // 5. 初期位置のアイテムをチェック
        CheckForItemAt(_gridTargetPos);
    }
    #endregion

    #region State Handling
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