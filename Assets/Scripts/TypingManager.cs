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

    void Start()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
    }
    public void StartTyping(Vector3Int moveDirection)
    {
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
    void UpdateTypedText()
    {
        string highlightedText = $"<color=red>{_currentRomaji.Substring(0, _typedIndex)}</color>";
        string remainingText = _currentRomaji.Substring(_typedIndex);
        typedText.text = highlightedText + remainingText;
    }
}