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

    [Header("Oxygen Bar Colors")]
    public Color fullOxygenColor = Color.green;     // 満タン時の色 (黄緑)
    public Color lowOxygenColor = Color.yellow;     // 30%以下になった時の色
    public Color criticalOxygenColor = Color.red;   // 10%以下になった時の色
    private Image fillImage;                        // ゲージの色を変更するためのImageコンポーネント

    [Header("Survival Time")]
    public TextMeshProUGUI survivalTimeText;        // 生存時間を表示するTextMeshProUGUIコンポーネント
    public PlayerController LocalPlayer { get; private set; }

    private float _currentOxygen;               // 現在の酸素量
    private bool _isOxygenInvincible = false;   // 酸素減少無効グラフ
    public bool IsOxygenInvincible => _isOxygenInvincible;

    private float _survivalTime = 0f;           // 生存時間
    private bool _gameEnded = false;            // ゲーム終了フラグ

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
        _gameEnded = false;
        UpdateSurvivalTimeUI();
    }

    void Update()
    {
        // ゲームが終了している場合は何もしない
        if (_gameEnded) return;

        // ゲームがプレイ中でなければ酸素を減らさない
        bool isPlaying = false;
        if (GameDataSync.Instance != null) // マルチプレイか確認
        {
            isPlaying = GameDataSync.Instance.currentState == GameState.Playing;
        }
        else // シングルプレイの場合
        {
            isPlaying = true; // シングルでは常にプレイ中とみなす
        }

        if (isPlaying)
        {
            // 生存時間を計測
            _survivalTime += Time.deltaTime;
            UpdateSurvivalTimeUI();

            if (!_isOxygenInvincible) // 酸素減少無効化が有効でない場合のみ酸素を減らす
            {
                // 経過時間に応じて酸素を時間で減らす
                float previousOxygen = _currentOxygen;
                _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
                _currentOxygen = Mathf.Max(_currentOxygen, 0);

                // 値が変化した場合のみUI更新とイベント発行を行う
                if (!Mathf.Approximately(previousOxygen, _currentOxygen))
                {
                    UpdateOxygenUI();
                    OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
                }
                // 酸素が0になったらゲームオーバー
                if (_currentOxygen <= 0)
                {
                    Debug.Log("ゲームオーバー");
                    // ここにゲームオーバー処理（リザルト画面表示など）
                    Time.timeScale = 0; // 時間を止める
                    _gameEnded = true;
                    Debug.Log($"最終生存時間: {FormatTime(_survivalTime)}");
                }
            }
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
    
    // 生存時間UIを更新する
    private void UpdateSurvivalTimeUI()
    {
        if (survivalTimeText != null)
        {
            survivalTimeText.text = FormatTime(_survivalTime);
        }
    }

    // 秒数を「分:秒.ミリ秒」形式にフォーマットする
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100); // 10ミリ秒単位で表示
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    // 最終的な生存時間を取得する
    public float GetSurvivalTime()
    {
        return _survivalTime;
    }
}