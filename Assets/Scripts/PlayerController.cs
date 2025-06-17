// PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーの移動、入力、アイテム取得を管理するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("References")]
    public Tilemap blockTilemap;
    public Tilemap itemTilemap;
    public TypingManager typingManager;

    private Vector3Int _targetGridPos;
    private bool _isMoving = false;

    void Start()
    {
        _targetGridPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
        CheckForItemAt(_targetGridPos);
    }

    void Update()
    {
        // 移動中は新しい入力を受け付けない
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos), moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos)) < 0.01f)
            {
                transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
                _isMoving = false;
                CheckForItemAt(_targetGridPos);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CheckAndGenerateChunksAroundPlayer();
                }
            }
            return;
        }

        // --- ★入力検知の変更 ---
        // Shiftキーが押されている場合のみ、移動入力を検知する
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int moveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) moveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) moveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) moveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) moveVec = Vector3Int.right;

            if (moveVec != Vector3Int.zero)
            {
                CheckMove(moveVec);
            }
        }
    }

    /// <summary>
    /// 指定された方向に移動できるかチェックする
    /// </summary>
    void CheckMove(Vector3Int moveVec)
    {
        Vector3Int nextGridPos = _targetGridPos + moveVec;
        
        // 移動先にブロックがあればタイピングを開始
        if (blockTilemap.HasTile(nextGridPos))
        {
            // ★変更点: TypingManagerに移動方向も渡す
            typingManager.StartTyping(nextGridPos, moveVec);
            return;
        }

        // ブロックがなければ移動開始
        _targetGridPos = nextGridPos;
        _isMoving = true;
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
    
    /// <summary>
    /// 外部からプレイヤーの移動を命令するためのメソッド
    /// </summary>
    public void MoveTo(Vector3Int targetPos)
    {
        _targetGridPos = targetPos;
        _isMoving = true;
    }
}