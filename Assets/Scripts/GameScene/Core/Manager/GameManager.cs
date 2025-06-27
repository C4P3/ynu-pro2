using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Oxygen")]
    public float maxOxygen = 100f;
    public float oxygenDecreaseRate = 1f;
    public Slider oxygenSlider;
    
    // privateなフィールドでUIの参照を保持する
    private Slider _oxygenSlider; 

    private float _currentOxygen;
    private bool _isOxygenInvincible = false;
    public bool IsOxygenInvincible => _isOxygenInvincible;

    void Awake()
    {
        // 【修正】マルチプレイヤー対応のため、シングルトンの自己破棄ロジックを修正
        Instance = this;
    }

    void Start()
    {
        _currentOxygen = maxOxygen;
        UpdateOxygenUI();
    }

    void Update()
    {
        if (_isOxygenInvincible) return;

        _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
        _currentOxygen = Mathf.Max(_currentOxygen, 0);
        UpdateOxygenUI();

        if (_currentOxygen <= 0)
        {
            Debug.Log("ゲームオーバー");
            Time.timeScale = 0;
        }
    }

    public void RecoverOxygen(float amount)
    {
        _currentOxygen += amount;
        _currentOxygen = Mathf.Min(_currentOxygen, maxOxygen);
        UpdateOxygenUI();
    }

    /// <summary>
    /// 【重要】初期化役から呼び出され、このManagerが操作するUIスライダーを設定する
    /// </summary>
    public void SetOxygenSlider(Slider slider)
    {
        _oxygenSlider = slider;
        UpdateOxygenUI(); // 受け取った直後にUIを最新の状態に更新
    }

    void UpdateOxygenUI()
    {
        if (_oxygenSlider != null)
        {
            _oxygenSlider.value = _currentOxygen / maxOxygen;
        }
    }

    public System.Collections.IEnumerator TemporaryOxygenInvincibility(float duration)
    {
        _isOxygenInvincible = true;
        yield return new WaitForSeconds(duration);
        _isOxygenInvincible = false;
    }
}