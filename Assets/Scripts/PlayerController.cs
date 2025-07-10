using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System.Collections;

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
    public AnimationManager animationManager;

    [HideInInspector]
    public int playerIndex; // NetworkPlayerInputから設定される

    [Header("Audio")]
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private float walkSoundInterval = 0.4f;
    private AudioSource audioSource;
    private Coroutine walkSoundCoroutine;

    // プレイヤーの現在の状態
    private PlayerState _currentState = PlayerState.Roaming;
    private Vector3Int _gridTargetPos;
    private Vector3Int _typingTargetPos; // タイピング対象のブロック座標
    private Vector3Int _lastMoveDirection = Vector3Int.up; // デフォルト上向き

    private NetworkPlayerInput _networkInput;

    private bool _isStunned = false;
    private float _stunTimer = 0f;

    public bool IsStunned => _isStunned;

    #region Unity Lifecycle Methods

    void Awake()
    {
        // 自分のゲームオブジェクトについているTypingManagerを取得する
        _networkInput = GetComponent<NetworkPlayerInput>();
        typingManager = GetComponent<TypingManager>();

        // 自分のTypingManagerのイベントだけを購読する
        if (typingManager != null)
        {
            typingManager.OnTypingEnded += HandleTypingEnded;
        }
        else
        {
            Debug.LogError("PlayerController could not find TypingManager on the same GameObject!");
        }
    }
    void OnEnable()
    {
    }

    void OnDisable()
    {
    }
    void OnDestroy()
    {
        // TypingManagerのイベント購読を解除する
        if (typingManager != null)
        {
            typingManager.OnTypingEnded -= HandleTypingEnded;
        }
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Tilemapなどの参照が設定された後に呼び出す初期化処理
    /// </summary>
    public void Initialize()
    {
        // プレイヤーの初期位置をブロックタイルマップの中心に設定
        _gridTargetPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_gridTargetPos);
        CheckForItemAt(_gridTargetPos);
        // アニメーションを初期状態(Idle)に設定
        if (animationManager != null)
        {
            animationManager.SetWalking(false);
            animationManager.UpdateSpriteDirection(_lastMoveDirection);
        }
    }

    void Update()
    {
        // スタン中はすべての入力を無効化
        if (_isStunned)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                _isStunned = false;
            }
            // ここでUpdate内の入力処理を全てスキップ
            return; 
        }

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
        // 入力待ち
        // SHIFTキーが押されているかチェック
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int moveVec = Vector3Int.zero;

            // WASDの長押しをチェックし、移動方向を決定
            if (Input.GetKey(KeyCode.W))      moveVec = Vector3Int.up;
            else if (Input.GetKey(KeyCode.S)) moveVec = Vector3Int.down;
            else if (Input.GetKey(KeyCode.A)) moveVec = Vector3Int.left;
            else if (Input.GetKey(KeyCode.D)) moveVec = Vector3Int.right;

            // 移動方向が決定された場合（キーが押されている場合）
            if (moveVec != Vector3Int.zero)
            {
                // 既存の移動/タイピング開始処理を呼び出す
                // これにより、移動先にブロックがあればタイピング、なければ移動が開始される
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
            if (walkSoundCoroutine != null)
            {
                StopCoroutine(walkSoundCoroutine);
                walkSoundCoroutine = null;
            }

            CheckForItemAt(_gridTargetPos);
            if (levelManager != null)
            {
                levelManager.CheckAndGenerateChunksAroundPlayer();
            }
            // 移動が完了したので、Roaming状態に戻る
            _currentState = PlayerState.Roaming;

            //　移動が完了したので、アニメーションをIdleに戻す
            if (animationManager != null)
            {
                animationManager.SetWalking(false);
            }
        }
    }

    /// <summary>
    /// TypingManagerからのイベントを処理するメソッド
    /// </summary>
    private void HandleTypingEnded(bool wasSuccessful)
    {
        // タイピングが成功/失敗問わず終了したので、アニメーションを停止する
        if (animationManager != null)
        {
            animationManager.SetTyping(false);
        }
        if (wasSuccessful)
        {
            if (_networkInput != null)
            {
                // 【マルチプレイ時】NetworkPlayerInputに破壊を依頼
                _networkInput.CmdDestroyBlock(_typingTargetPos);
            }
            else
            {
                // 【シングルプレイ時】直接LevelManagerを呼ぶ
                if (levelManager != null)
                {
                    levelManager.DestroyConnectedBlocks(_typingTargetPos);
                }
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
            if (levelManager != null && levelManager.unchiItemData != null && blockTilemap.GetTile(nextGridPos) == levelManager.unchiItemData.unchiTile)
            {
                // ウンチタイルがある場合は、タイピングを開始しない
                if (animationManager != null)
                {
                    animationManager.SetTyping(false);
                    animationManager.SetWalking(false);
                }
                return;
            }

            // ブロックがある場合
            _typingTargetPos = nextGridPos;
            _currentState = PlayerState.Typing;

            // タイピング開始に合わせて、Attackアニメーションを開始する
            if (animationManager != null)
            {
                animationManager.SetTyping(true);
            }

            // _networkInputがnull（シングルプレイ時）か、isLocalPlayerがtrueの場合のみ実行
            if (_networkInput == null || _networkInput.isLocalPlayer)
            {
                // ブロックの塊サイズを取得
                int clusterSize = 1;
                if (levelManager != null)
                {
                    clusterSize = levelManager.GetConnectedBlockCount(_typingTargetPos);
                }
                typingManager.StartTyping(moveVec, clusterSize);
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
            // プレイヤーの向きが変わったら、スプライトの向きも更新
            if (animationManager != null)
            {
                animationManager.UpdateSpriteDirection(_lastMoveDirection);
            }
        }
    }

    private void PlayWalkSound()
    {
        if (walkSounds != null && walkSounds.Length > 0 && audioSource != null)
    {
        int index = Random.Range(0, walkSounds.Length);
        audioSource.PlayOneShot(walkSounds[index]);
    }
    }

    private IEnumerator WalkSoundLoop()
    {
        while (_currentState == PlayerState.Moving)
    {
        PlayWalkSound();
        yield return new WaitForSeconds(walkSoundInterval);
    }

    }
    /// <summary>
    /// 指定された座標への移動を開始する
    /// </summary>
    public void MoveTo(Vector3Int targetPos)
    {
        // Roaming状態でない場合は何もしない
        _gridTargetPos = targetPos;
        _currentState = PlayerState.Moving;

        // 移動開始に合わせて、歩行アニメーションを開始
        if (animationManager != null)
        {
            animationManager.SetWalking(true);
        }

        //移動音を再生
         if (walkSoundCoroutine != null)
    {
        StopCoroutine(walkSoundCoroutine);
    }
    walkSoundCoroutine = StartCoroutine(WalkSoundLoop());

    }

    /// <summary>
    /// 指定された座標にアイテムがあるかチェックし、あれば取得する
    /// </summary>
    private void CheckForItemAt(Vector3Int position)
    {
        TileBase itemTile = itemTilemap.GetTile(position);
        if (itemTile != null && ItemManager.Instance != null)
        {
            if (levelManager != null && levelManager.unchiItemData != null && itemTile == levelManager.unchiItemData.unchiTile) return;
            ItemManager.Instance.AcquireItem(itemTile, position, levelManager, this.transform);
        }
    }

    public Vector3Int GetLastMoveDirection()
    {
        return _lastMoveDirection;
    }
    #endregion

    // スタンを付与するメソッド
    public void Stun(float duration)
    {
        _isStunned = true;
        _stunTimer = duration;
    }
}