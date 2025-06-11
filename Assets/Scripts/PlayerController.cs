using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Tilemap blockTilemap;
    public TypingManager typingManager;

    private Vector3Int _targetGridPos;
    private bool _isMoving = false;

    void Start()
    {
        _targetGridPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
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

                // --- 修正点 ---
                // 移動完了後にチャンクのチェックと生成を依頼する
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CheckAndGenerateChunksAroundPlayer();
                }
            }
            return;
        }
        
        // (入力処理部分は変更なし)
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

    // (CheckMove と MoveTo メソッドは変更なし)
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

    public void MoveTo(Vector3Int targetPos)
    {
        _targetGridPos = targetPos;
        _isMoving = true;
    }
}
