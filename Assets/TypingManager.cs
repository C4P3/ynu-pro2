using UnityEngine;
using TMPro; // TextMeshProをインポート
using UnityEngine.Tilemaps;

public class TypingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject typingUI; // タイピングUI全体の親オブジェクト
    public TextMeshProUGUI questionText; // 問題文
    public TextMeshProUGUI typedText;    // 入力済みテキスト

    [Header("Game References")]
    public PlayerController player;
    public Tilemap blockTilemap;

    private string[] _questions = { "unity", "game", "development", "drill", "block", "type" }; // 問題リスト
    private string _currentQuestion;
    private int _typedIndex;
    private Vector3Int _targetBlockPos; // 破壊対象のブロック座標

    void Start()
    {
        typingUI.SetActive(false); // 最初は非表示
    }

    void Update()
    {
        // タイピングモード中のみ入力を受け付ける
        if (!typingUI.activeSelf) return;

        // Input.inputString を使うと、そのフレームで入力された文字を取得できる
        foreach (char c in Input.inputString)
        {
            if (_typedIndex < _currentQuestion.Length && c == _currentQuestion[_typedIndex])
            {
                // 正しい文字が入力された
                _typedIndex++;
                UpdateTypedText();

                if (_typedIndex >= _currentQuestion.Length)
                {
                    // 全て打ち終わった
                    OnTypingComplete();
                }
            }
        }
    }

    // タイピングを開始する（PlayerControllerから呼ばれる）
    public void StartTyping(Vector3Int blockPos)
    {
        player.enabled = false; // プレイヤーの操作を一時的に無効化
        _targetBlockPos = blockPos;

        // ランダムに問題を選択
        _currentQuestion = _questions[Random.Range(0, _questions.Length)];
        _typedIndex = 0;

        questionText.text = _currentQuestion;
        UpdateTypedText();
        typingUI.SetActive(true);
    }

    // タイピング完了時の処理
    void OnTypingComplete()
    {
        Debug.Log("タイピング成功！");
        typingUI.SetActive(false); // UIを非表示に

        // ブロックを破壊
        blockTilemap.SetTile(_targetBlockPos, null);

        // プレイヤーをブロックがあった位置へ移動させ、操作を再開
        player.enabled = true;
        player.MoveTo(_targetBlockPos);
    }

    // 入力済みテキストの表示を更新
    void UpdateTypedText()
    {
        // 例: "unity" の "un" まで打ったら "<color=red>un</color>ity" のように表示
        typedText.text = $"<color=red>{_currentQuestion.Substring(0, _typedIndex)}</color>{_currentQuestion.Substring(_typedIndex)}";
    }
}