// TypingManager.cs
using UnityEngine;
using TMPro;
using Models;

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
    private TypingTextStore _typingTextStore = new TypingTextStore();
    private Models.TypingText _currentTypingText;
    private string _currentHiragana; // ひらがな
    private string _currentRomaji;   // ローマ字変換後
    private int _typedIndex;
    private ConvertHiraganaToRomanModel _converter = new ConvertHiraganaToRomanModel();
    private Vector3Int _initialMoveDirection; // 追加: 最初の移動方向を保持

    void Start()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
    }

    public void StartTyping(Vector3Int moveDirection)
    {
        _initialMoveDirection = moveDirection;
        _currentTypingText = _typingTextStore.RandomTypingText;
        _currentHiragana = _currentTypingText.hiragana;
        // ここで変換
        var romanChars = _converter.ConvertHiraganaToRoman(_currentHiragana.ToCharArray());
        _currentRomaji = new string(System.Linq.Enumerable.ToArray(romanChars));
        _typedIndex = 0;

        // title（日本語）を出題
        questionText.text = _currentTypingText.title;
        UpdateTypedText();

        if (typingPanel != null)
        {
            typingPanel.SetActive(true);
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
            if (_typedIndex < _currentRomaji.Length && c == _currentRomaji[_typedIndex])
            {
                _typedIndex++;
                UpdateTypedText();
                if (_typedIndex >= _currentRomaji.Length)
                {
                    OnTypingEnded?.Invoke(true);
                    typingPanel.SetActive(false);
                }
            }
            // キャンセル処理などは必要に応じて追加
        }
    }

    /// <summary>
    /// タイピングを開始する
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
        string highlightedText = $"<color=red>{_currentRomaji.Substring(0, _typedIndex)}</color>";
        string remainingText = _currentRomaji.Substring(_typedIndex);
        typedText.text = highlightedText + remainingText;
    }
}