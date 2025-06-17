// TypingManager.cs
using UnityEngine;
using TMPro;

/// <summary>
/// タイピングのUI表示と入力判定を管理するクラス
/// </summary>
public class TypingManager : MonoBehaviour
{
    // ★追加: タイピングが終了したことを通知するためのイベント
    // boolは成功(true)かキャンセル(false)かを示す
    public static event System.Action<bool> OnTypingEnded;

    [Header("UI References")]
    public GameObject typingPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI typedText;

    // ★削除: PlayerControllerへの直接参照は不要になる
    // public PlayerController player;

    private string[] _questions = { "unity", "game", "development", "drill", "block", "type", "item", "oxygen" };
    private string _currentQuestion;
    private int _typedIndex;
    private Vector3Int _initialMoveDirection;

    void Start()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (typingPanel == null || !typingPanel.activeSelf) return;

        // キャンセル機能
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int cancelMoveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) cancelMoveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) cancelMoveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) cancelMoveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) cancelMoveVec = Vector3Int.right;

            if (cancelMoveVec != Vector3Int.zero && cancelMoveVec != _initialMoveDirection)
            {
                CancelTyping();
                return;
            }
        }

        // 文字入力処理
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
    /// タイピングを開始する
    /// </summary>
    public void StartTyping(Vector3Int moveDirection)
    {
        _initialMoveDirection = moveDirection;
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
        // ★変更: 成功した(true)ことをイベントで通知
        OnTypingEnded?.Invoke(true);
    }

    /// <summary>
    /// タイピングを中断する処理
    /// </summary>
    private void CancelTyping()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        // ★変更: キャンセル(false)したことをイベントで通知
        OnTypingEnded?.Invoke(false);
    }

    void UpdateTypedText()
    {
        string highlightedText = $"<color=red>{_currentQuestion.Substring(0, _typedIndex)}</color>";
        string remainingText = _currentQuestion.Substring(_typedIndex);
        typedText.text = highlightedText + remainingText;
    }
}