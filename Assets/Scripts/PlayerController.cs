// PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Tilemap blockTilemap;
    public Tilemap itemTilemap; // ItemTilemapへの参照を追加
    public TypingManager typingManager;

    private Vector3Int _targetGridPos;
    private bool _isMoving = false;

    void Start()
    {
        _targetGridPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
        CheckForItemAt(_targetGridPos); // 開始地点のアイテムもチェック
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos), moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos)) < 0.01f)
            {
                transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
                _isMoving = false;

                CheckForItemAt(_targetGridPos); // 移動完了時にアイテムをチェック

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CheckAndGenerateChunksAroundPlayer();
                }
            }
            return;
        }

        Vector3Int moveVec = Vector3Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))    moveVec = Vector3Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow))  moveVec = Vector3Int.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow))  moveVec = Vector3Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) moveVec = Vector3Int.right;

        if (moveVec != Vector3Int.zero)
        {
            CheckMove(moveVec);
        }
    }

    void CheckMove(Vector3Int moveVec)
    {
        Vector3Int nextGridPos = _targetGridPos + moveVec;
        if (blockTilemap.HasTile(nextGridPos))
        {
            typingManager.StartTyping(nextGridPos);
            return;
        }
        _targetGridPos = nextGridPos;
        _isMoving = true;
    }

    // 新しく追加するメソッド
    private void CheckForItemAt(Vector3Int position)
    {
        TileBase itemTile = itemTilemap.GetTile(position);
        if (itemTile != null && ItemManager.Instance != null)
        {
            ItemManager.Instance.AcquireItem(itemTile, position);
            itemTilemap.SetTile(position, null); // アイテムを消す
        }
    }
    
    public void MoveTo(Vector3Int targetPos)
    {
        _targetGridPos = targetPos;
        _isMoving = true;
    }
}