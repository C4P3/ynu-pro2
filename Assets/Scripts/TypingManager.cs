// TypingManager.cs
using UnityEngine;
using TMPro;
using Models;

/// <summary>
/// タイピングのUI表示と入力判定を管理するクラス
/// </summary>
public class TypingManager : MonoBehaviour
{
    public static event System.Action<bool> OnTypingEnded;
    
    [Header("UI References")]
    public GameObject typingPanel;
    public TextMeshProUGUI typedText;

    private TypingTextStore _typingTextStore = new TypingTextStore();
    // ★変更: 古い変数を削除し、新しい入力判定モデルのインスタンスを保持する
    private CurrentTypingTextModel _typingModel = new CurrentTypingTextModel();
    private Vector3Int _initialMoveDirection;

    void Start()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        // ★追加: 必要に応じてOSを設定します。Mac環境でテストする場合は OperatingSystemName.Mac を設定してください。
        _typingModel.SetOperatingSystemName(OperatingSystemName.Windows);
    }

    public void StartTyping(Vector3Int moveDirection)
    {
        _initialMoveDirection = moveDirection;
        TypingText currentTypingText = _typingTextStore.RandomTypingText;

        // ローマ字変換モデルを使って初期ローマ字を生成
        var converter = new ConvertHiraganaToRomanModel();
        var initialRomanChars = converter.ConvertHiraganaToRoman(currentTypingText.hiragana.ToCharArray());

        // ★変更: 新しい入力判定モデルを初期化
        _typingModel.SetTitle(currentTypingText.title);
        _typingModel.SetCharacters(initialRomanChars);
        _typingModel.ResetCharactersIndex();
        
        UpdateTypedText();

        if (typingPanel != null)
        {
            typingPanel.SetActive(true);
        }
    }

    void Update()
    {
        if (typingPanel == null || !typingPanel.activeSelf) return;

        // キャンセル機能 (変更なし)
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

        // ★★★ 変更: 文字入力処理を全面的に書き換え ★★★
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            foreach (char c in Input.inputString)
            {
                // アルファベット小文字のみを処理対象とする
                if ((c >= 'a' && c <= 'z') || c == '-')
                {
                    // 入力判定モデルに文字を渡して判定を依頼
                    var result = _typingModel.TypeCharacter(c);

                    // 正しい入力、または完了した場合
                    if (result != TypeResult.Incorrect)
                    {
                        UpdateTypedText(); // 表示を更新
                    }

                    // タイピングが完了した場合
                    if (result == TypeResult.Finished)
                    {
                        OnTypingComplete();
                        return; // このフレームのUpdate処理を抜ける
                    }
                }
            }
        }
    }

    /// <summary>
    /// タイピングが成功裏に完了した際の処理
    /// </summary>
    void OnTypingComplete()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
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
        OnTypingEnded?.Invoke(false);
    }

    /// <summary>
    /// ★★★ 変更: UIのテキストを更新する処理 ★★★
    /// </summary>
    void UpdateTypedText()
    {
        // 入力判定モデルから最新の情報を取得
        string title = _typingModel.Title;
        string currentRomaji = _typingModel.GetRomajiString();
        int typedIndex = _typingModel.TypedIndex;

        // 1行目: 日本語タイトル
        // 2行目: ローマ字（入力進捗を色分け）
        string highlightedText = $"<color=red>{currentRomaji.Substring(0, typedIndex)}</color>";
        string remainingText = currentRomaji.Substring(typedIndex);
        string romajiLine = highlightedText + remainingText;

        // 2行表示（1行目: 日本語, 2行目: ローマ字）
        typedText.text = $"{title}\n{romajiLine}";
    }
}