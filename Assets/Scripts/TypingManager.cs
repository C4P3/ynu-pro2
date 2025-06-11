using UnityEngine;
using TMPro;

public class TypingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject typingUI;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI typedText;

    [Header("Game References")]
    public PlayerController player;
    // public Tilemap blockTilemap; // LevelManagerが一元管理するので不要になる

    private string[] _questions = { "unity", "game", "development", "drill", "block", "type" };
    private string _currentQuestion;
    private int _typedIndex;
    private Vector3Int _targetBlockPos;

    void Start()
    {
        typingUI.SetActive(false);
    }

    void Update()
    {
        if (!typingUI.activeSelf) return;

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

    public void StartTyping(Vector3Int blockPos)
    {
        player.enabled = false;
        _targetBlockPos = blockPos;
        _currentQuestion = _questions[Random.Range(0, _questions.Length)];
        _typedIndex = 0;
        questionText.text = _currentQuestion;
        UpdateTypedText();
        typingUI.SetActive(true);
    }

    void OnTypingComplete()
    {
        Debug.Log("タイピング成功！");
        typingUI.SetActive(false);

        // --- 修正点 ---
        // LevelManagerの連結破壊メソッドを呼び出す
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.DestroyConnectedBlocks(_targetBlockPos);
        }
        
        // プレイヤーをブロックがあった位置へ移動させ、操作を再開
        player.enabled = true;
        player.MoveTo(_targetBlockPos);
    }

    void UpdateTypedText()
    {
        typedText.text = $"<color=red>{_currentQuestion.Substring(0, _typedIndex)}</color>{_currentQuestion.Substring(_typedIndex)}";
    }
}
