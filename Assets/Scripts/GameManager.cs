using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトン

    /// <summary>
    /// 酸素量が変化したときに呼び出されるイベント
    /// 引数: (現在の酸素量, 最大酸素量)
    /// </summary>
    public static event Action<float, float> OnOxygenChanged;

    [Header("Oxygen")]
    public float maxOxygen = 100f;              // 最大酸素量
    public float oxygenDecreaseRate = 0.5f;       // 1秒あたりに減る酸素量
    public Slider oxygenSlider;                 // 酸素ゲージUI
    public TextMeshProUGUI oxygenText;

    [Header("Oxygen Bar Colors")]
    public Color fullOxygenColor = Color.green;     // 満タン時の色 (黄緑)
    public Color lowOxygenColor = Color.yellow;     // 30%以下になった時の色
    public Color criticalOxygenColor = Color.red;   // 10%以下になった時の色
    private Image fillImage;                        // ゲージの色を変更するためのImageコンポーネント

    [Header("UI References")]
    public TextMeshProUGUI survivalTimeDisplay;    // 生存時間をリアルタイムで表示するTextMeshProUGUI
    public PlayerController LocalPlayer { get; private set; }

    [Header("Game State")]
    private float _currentOxygen;               // 現在の酸素量
    private bool _isOxygenInvincible = false;   // 酸素減少無効フラグ
    public bool IsOxygenInvincible => _isOxygenInvincible; // 外部からの読み取り専用プロパティ
    private float _survivalTime = 0f;           // 生存時間
    private int _blocksDestroyed = 0;           // 破壊したブロック数
    private int _missTypes = 0;                 // ミスタイプ数
    private bool _isGameOver = false;           // ゲーム終了フラグ (以前の_gameEndedをこれに統一)

    // 結果表示用UIの参照 (Unityエディタで設定)
    [Header("Game Over UI")]
    public GameObject gameOverPanel; // 結果表示パネル
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalSurvivalTimeText;
    public TextMeshProUGUI finalBlocksDestroyedText;
    public TextMeshProUGUI finalMissTypesText;

    void Awake()
    {
        // シングルトンのインスタンスを設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ゲーム開始時に酸素を最大値に設定
        _currentOxygen = maxOxygen;
        // 酸素ゲージの初期化
        if (oxygenSlider != null && oxygenSlider.fillRect != null)
        {
            fillImage = oxygenSlider.fillRect.GetComponent<Image>();
        }
        UpdateOxygenUI();
        // UIマネージャーなどに初期状態を通知する
        OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);

        // 生存時間の初期化と表示更新
        _survivalTime = 0f;
        _isGameOver = false;
        UpdateSurvivalTimeDisplay();

        // ゲームオーバーパネルの初期化
        if (gameOverPanel != null)
        {
            // CanvasGroupがある場合はそのプロパティも設定
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameOverPanel.SetActive(false); // CanvasGroupがない場合はSetActiveで非表示
            }
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!_isGameOver)
        {
            // 酸素量の減少
            if (!_isOxygenInvincible)
            {
                _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
                _currentOxygen = Mathf.Max(0, _currentOxygen); // 0未満にならないようにする
                UpdateOxygenUI();

                OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen); //酸素切れエフェクト用

                if (_currentOxygen <= 0)
                {
                    GameOver();
                }
            }

            // 生存時間の更新
            _survivalTime += Time.deltaTime;
            UpdateSurvivalTimeDisplay();
        }
    }

    // 生存時間UIをリアルタイムで更新するメソッド (メソッド名を変更しました)
    private void UpdateSurvivalTimeDisplay()
    {
        if (survivalTimeDisplay != null)
        {
            // TimeSpanを使って時間表示をフォーマット
            TimeSpan timeSpan = TimeSpan.FromSeconds(_survivalTime);
            // "MM:SS.FFF" (分:秒.ミリ秒) 形式で表示 (ミリ秒は2桁に統一)
            survivalTimeDisplay.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }
    }

    // ゲームオーバー処理
    public void GameOver()
    {
        if (_isGameOver) return; // 既にゲームオーバーなら何もしない

        _isGameOver = true;
        Time.timeScale = 0f; // ゲームを一時停止
        GameSceneBGMManager.Instance.StopBGM(); // BGMを停止
        GameSceneBGMManager.Instance.PlayBGM(GameSceneBGMManager.Instance.gameOverBGM); // ゲームオーバーBGMを再生
        Debug.Log("Game Over!");
        Debug.Log($"Final Survival Time: {_survivalTime} seconds");
        Debug.Log($"Blocks Destroyed: {_blocksDestroyed}");
        Debug.Log($"Miss Types: {_missTypes}");

        DisplayGameOverResults();
    }

    // ゲームオーバー結果の表示
    private void DisplayGameOverResults()
    {
        if (gameOverPanel != null)
        {
            // CanvasGroupがある場合、Alphaを1、InteractableとBlocks Raycastsをtrueにする
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                // CanvasGroupがない場合、単にSetActiveで表示
                gameOverPanel.SetActive(true);
            }

            // スコアの計算
            // スコア = 最終生存時間(秒) + 壊したブロック数 - ミスタイプ数
            int score = Mathf.FloorToInt(_survivalTime) + _blocksDestroyed - _missTypes;
            if (score < 0) score = 0; // スコアがマイナスにならないように

            // 各TextMeshProUGUIに値を設定
            if (finalScoreText != null)
                finalScoreText.text = $"スコア: {score}";

            if (finalSurvivalTimeText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(_survivalTime);
                // "MM:SS.FFF" (分:秒.ミリ秒) 形式で表示 (ミリ秒は2桁に統一)
                finalSurvivalTimeText.text = $"生存時間: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
            }

            if (finalBlocksDestroyedText != null)
                finalBlocksDestroyedText.text = $"破壊したブロック数: {_blocksDestroyed}";

            if (finalMissTypesText != null)
                finalMissTypesText.text = $"ミスタイプ数: {_missTypes}";
        }
    }

    // 酸素を回復する（アイテム取得時に呼ばれる）
    public void RecoverOxygen(float amount)
    {
        _currentOxygen += amount;
        _currentOxygen = Mathf.Min(_currentOxygen, maxOxygen); // 最大値を超えないように
        UpdateOxygenUI();
        // 酸素量が変化したことをイベントで通知
        OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
    }

    // 酸素ゲージUIを現在の酸素量に合わせて更新する
    void UpdateOxygenUI()
    {
        if (oxygenSlider != null)
        {
            oxygenSlider.value = _currentOxygen / maxOxygen;
        }

        if (oxygenText != null)
        {
            oxygenText.text = $"酸素: {Mathf.CeilToInt(_currentOxygen)}";
        }

        // 酸素残量に応じて色を変化
        if (fillImage != null)
        {
            float oxygenPercentage = _currentOxygen / maxOxygen;

            if (oxygenPercentage <= 0.10f) // 10%以下
            {
                fillImage.color = criticalOxygenColor;
            }
            else if (oxygenPercentage <= 0.30f) // 30%以下
            {
                fillImage.color = lowOxygenColor;
            }
            else // 30%より上
            {
                fillImage.color = fullOxygenColor;
            }
        }
    }

    // 一時的に酸素量の減少を無効化する（アイテム取得時に呼ばれる）
    public System.Collections.IEnumerator TemporaryOxygenInvincibility(float duration)
    {
        _isOxygenInvincible = true;
        yield return new WaitForSeconds(duration);
        _isOxygenInvincible = false;
    }

    /// <summary>
    /// ローカルプレイヤーをGameManagerに登録する
    /// </summary>
    /// <param name="player">登録するプレイヤーのPlayerController</param>
    public void RegisterLocalPlayer(PlayerController player)
    {
        LocalPlayer = player;
        Debug.Log($"Local Player '{player.name}' has been registered.");
    }

    // 秒数を「分:秒.ミリ秒」形式にフォーマットする
    private string FormatTime(float timeInSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}"; // ミリ秒は上位2桁
    }

    // 最終的な生存時間を取得する
    public float GetSurvivalTime()
    {
        return _survivalTime;
    }

    // ブロック破壊数をカウントする
    public void AddDestroyedBlock()
    {
        if (!_isGameOver) // ゲームオーバー中でない場合のみ加算
        {
            _blocksDestroyed++;
        }
    }

    // ミスタイプ数をカウントする
    public void AddMissType()
    {
        if (!_isGameOver) // ゲームオーバー中でない場合のみ加算
        {
            _missTypes++;
        }
    }
}