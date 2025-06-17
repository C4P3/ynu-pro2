// TypingManager.cs
using UnityEngine;
using TMPro; // TextMeshProをインポート

/// <summary>
/// タイピングのUI表示と入力判定を管理するクラス
/// </summary>
public class TypingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject typingPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI typedText;

    [Header("Game References")]
    public PlayerController player;

    private string[] _questions = { "unity", "game", "development", "drill", "block", "type", "item", "oxygen" };
    private string _currentQuestion;
    private int _typedIndex;
    private Vector3Int _targetBlockPos;
    private Vector3Int _initialMoveDirection; // ★追加: タイピングを開始した時の移動方向

    void Start()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
    }

    void Update()
    {
        // タイピングUIが表示されていなければ何もしない
        if (typingPanel == null || !typingPanel.activeSelf) return;

        // --- ★キャンセル機能の追加 ---
        // タイピング中にShift+WASD入力があったかチェック
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int cancelMoveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) cancelMoveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) cancelMoveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) cancelMoveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) cancelMoveVec = Vector3Int.right;

            // 新しい移動入力があり、かつそれが最初の方向と違う場合
            if (cancelMoveVec != Vector3Int.zero && cancelMoveVec != _initialMoveDirection)
            {
                CancelTyping();
                return; // キャンセルしたので、このフレームの文字入力は受け付けない
            }
        }

        // --- 既存の文字入力処理 ---
        foreach (char c in Input.inputString)
        {
            if (_typedIndex < _currentQuestion.Length && c == _currentQuestion[_typedIndex])
            {
                _typedIndex++;
                UpdateTypedText();
                if (_typedIndex >= _currentQuestion.Length)
                {
                    OnTypingComplete();
                }
            }
        }
    }

    /// <summary>
    /// タイピングを開始する（PlayerControllerから呼ばれる）
    /// </summary>
    /// <param name="blockPos">対象ブロックの座標</param>
    /// <param name="moveDirection">タイピングを開始した移動方向</param>
    public void StartTyping(Vector3Int blockPos, Vector3Int moveDirection)
    {
        player.enabled = false;
        _targetBlockPos = blockPos;
        _initialMoveDirection = moveDirection; // ★追加: 方向を保存
        
        _currentQuestion = _questions[Random.Range(0, _questions.Length)];
        _typedIndex = 0;
        
        questionText.text = _currentQuestion;
        UpdateTypedText();

        if (typingPanel != null)
        {
            typingPanel.SetActive(true);
        }
    }

    /// <summary>
    /// タイピング完了時の処理
    /// </summary>
    void OnTypingComplete()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.DestroyConnectedBlocks(_targetBlockPos);
        }
        player.enabled = true;
        player.MoveTo(_targetBlockPos);
    }

    /// <summary>
    /// タイピングを中断する処理
    /// </summary>
    private void CancelTyping()
    {
        Debug.Log("Typing Cancelled.");
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        // プレイヤーの操作を元に戻す
        player.enabled = true;
    }

    /// <summary>
    /// 入力済みテキストの表示を更新する
    /// </summary>
    void UpdateTypedText()
    {
        string highlightedText = $"<color=red>{_currentQuestion.Substring(0, _typedIndex)}</color>";
        string remainingText = _currentQuestion.Substring(_typedIndex);
        typedText.text = highlightedText + remainingText;
    }
}