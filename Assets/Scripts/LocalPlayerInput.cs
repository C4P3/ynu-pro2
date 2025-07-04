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
            levelManager.playerTransform = this.transform; // ★★★ これがあなたのプレイヤーです、と教える ★★★
        }
        else
        {
            Debug.LogError("LevelManagerが見つかりません！ シングルプレイ用のシーンに配置されていますか？");
        }

        // PlayerControllerに必要な参照を設定する
        if (_playerController != null)
        {
            _playerController.levelManager = levelManager;
            if (levelManager != null)
            {
                _playerController.blockTilemap = levelManager.blockTilemap;
                _playerController.itemTilemap = levelManager.itemTilemap;
            }
        }

        // ★★★ シングルプレイ用のマップ生成をここから呼び出す ★★★
        if (levelManager != null)
        {
            levelManager.GenerateMap();
        }

        var typingManager = Object.FindFirstObjectByType<TypingManager>();
        if (typingManager != null)
        {
            _playerController.typingManager = typingManager;
        }
        
        // 参照設定が終わった後に、プレイヤーの初期化処理を呼び出す
        _playerController.Initialize();
        // ★★★ここまで修正★★★

        // GameManagerへの登録も忘れずに行う
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