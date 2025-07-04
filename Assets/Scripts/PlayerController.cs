// PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

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
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("References")]
    public Tilemap blockTilemap;
    public Tilemap itemTilemap;
    public TypingManager typingManager;
    public LevelManager levelManager;

    [Header("Audio")]
    [SerializeField] private AudioClip walkSound;
    private AudioSource audioSource;

    // プレイヤーの現在の状態
    private PlayerState _currentState = PlayerState.Roaming;
    private Vector3Int _gridTargetPos;
    private Vector3Int _typingTargetPos; // タイピング対象のブロック座標
    private Vector3Int _lastMoveDirection = Vector3Int.right; // デフォルト右向き

    private NetworkPlayerInput _networkInput;

    #region Unity Lifecycle Methods

    void Awake()
    {
        // ★★★ このメソッドを追加、または追記 ★★★
        // 自分のゲームオブジェクトについているTypingManagerを取得する
        typingManager = GetComponent<TypingManager>();
        _networkInput = GetComponent<NetworkPlayerInput>();
    }
    void OnEnable()
    {
        // TypingManagerのイベントに、自分のメソッドを登録
        TypingManager.OnTypingEnded += HandleTypingEnded;
    }

    void OnDisable()
    {
        // オブジェクトが無効になるときに、登録を解除（メモリリーク防止）
        TypingManager.OnTypingEnded -= HandleTypingEnded;
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // ★★★ ここから新しいメソッドを追加 ★★★
    /// <summary>
    /// Tilemapなどの参照が設定された後に呼び出す初期化処理
    /// </summary>
    public void Initialize()
    {
        // Start()から移動してきたコード
        _gridTargetPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_gridTargetPos);
        CheckForItemAt(_gridTargetPos);
    }

    void Update()
    {
        // 状態に応じて処理を分岐
        switch (_currentState)
        {
            case PlayerState.Roaming:
                HandleRoamingState();
                break;
            case PlayerState.Moving:
                HandleMovingState();
                break;
            case PlayerState.Typing:
                // タイピング中はプレイヤー自身は何もしない
                break;
        }
    }
    #endregion

    #region State Handling
    /// <summary>
    /// Roaming（待機・自由移動）状態の処理
    /// </summary>
    private void HandleRoamingState()
    {
        // ★★★ 変更点 ★★★
        // このメソッド内のキー入力処理は、後で新しく作る入力用スクリプトに移動させるため、
        // 残しておいても良いですが、最終的には削除、またはコメントアウトします。
        // 今回の設計では、このメソッドはUpdateから呼ばれ続けますが、中身は空っぽでも問題ありません。
        
        // if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        // {
        //     Vector3Int moveVec = Vector3Int.zero;
        //     if (Input.GetKeyDown(KeyCode.W)) moveVec = Vector3Int.up;
        //     if (Input.GetKeyDown(KeyCode.S)) moveVec = Vector3Int.down;
        //     if (Input.GetKeyDown(KeyCode.A)) moveVec = Vector3Int.left;
        //     if (Input.GetKeyDown(KeyCode.D)) moveVec = Vector3Int.right;
        //
        //     if (moveVec != Vector3Int.zero)
        //     {
        //         OnMoveInput(moveVec); // 新しいメソッドを呼ぶように変更
        //     }
        // }
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
            if (levelManager != null)
            {
                levelManager.CheckAndGenerateChunksAroundPlayer();
            }
            // 移動が完了したので、Roaming状態に戻る
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
            if (levelManager != null)
            {
                levelManager.DestroyConnectedBlocks(_typingTargetPos);
            }
            
            // タイピングに成功したら、対象ブロックへ移動を開始
            MoveTo(_typingTargetPos);
        }
        else
        {
            // キャンセルされたら、Roaming状態に戻る
            _currentState = PlayerState.Roaming;
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// 外部からの移動入力に基づいて行動を開始する公開メソッド
    /// </summary>
    /// <param name="moveVec">移動方向のベクトル</param>
    public void OnMoveInput(Vector3Int moveVec)
    {
        // Roaming状態でない場合や、移動ベクトルがゼロの場合は何もしない
        if (_currentState != PlayerState.Roaming || moveVec == Vector3Int.zero)
        {
            return;
        }

        // 元々の HandleRoamingState にあったロジックをここに集約
        CheckAndMove(moveVec);
    }

    /// <summary>
    /// 指定された方向に移動できるかチェックし、行動を決定する
    /// </summary>
    void CheckAndMove(Vector3Int moveVec)
    {
        Vector3Int nextGridPos = _gridTargetPos + moveVec;

        if (blockTilemap.HasTile(nextGridPos))
        {
            // ブロックがある場合
            _typingTargetPos = nextGridPos;
            _currentState = PlayerState.Typing;

            // ★★★ 3. ローカルプレイヤーの場合のみタイピングを開始する ★★★
            // _networkInputがnull（シングルプレイ時）か、isLocalPlayerがtrueの場合のみ実行
            if (_networkInput == null || _networkInput.isLocalPlayer)
            {
                typingManager.StartTyping(moveVec);
            }
        }
        else
        {
            // ブロックがない場合
            MoveTo(nextGridPos);
        }
        
        if (moveVec != Vector3Int.zero)
        {
            _lastMoveDirection = moveVec;
        }
    }

    /// <summary>
    /// 指定された座標への移動を開始する
    /// </summary>
    public void MoveTo(Vector3Int targetPos)
    {
        _gridTargetPos = targetPos;
        _currentState = PlayerState.Moving; // 自身の状態を「移動中」に変更

        //移動音を再生
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
        TileBase itemTile = itemTilemap.GetTile(position);
        if (itemTile != null && ItemManager.Instance != null)
        {
            // ★★★ 第3引数に自身のlevelManagerを渡す ★★★
            ItemManager.Instance.AcquireItem(itemTile, position, levelManager, this.transform);
            itemTilemap.SetTile(position, null);
        }
    }

    public Vector3Int GetLastMoveDirection()
    {
        return _lastMoveDirection;
    }
    #endregion
}