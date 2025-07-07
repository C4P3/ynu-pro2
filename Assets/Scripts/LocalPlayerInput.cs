// LocalPlayerInput.cs (これは Multiplayer フォルダの外に置く)
using UnityEngine;

/// <summary>
/// シングルプレイ時にキーボード入力をPlayerControllerに伝えるだけのクラス
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class LocalPlayerInput : MonoBehaviour
{
    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        
        var levelManager = Object.FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.playerTransform = this.transform;
            // PlayerControllerに必要な参照を設定
            _playerController.levelManager = levelManager;
            _playerController.blockTilemap = levelManager.blockTilemap;
            _playerController.itemTilemap = levelManager.itemTilemap;
            
            // マップ生成を呼び出し
            levelManager.GenerateMap();
        }
        else
        {
            Debug.LogError("LevelManagerが見つかりません！");
        }

        // ★★★ PlayerControllerのtypingManagerを設定する処理は不要なので削除 ★★★
        /*
        var typingManager = FindObjectOfType<TypingManager>();
        if (typingManager != null)
        {
            _playerController.typingManager = typingManager;
        }
        */
        
        // GameManagerへの登録
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLocalPlayer(_playerController);
        }
    }

        void Update()
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
                // リファクタリングしたPlayerControllerの公開メソッドを呼び出す
                _playerController.OnMoveInput(moveVec);
            }
        }
    }
}