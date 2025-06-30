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
    public float oxygenDecreaseRate = 1f;       // 1秒あたりに減る酸素量
    public Slider oxygenSlider;                 // 酸素ゲージUI

    private float _currentOxygen;               // 現在の酸素量
    private bool _isOxygenInvincible = false;   // 酸素減少無効グラフ
    public bool IsOxygenInvincible => _isOxygenInvincible;

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
        UpdateOxygenUI();
         // UIマネージャーなどに初期状態を通知する
        OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
    }

    void Update()
    {
        // 酸素減少無効でなければ酸素を減らす
        if (!_isOxygenInvincible)
        {
            // 経過時間に応じて酸素を時間で減らす
            float previousOxygen = _currentOxygen;
            _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
            _currentOxygen = Mathf.Max(_currentOxygen, 0); // 0未満にならないように
            
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
    }

    // 一時的に酸素量の減少を無効化する（アイテム取得時に呼ばれる）
    public System.Collections.IEnumerator TemporaryOxygenInvincibility(float duration)
    {
        _isOxygenInvincible = true;
        yield return new WaitForSeconds(duration);
        _isOxygenInvincible = false;
    }
}