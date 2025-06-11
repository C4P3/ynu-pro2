using UnityEngine;
using UnityEngine.UI; // Sliderを使う場合
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトン

    [Header("Oxygen")]
    public float maxOxygen = 100f;
    public float oxygenDecreaseRate = 1f; // 1秒あたりに減る酸素量
    public Slider oxygenSlider; // 酸素ゲージUI

    private float _currentOxygen;

    void Awake()
    {
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
        _currentOxygen = maxOxygen;
        UpdateOxygenUI();
    }

    void Update()
    {
        // 酸素を時間で減らす
        _currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
        _currentOxygen = Mathf.Max(_currentOxygen, 0); // 0未満にならないように
        UpdateOxygenUI();

        if (_currentOxygen <= 0)
        {
            Debug.Log("ゲームオーバー");
            // ここにゲームオーバー処理（リザルト画面表示など）
            Time.timeScale = 0; // 時間を止める
        }
    }
    
    // 酸素を回復する（アイテム取得時に呼ばれる）
    public void RecoverOxygen(float amount)
    {
        _currentOxygen += amount;
        _currentOxygen = Mathf.Min(_currentOxygen, maxOxygen); // 最大値を超えないように
        UpdateOxygenUI();
    }

    void UpdateOxygenUI()
    {
        if (oxygenSlider != null)
        {
            oxygenSlider.value = _currentOxygen / maxOxygen;
        }
    }
}