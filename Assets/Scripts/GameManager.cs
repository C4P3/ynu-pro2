using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトン

    /// <summary>
    /// 酸素量が変化したときに呼び出されるイベント
    /// 引数: (現在の酸素量, 最大酸素量)
    /// </summary>
    public static event Action<float, float> OnOxygenChanged;
    // ゲーム開始時に呼び出されるイベント
    public static event Action OnGameStarted;
    
    [Header("PlayFab")]
    private const string LeaderboardName = "SinglePlayerScore"; // PlayFabでのリーダーボード名
    public int PlayerRank { get; private set; }
    public int PlayerBestScore { get; private set; }

    [Header("GameUI")]
    public GameObject UIPanel;

    [Header("Oxygen")]
    public float maxOxygen = 100f;              // 最大酸素量
    public float oxygenDecreaseRate = 0.5f;       // 1秒あたりに減る酸素量
    public Slider oxygenSlider;                 // 酸素ゲージUI
    public TextMeshProUGUI oxygenText;
    public RectTransform fillRectTransform; // InspectorでFillオブジェクトのRectTransformをアタッチ

    [Header("Oxygen Bar Colors")]
    public Color fullOxygenColor = Color.green;     // 満タン時の色 (黄緑)
    public Color lowOxygenColor = Color.yellow;     // 30%以下になった時の色
    public Color criticalOxygenColor = Color.red;   // 10%以下になった時の色
    public Image fillImage;                        // ゲージの色を変更するためのImageコンポーネント

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

    [Header("Sound Effects")]
    public AudioClip startsound; // ゲーム開始時の音

    // 結果表示用UIの参照 (Unityエディタで設定)
    [Header("Game Over UI")]
    public GameObject gameOverPanel; // 結果表示パネル
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalSurvivalTimeText;
    public TextMeshProUGUI finalBlocksDestroyedText;
    public TextMeshProUGUI finalMissTypesText;
    public TextMeshProUGUI bestScoreText; // 自己ベストスコア表示用
    public TextMeshProUGUI rankText;      // 順位表示用

    // ゲーム開始時にカウントダウンを表示するためのパネル
    [Header("Countdown UI")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText; // ← カウントダウン用テキスト

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
        Time.timeScale = 0f;
        StartCoroutine(StartCountdownAndGame());
    
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

    // カウントダウンとゲーム開始のコルーチン
    IEnumerator StartCountdownAndGame()
    {
        string[] countdownWords = { "3", "2", "1", "START" };

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        for (int i = 0; i < countdownWords.Length; i++)
        {
            if (countdownText != null)
                countdownText.text = countdownWords[i];

            // 音を再生
            SoundManager.Instance.PlaySound(startsound);
            yield return new WaitForSecondsRealtime(1f);
        }

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        Time.timeScale = 1f; // ゲームスタート！
        OnGameStarted?.Invoke(); // ゲーム開始イベントを通知
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
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameoversound); // ゲームオーバー音を再生
        
        // ゲームオーバー時に不要なUI��非表示にする
        if (UIPanel != null)
        {
            UIPanel.SetActive(false);
        }
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

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
            
            // PlayFabにスコアを送信
            SubmitScoreToPlayFab(score);
        }
    }

    // PlayFabにスコアを送信し、順位とベストスコアを取得する
    private void SubmitScoreToPlayFab(int score)
    {
        // ローディング表示などをここに入れると親切
        if (bestScoreText) bestScoreText.text = "自己ベスト: ---";
        if (rankText) rankText.text = "順位: ---";

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = LeaderboardName,
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdateStatisticsSuccess, OnError);
    }

    private void OnUpdateStatisticsSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully submitted score to PlayFab.");
        // スコア送信に成功したら、リーダーボードから自分の順位を取得
        FetchPlayerLeaderboardRank();
    }

    private void FetchPlayerLeaderboardRank()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = LeaderboardName,
            MaxResultsCount = 1 // 自分のデータだけ取得
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetLeaderboardAroundPlayerSuccess, OnError);
    }

    private void OnGetLeaderboardAroundPlayerSuccess(GetLeaderboardAroundPlayerResult result)
    {
        if (result.Leaderboard.Count > 0)
        {
            var playerEntry = result.Leaderboard[0];
            PlayerRank = playerEntry.Position + 1; // Positionは0ベースなので+1する
            PlayerBestScore = playerEntry.StatValue;

            Debug.Log($"Player Rank: {PlayerRank}, Best Score: {PlayerBestScore}");

            // UIを更新
            if (bestScoreText) bestScoreText.text = $"自己ベスト: {PlayerBestScore}";
            if (rankText) rankText.text = $"順位: {PlayerRank}位";
        }
        else
        {
            Debug.LogWarning("Could not find player on the leaderboard.");
            if (bestScoreText) bestScoreText.text = "自己ベスト: N/A";
            if (rankText) rankText.text = "順位: N/A";
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab API call failed: " + error.GenerateErrorReport());
        if (bestScoreText) bestScoreText.text = "自己ベスト: 取得失敗";
        if (rankText) rankText.text = "順位: 取得失敗";
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

        float oxygenPercentage = _currentOxygen / maxOxygen;

        // 【追加】fillRectTransformが設定されていれば、PosXを動かす処理を行う
        if (fillRectTransform != null)
        {
            // Lerpを使って割合をPosXの範囲(0 ~ -840)に変換
            // oxygenPercentageが1のとき0、0のとき-840になる
            float newPosX = Mathf.Lerp(-840f, 0f, oxygenPercentage);

            // RectTransformのX座標を更新（Y座標は元の値を維持）
            fillRectTransform.anchoredPosition = new Vector2(newPosX, fillRectTransform.anchoredPosition.y);
        }

        // 酸素残量に応じて色を変化
        if (fillImage != null)
        {
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
