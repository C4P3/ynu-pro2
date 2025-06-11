using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動の速さ（見た目上の補間用）
    public Tilemap blockTilemap; // 破壊可能なブロックが配置されているタイルマップ

    private Vector3Int _targetGridPos; // 目標のグリッド座標
    private bool _isMoving = false;    // 移動中かどうか

    // ゲームマネージャーやタイピングマネージャーへの参照（後で使う）
    // public GameManager gameManager;
    public TypingManager typingManager;

    void Start()
    {
        // 最初の位置をグリッド座標に変換して保持
        _targetGridPos = blockTilemap.WorldToCell(transform.position);
        transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos); // 念のため中心に合わせる
    }

    void Update()
    {
        if (_isMoving)
        {
            // 目標地点までスムーズに移動（補間）
            transform.position = Vector3.MoveTowards(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos), moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, blockTilemap.GetCellCenterWorld(_targetGridPos)) < 0.01f)
            {
                transform.position = blockTilemap.GetCellCenterWorld(_targetGridPos);
                _isMoving = false;
            }
            return; // 移動中は新しい入力を受け付けない
        }

        // --- 入力検知 ---
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

        // 1. 移動先にブロックがあるかチェック
        if (blockTilemap.HasTile(nextGridPos))
        {
            Debug.Log("ブロック発見！タイピングを開始します。");
            // TypingManagerを呼び出す
            typingManager.StartTyping(nextGridPos); 
            return;
        }

        // 2. 移動先に何もない場合（壁なども考慮する場合は別途Tilemapを用意する）
        _targetGridPos = nextGridPos;
        _isMoving = true;
    }

    // ブロック破壊後に呼ばれる関数
    public void MoveTo(Vector3Int targetPos)
    {
        _targetGridPos = targetPos;
        _isMoving = true;
    }
}