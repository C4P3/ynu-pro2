using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトン

    [Header("Oxygen")]
    public float maxOxygen = 100f;              // 最大酸素量
    public float oxygenDecreaseRate = 1f;       // 1秒あたりに減る酸素量
    public Slider oxygenSlider;                 // 酸素ゲージUI

    public PlayerController LocalPlayer { get; private set; }

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
    }

    void Update()
    {
        // 酸素減少無効でなければ酸素を減らす
        if (!_isOxygenInvincible)
        {
            // 経過時間に応じて酸素を時間で減らす
            _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
            _currentOxygen = Mathf.Max(_currentOxygen, 0); // 0未満にならないように
            UpdateOxygenUI();
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

    /// <summary>
    /// ローカルプレイヤーをGameManagerに登録する
    /// </summary>
    /// <param name="player">登録するプレイヤーのPlayerController</param>
    public void RegisterLocalPlayer(PlayerController player)
    {
        LocalPlayer = player;
        Debug.Log($"Local Player '{player.name}' has been registered.");
    }
}