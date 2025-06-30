// PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;
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

    [Header("Audio")]
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private float walkSoundInterval = 0.4f;
    private AudioSource audioSource;
    private Coroutine walkSoundCoroutine;

    // プレイヤーの現在の状態
    private PlayerState _currentState = PlayerState.Roaming;
    private Vector3Int _gridTargetPos;
    private Vector3Int _typingTargetPos; // タイピング対象のブロック座標

    #region Unity Lifecycle Methods
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
        audioSource = gameObject.AddComponent<AudioSource>(); //AudioSourceの初期化
        audioSource.playOnAwake = false; // 自動再生を無効化
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
            if (walkSoundCoroutine != null)
            {
                StopCoroutine(walkSoundCoroutine);
                walkSoundCoroutine = null;
            }

            CheckForItemAt(_gridTargetPos);

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CheckAndGenerateChunksAroundPlayer();
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
            // ★修正: ブロックを破壊する処理をここに追加
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.DestroyConnectedBlocks(_typingTargetPos);
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
    /// 指定された方向に移動できるかチェックし、行動を決定する
    /// </summary>
    void CheckAndMove(Vector3Int moveVec)
    {
        Vector3Int nextGridPos = _gridTargetPos + moveVec;
        
        if (blockTilemap.HasTile(nextGridPos))
        {
            // ブロックがある場合
            _typingTargetPos = nextGridPos; // 対象を記憶
            _currentState = PlayerState.Typing; // 自身の状態を「タイピング中」に変更
            typingManager.StartTyping(moveVec); // TypingManagerに開始を依頼
        }
        else
        {
            // ブロックがない場合
            MoveTo(nextGridPos);
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
        _gridTargetPos = targetPos;
        _currentState = PlayerState.Moving; // 自身の状態を「移動中」に変更

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
            ItemManager.Instance.AcquireItem(itemTile, position);
            itemTilemap.SetTile(position, null);
        }
    }
    #endregion
}