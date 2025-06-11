// TypingManager.cs
using UnityEngine;
using TMPro; // TextMeshProをインポート

/// <summary>
/// タイピングのUI表示と入力判定を管理するクラス
/// </summary>
public class TypingManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("タイピングUI全体の親オブジェクト")]
    public GameObject typingUI;
    [Tooltip("問題文を表示するTextMeshProUGUI")]
    public TextMeshProUGUI questionText;
    [Tooltip("入力中のテキストを表示するTextMeshProUGUI")]
    public TextMeshProUGUI typedText;

    [Header("Game References")]
    [Tooltip("プレイヤーへの参照")]
    public PlayerController player;

    // 問題文のリスト
    private string[] _questions = { "unity", "game", "development", "drill", "block", "type", "item", "oxygen" };
    private string _currentQuestion;
    private int _typedIndex;
    private Vector3Int _targetBlockPos; // 破壊対象のブロック座標

    void Start()
    {
        // ゲーム開始時はUIを非表示にする
        typingUI.SetActive(false);
    }

    void Update()
    {
        // タイピングUIが表示されているときのみ入力を受け付ける
        if (!typingUI.activeSelf) return;

        // Input.inputString を使うと、そのフレームで入力された文字をまとめて取得できる
        foreach (char c in Input.inputString)
        {
            // 入力すべき文字が残っていて、かつ正しい文字が入力された場合
            if (_typedIndex < _currentQuestion.Length && c == _currentQuestion[_typedIndex])
            {
                // 入力済みインデックスを進める
                _typedIndex++;
                UpdateTypedText();

                // 全ての文字を打ち終わった場合
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
    public void StartTyping(Vector3Int blockPos)
    {
        // プレイヤーの操作を一時的に無効化
        player.enabled = false;
        _targetBlockPos = blockPos;
        
        // ランダムに問題を選択
        _currentQuestion = _questions[Random.Range(0, _questions.Length)];
        _typedIndex = 0;
        
        // UIを更新して表示
        questionText.text = _currentQuestion;
        UpdateTypedText();
        typingUI.SetActive(true);
    }

    /// <summary>
    /// タイピング完了時の処理
    /// </summary>
    void OnTypingComplete()
    {
        Debug.Log("タイピング成功！");
        typingUI.SetActive(false); // UIを非表示に

        // LevelManagerにブロックの破壊を依頼
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.DestroyConnectedBlocks(_targetBlockPos);
        }

        // プレイヤーをブロックがあった位置へ移動させ、操作を再開
        player.enabled = true;
        player.MoveTo(_targetBlockPos);
    }

    /// <summary>
    /// 入力済みテキストの表示を更新する
    /// 例: "unity" の "un" まで打ったら "<color=red>un</color>ity" のように表示
    /// </summary>
    void UpdateTypedText()
    {
        string highlightedText = $"<color=red>{_currentQuestion.Substring(0, _typedIndex)}</color>";
        string remainingText = _currentQuestion.Substring(_typedIndex);
        typedText.text = highlightedText + remainingText;
    }
}